using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using Game.Features.Network.Scripts;
using Game.Scripts.Core.Battle;
using UnityEngine;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Data.Cards;
using Game.Scripts.Core.Matchmaking;
using Game.Scripts.Core.Networking;
using Game.Scripts.Core.Scenes;
using Game.Scripts.Infrastructure.Battle.Session;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Infrastructure.Networking
{
    public sealed class FusionMatchmakingService : IMatchmakingService, IDisposable
    {
        private readonly INetworkSession _networkSession;
        private readonly IPlayerRepository _playerRepository;
        private readonly IDeckService _deckService;
        private readonly CardCatalog _cardCatalog;
        private readonly BattleModeConfigAsset _battleModeConfig;
        private readonly GamePrefabCatalog _prefabCatalog;

        private CancellationTokenSource _battleEntryCts;
        private int _isEnteringBattle;
        private int _hasEnteredMatch;

        public FusionMatchmakingService(INetworkSession networkSession, IPlayerRepository playerRepository,
            IDeckService deckService, CardCatalog cardCatalog, BattleModeConfigAsset battleModeConfig,
            GamePrefabCatalog prefabCatalog)
        {
            _networkSession = networkSession;
            _playerRepository = playerRepository;
            _deckService = deckService;
            _cardCatalog = cardCatalog;
            _battleModeConfig = battleModeConfig;
            _prefabCatalog = prefabCatalog;
            _networkSession.PlayerCountChanged += HandlePlayerCountChanged;
            _networkSession.StateChanged += HandleSessionStateChanged;
        }

        public MatchmakingState State { get; private set; } = MatchmakingState.Idle;

        public string StatusMessage { get; private set; } = "Tap PLAY to find an opponent.";

        public event Action<MatchmakingState> StateChanged;
        public event Action<string> StatusMessageChanged;

        public void Dispose()
        {
            _networkSession.PlayerCountChanged -= HandlePlayerCountChanged;
            _networkSession.StateChanged -= HandleSessionStateChanged;
            _battleEntryCts?.Cancel();
            _battleEntryCts?.Dispose();
        }

        public async UniTask StartQuickMatchAsync(CancellationToken cancellationToken = default)
        {
            await PrepareForSearchAsync(cancellationToken);

            if (State is MatchmakingState.InMatch)
                return;

            SetState(MatchmakingState.Connecting, "Connecting to Photon...");

            try
            {
                var request = BuildQuickMatchRequest(_playerRepository.Profile);
                await _networkSession.ConnectAsync(request, cancellationToken);
                HandlePostConnect(_networkSession.PlayerCount);
            }
            catch (OperationCanceledException)
            {
                HandleQuickMatchCanceled();
                throw;
            }
            catch (Exception exception)
            {
                HandleQuickMatchFailed(exception);
                throw;
            }
        }

        public async UniTask PrepareForSearchAsync(CancellationToken cancellationToken = default)
        {
            if (State is MatchmakingState.Idle or MatchmakingState.Failed)
                return;

            if (State == MatchmakingState.InMatch)
                return;

            await CancelAsync(cancellationToken);
        }

        public async UniTask ExitMatchAsync(CancellationToken cancellationToken = default) =>
            await DisconnectAndResetAsync(cancellationToken, cancelBattleEntry: true);

        public async UniTask CancelAsync(CancellationToken cancellationToken = default)
        {
            if (State == MatchmakingState.Idle)
                return;

            await DisconnectAndResetAsync(cancellationToken);
        }

        private async UniTask DisconnectAndResetAsync(CancellationToken cancellationToken, bool cancelBattleEntry = false)
        {
            if (cancelBattleEntry)
                _battleEntryCts?.Cancel();

            try
            {
                if (State != MatchmakingState.Idle)
                    await _networkSession.DisconnectAsync(cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.LogException(exception);
            }
            finally
            {
                Interlocked.Exchange(ref _hasEnteredMatch, 0);
                SetState(MatchmakingState.Idle, "Tap PLAY to find an opponent.");
            }
        }

        private NetworkSessionRequest BuildQuickMatchRequest(PlayerProfile profile)
        {
            var payload = BuildLocalPayload(profile);
            return new NetworkSessionRequest(NetworkGameMode.AutoHostOrClient, sessionName: null,
                playerId: profile.PlayerId, displayName: profile.DisplayName,
                maxPlayers: MatchmakingConstants.RequiredPlayers, sessionPayload: payload);
        }

        private BattleSessionPayload BuildLocalPayload(PlayerProfile profile)
        {
            var battleMode = _battleModeConfig.CreateConfig();
            return new BattleSessionPayload(
                PlayerProfile.CreateBattleDefault(profile.PlayerId, profile.DisplayName, battleMode.MaxHp),
                _deckService.GetDeck(_deckService.SelectedDeckIndex),
                _cardCatalog,
                battleMode);
        }

        private void HandlePostConnect(int playerCount)
        {
            if (playerCount >= MatchmakingConstants.RequiredPlayers)
                EnterMatch();
            else
                SetState(MatchmakingState.WaitingForOpponent, FormatWaitingMessage(playerCount));
        }

        private void HandleQuickMatchCanceled() =>
            SetState(MatchmakingState.Idle, "Tap PLAY to find an opponent.");

        private void HandleQuickMatchFailed(Exception exception)
        {
            if (exception is not OperationCanceledException)
                Debug.LogException(exception);

            SetState(MatchmakingState.Failed, "Could not connect. Try again.");
        }

        private void HandlePlayerCountChanged(int playerCount)
        {
            if (State == MatchmakingState.WaitingForOpponent)
                SetStatusMessage(FormatWaitingMessage(playerCount));

            if (playerCount >= MatchmakingConstants.RequiredPlayers
                && State is MatchmakingState.WaitingForOpponent or MatchmakingState.Connecting)
                EnterMatch();
        }

        private void HandleSessionStateChanged(NetworkSessionState sessionState)
        {
            if (sessionState != NetworkSessionState.Disconnected)
                return;

            if (State is MatchmakingState.Idle or MatchmakingState.Failed)
                return;

            SetState(MatchmakingState.Failed, "Connection lost. Try again.");
        }

        private void EnterMatch()
        {
            if (Interlocked.CompareExchange(ref _hasEnteredMatch, 1, 0) != 0)
                return;

            SetState(MatchmakingState.InMatch, "Opponent found!");

            _battleEntryCts?.Cancel();
            _battleEntryCts?.Dispose();
            _battleEntryCts = new CancellationTokenSource();
            EnterBattleAsync(_battleEntryCts.Token).Forget(Debug.LogException);
        }

        private async UniTask EnterBattleAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _isEnteringBattle, 1, 0) != 0)
                return;

            try
            {
                if (_networkSession is not FusionNetworkSessionService fusionSession)
                    return;

                var runner = fusionSession.ActiveRunner;
                if (runner == null || !runner.IsRunning)
                {
                    FailBattleEntry("Could not start battle.");
                    return;
                }

                if (runner.IsServer)
                {
                    await LoadBattleSceneAsync(runner, cancellationToken);
                    SpawnBattleSession(runner);
                }
                else
                {
                    await WaitForBattleSceneAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                FailBattleEntry("Could not start battle.");
            }
            finally
            {
                Interlocked.Exchange(ref _isEnteringBattle, 0);
            }
        }

        private void FailBattleEntry(string message)
        {
            Interlocked.Exchange(ref _hasEnteredMatch, 0);
            SetState(MatchmakingState.Failed, message);
        }

        private static async UniTask LoadBattleSceneAsync(NetworkRunner runner, CancellationToken cancellationToken)
        {
            var sceneRef = FusionSceneRefs.FromGameScene(GameSceneId.Battle);
            var operation = runner.LoadScene(
                sceneRef,
                LoadSceneMode.Single,
                LocalPhysicsMode.None,
                setActiveOnLoad: true);

            await operation.ToUniTask(cancellationToken: cancellationToken);

            if (operation.Error != null)
                throw new InvalidOperationException($"Failed to load battle scene: {operation.Error}");
        }

        private static async UniTask WaitForBattleSceneAsync(CancellationToken cancellationToken)
        {
            var battleSceneIndex = (int)GameSceneId.Battle;
            await UniTask.WaitUntil(() => IsBattleSceneLoaded(battleSceneIndex), cancellationToken: cancellationToken);
        }

        private static bool IsBattleSceneLoaded(int buildIndex)
        {
            var scene = SceneManager.GetSceneByBuildIndex(buildIndex);
            return scene.IsValid() && scene.isLoaded;
        }

        private void SpawnBattleSession(NetworkRunner runner)
        {
            var prefab = _prefabCatalog.BattleSessionPrefab;
            if (prefab == null)
                throw new InvalidOperationException("BattleSessionPrefab is not assigned in GamePrefabCatalog.");

            runner.Spawn(prefab);
        }

        private static string FormatWaitingMessage(int playerCount) =>
            $"Waiting for opponent ({playerCount}/{MatchmakingConstants.RequiredPlayers})...";

        private void SetState(MatchmakingState newState, string statusMessage)
        {
            SetStatusMessage(statusMessage);

            if (State == newState) return;
            State = newState;
            StateChanged?.Invoke(newState);
        }

        private void SetStatusMessage(string statusMessage)
        {
            if (StatusMessage == statusMessage)
                return;

            StatusMessage = statusMessage;
            StatusMessageChanged?.Invoke(statusMessage);
        }
    }
}
