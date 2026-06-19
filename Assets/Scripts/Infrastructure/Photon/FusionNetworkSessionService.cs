using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cards;
using Core.Battle;
using Core.Data;
using Core.Networking;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;

namespace Infrastructure.Photon
{
    public sealed class FusionNetworkSessionService
        : INetworkSession,
            IFusionRunnerAccessor,
            INetworkRunnerCallbacks,
            IDisposable
    {
        private readonly FusionNetworkRunnerFactory _runnerFactory;
        private readonly IBattleControllerRegistry _battleControllerRegistry;
        private readonly IBattleLoadoutProvider _battleLoadout;
        private readonly CardCatalog _cardCatalog;

        private NetworkRunner _runner;

        public FusionNetworkSessionService(
            FusionNetworkRunnerFactory runnerFactory,
            IBattleControllerRegistry battleControllerRegistry,
            IBattleLoadoutProvider battleLoadout,
            CardCatalog cardCatalog)
        {
            _runnerFactory = runnerFactory;
            _battleControllerRegistry = battleControllerRegistry;
            _battleLoadout = battleLoadout;
            _cardCatalog = cardCatalog;
        }

        public NetworkSessionState State { get; private set; } = NetworkSessionState.Disconnected;

        public int PlayerCount => _runner == null ? 0 : _runner.ActivePlayers.Count();

        public NetworkRunner ActiveRunner => _runner;

        public event Action<int> PlayerCountChanged;

        public async UniTask ConnectAsync(NetworkSessionRequest request, CancellationToken cancellationToken = default)
        {
            if (State is NetworkSessionState.Connecting or NetworkSessionState.Connected)
            {
                throw new InvalidOperationException($"Cannot connect while session state is {State}.");
            }

            SetState(NetworkSessionState.Connecting);

            try
            {
                var payload = new BattleSessionPayload(
                    PlayerProfile.CreateBattleDefault(request.PlayerId, request.DisplayName),
                    _battleLoadout.SelectedDeck,
                    _cardCatalog);

                _runner = _runnerFactory.CreateRunner();
                _runner.GetComponent<FusionSessionBridge>()
                    ?.Initialize(_battleControllerRegistry, payload);
                _runner.AddCallbacks(this);

                var sceneInfo = new NetworkSceneInfo();
                if (request.InitialScene.HasValue)
                {
                    var sceneRef = SceneRef.FromIndex((int)request.InitialScene.Value);
                    if (sceneRef.IsValid)
                    {
                        sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);
                    }
                }

                var result = await _runner.StartGame(new StartGameArgs
                {
                    GameMode = FusionGameModeMapper.ToFusion(request.GameMode),
                    SessionName = request.SessionName,
                    Scene = sceneInfo,
                    SceneManager = _runner.GetComponent<INetworkSceneManager>(),
                    ObjectProvider = _runner.GetComponent<INetworkObjectProvider>(),
                    ConnectionToken = ProfileConnectionToken.Encode(request.PlayerId, request.DisplayName)
                });

                cancellationToken.ThrowIfCancellationRequested();

                if (!result.Ok)
                {
                    await CleanupRunnerAsync();
                    SetState(NetworkSessionState.Disconnected);
                    throw new NetworkSessionException(
                        result.ShutdownReason.ToString(),
                        $"Failed to start Fusion session: {result.ShutdownReason}");
                }

                SetState(NetworkSessionState.Connected);
                NotifyPlayerCountChanged();
            }
            catch (OperationCanceledException)
            {
                await CleanupRunnerAsync();
                SetState(NetworkSessionState.Disconnected);
                throw;
            }
            catch
            {
                await CleanupRunnerAsync();
                SetState(NetworkSessionState.Disconnected);
                throw;
            }
        }

        public async UniTask DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (State is NetworkSessionState.Disconnected or NetworkSessionState.Disconnecting)
            {
                return;
            }

            SetState(NetworkSessionState.Disconnecting);
            await CleanupRunnerAsync();
            cancellationToken.ThrowIfCancellationRequested();
            SetState(NetworkSessionState.Disconnected);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (_runner == runner)
            {
                _runner = null;
            }

            SetState(NetworkSessionState.Disconnected);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) => NotifyPlayerCountChanged();

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => NotifyPlayerCountChanged();

        public void Dispose()
        {
            var runner = DetachRunner();
            if (runner == null)
            {
                return;
            }

            if (runner.IsRunning)
            {
                _ = runner.Shutdown();
            }

            UnityEngine.Object.Destroy(runner.gameObject);
        }

        private async UniTask CleanupRunnerAsync()
        {
            var runner = DetachRunner();
            if (runner == null)
            {
                return;
            }

            if (runner.IsRunning)
            {
                await runner.Shutdown();
            }

            UnityEngine.Object.Destroy(runner.gameObject);
        }

        private NetworkRunner DetachRunner()
        {
            if (_runner == null)
            {
                return null;
            }

            var runner = _runner;
            _runner = null;
            runner.RemoveCallbacks(this);
            return runner;
        }

        private void NotifyPlayerCountChanged() => PlayerCountChanged?.Invoke(PlayerCount);

        private void SetState(NetworkSessionState newState)
        {
            if (State != newState)
            {
                State = newState;
            }
        }

        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        void INetworkRunnerCallbacks.OnConnectRequest(
            NetworkRunner runner,
            NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        { }
        void INetworkRunnerCallbacks.OnConnectFailed(
            NetworkRunner runner,
            NetAddress remoteAddress,
            NetConnectFailedReason reason)
        { }
        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(
            NetworkRunner runner,
            Dictionary<string, object> data)
        { }
        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        void INetworkRunnerCallbacks.OnReliableDataReceived(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            ArraySegment<byte> data)
        { }
        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    }
}
