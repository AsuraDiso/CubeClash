using System;
using System.Collections.Generic;
using Fusion;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Actions;
using Game.Scripts.Core.Battle.Actions.Card;
using Game.Scripts.Core.Battle.Simulation;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Data.Cards;
using Game.Scripts.Core.Networking;
using Game.Scripts.Core.Matchmaking;
using Game.Scripts.Infrastructure.Networking;

namespace Game.Scripts.Infrastructure.Battle.Network
{
    public sealed class BattleNetworkController : NetworkBehaviour, IBattleGateway
    {
        private const int MaxPlayers = BattleState.MaxPlayers;

        [Networked] public int BattleModeId { get; private set; }
        [Networked] public int CurrentTurnPlayerId { get; private set; }
        [Networked] public int WinnerNetworkId { get; private set; }
        [Networked] public int TurnDiceConsumedMask { get; private set; }
        [Networked] public NetworkBool MatchInitialized { get; private set; }
        [Networked] public NetworkBool IsGameOver { get; private set; }

        [Networked, Capacity(BattleNetworkStateSync.MaxDiceCount)]
        public NetworkArray<int> TurnDiceValues => default;

        [Networked, Capacity(MaxPlayers)]
        public NetworkArray<int> PlayerNetworkIds => default;

        [Networked, Capacity(MaxPlayers)]
        public NetworkArray<NetworkPlayerProfile> PlayerProfiles => default;

        public event Action TurnChanged;
        public event Action ProfilesUpdated;
        public event Action DecksUpdated;
        public event Action<bool> GameOver;
        public event Action<BattleActionResolvedEventArgs> ActionResolved;
        public event Action ActionFailed;

        private readonly BattleMatchInitializer _matchInitializer = new();
        private readonly BattleDeckStore _deckStore = new();
        private readonly UnityBattleRandom _battleRandom = new();
        private readonly HashSet<int> _activeIdCache = new();

        private IBattleControllerRegistry _controllerRegistry;
        private FusionSessionBridge _bridge;
        private BattleEngine _engine;
        private BattleModeConfig _modeConfig;
        private CardCatalog _cardCatalog;
        private bool _gameOverNotified;
        private bool _decksDirty;
        private bool _turnDirty;
        private bool _profilesDirty;
        private BattleActionResolvedEventArgs _pendingActionResolved;
        private bool _actionFailedPending;
        private bool _hasPresentationSnapshot;
        private bool _hasSeenRequiredPlayersBeforeInit;
        private BattlePresentationSnapshot _previousPresentation;
        private readonly int[] _currentPlayerNetworkIds = new int[MaxPlayers];
        private readonly int[] _currentTurnDiceValues = new int[BattleNetworkStateSync.MaxDiceCount];
        private readonly NetworkPlayerProfile[] _currentPlayerProfiles = new NetworkPlayerProfile[MaxPlayers];
        private readonly int[] _previousPlayerNetworkIds = new int[MaxPlayers];
        private readonly int[] _previousTurnDiceValues = new int[BattleNetworkStateSync.MaxDiceCount];
        private readonly NetworkPlayerProfile[] _previousPlayerProfiles = new NetworkPlayerProfile[MaxPlayers];

        int IBattleGateway.BattleModeId => BattleModeId;
        int IBattleGateway.MaxHp => _modeConfig.MaxHp;
        int IBattleGateway.DiceCount => _modeConfig.DiceCount;
        int IBattleGateway.DiceMin => _modeConfig.DiceMin;
        int IBattleGateway.DiceMax => _modeConfig.DiceMax;
        public bool IsMyTurn => MatchInitialized && !IsGameOver && CurrentTurnPlayerId == Runner.LocalPlayer.PlayerId;
        int IBattleGateway.WinnerNetworkId => WinnerNetworkId;
        public bool IsMatchReady => MatchInitialized;
        public PlayerProfile LocalProfile => MatchInitialized
            ? BattlePlayerSlots.GetProfile(PlayerNetworkIds, PlayerProfiles, Runner.LocalPlayer.PlayerId)
            : _bridge.Payload.LocalProfile;
        public PlayerProfile OpponentProfile
        {
            get
            {
                if (MatchInitialized)
                {
                    var opponentId = BattlePlayerSlots.GetOpponentNetworkId(PlayerNetworkIds, Runner.LocalPlayer.PlayerId);
                    if (opponentId != BattleState.UnassignedNetworkId)
                        return BattlePlayerSlots.GetProfile(PlayerNetworkIds, PlayerProfiles, opponentId);
                }

                return TryResolveOpponentPreview() ?? PlayerProfile.Unknown;
            }
        }
        public IReadOnlyList<PlacedCard> LocalDeck => _deckStore.LocalDeck;
        public IReadOnlyList<PlacedCard> OpponentDeck => _deckStore.OpponentDeck;

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || IsGameOver)
                return;

            if (!MatchInitialized)
            {
                TryForfeitIfOpponentLeftBeforeInit();
                if (HasStateAuthority)
                    TryInitializeMatch();

                return;
            }

            TryForfeitDisconnectedPlayer();
        }

        public override void Spawned()
        {
            _bridge = Runner.GetComponent<FusionSessionBridge>();
            _controllerRegistry = _bridge.BattleControllerRegistry;
            _modeConfig = _bridge.Payload.BattleModeConfig;
            _cardCatalog = _bridge.Payload.CardCatalog;

            if (HasStateAuthority)
                _engine = new BattleEngine(_modeConfig, _battleRandom);

            var localDeck = _bridge.Payload.LocalDeck;
            _deckStore.SetLocalPreview(localDeck);
            _decksDirty = true;

            var packedDeck = DeckRecordConverter.PackNetwork(DeckRecordConverter.FromPlacedCards(localDeck));
            if (HasStateAuthority)
                SubmitDeckOnAuthority(Runner.LocalPlayer.PlayerId, packedDeck);
            else
                TrySubmitLocalDeckToAuthority(packedDeck);

            if (HasStateAuthority)
                TryInitializeMatch();

            _controllerRegistry.Register(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _controllerRegistry.Unregister(this);
            _controllerRegistry = null;
            _engine = null;
        }

        public override void Render()
        {
            var current = CapturePresentationSnapshot();
            var snapshotChanged = !_hasPresentationSnapshot || !current.Equals(_previousPresentation);

            if (snapshotChanged)
            {
                var matchReadyChanged = !_hasPresentationSnapshot
                    || _previousPresentation.MatchInitialized != current.MatchInitialized;
                var turnChanged = !_hasPresentationSnapshot || _previousPresentation.TurnChangedFrom(current);
                var profilesChanged = !_hasPresentationSnapshot
                    || matchReadyChanged
                    || _previousPresentation.ProfilesChangedFrom(current);
                var gameOverChanged = !_hasPresentationSnapshot
                    || _previousPresentation.GameOverChangedFrom(current);

                CopyPresentationSnapshot(current, _previousPlayerNetworkIds, _previousTurnDiceValues,
                    _previousPlayerProfiles);
                _previousPresentation = CreatePresentationSnapshot(
                    current,
                    _previousPlayerNetworkIds,
                    _previousTurnDiceValues,
                    _previousPlayerProfiles);
                _hasPresentationSnapshot = true;

                if (turnChanged || matchReadyChanged)
                    _turnDirty = true;

                if (profilesChanged || matchReadyChanged)
                    _profilesDirty = true;

                if (gameOverChanged || profilesChanged)
                    CheckGameOver();
            }

            FlushPresentationEvents();
        }

        private void FlushPresentationEvents()
        {
            if (_turnDirty)
            {
                _turnDirty = false;
                TurnChanged?.Invoke();
            }

            if (_profilesDirty)
            {
                _profilesDirty = false;
                ProfilesUpdated?.Invoke();
            }

            if (_decksDirty)
            {
                _decksDirty = false;
                DecksUpdated?.Invoke();
            }

            if (_pendingActionResolved != null)
            {
                var resolved = _pendingActionResolved;
                _pendingActionResolved = null;
                ActionResolved?.Invoke(resolved);
            }

            if (_actionFailedPending)
            {
                _actionFailedPending = false;
                ActionFailed?.Invoke();
            }
        }

        public int GetTurnDiceValue(int dieIndex) =>
            BattleNetworkStateSync.GetTurnDiceValue(TurnDiceValues, dieIndex);

        public bool IsTurnDiceConsumed(int dieIndex) => (TurnDiceConsumedMask & (1 << dieIndex)) != 0;

        public bool TryValidateAction(IBattleAction action)
        {
            if (!IsMyTurn || action == null)
                return false;

            if (HasStateAuthority && _engine != null)
                return _engine.CanExecuteAction(Runner.LocalPlayer.PlayerId, action);

            if (action is CardBattleAction card)
                return ValidateCardActionClient(card);

            return false;
        }

        public void SubmitAction(IBattleAction action)
        {
            if (!Object.IsValid || !IsMyTurn || action == null)
                return;

            if (!BattleActionCodec.TryEncode(action, out var typeId, out var payload))
                return;

            RPC_RequestAction(typeId, payload);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_SubmitDeck(int[] packedDeck, RpcInfo info = default) =>
            SubmitDeckOnAuthority(info.Source.PlayerId, packedDeck);

        private void SubmitDeckOnAuthority(int playerId, int[] packedDeck)
        {
            if (!HasStateAuthority || packedDeck == null || packedDeck.Length > 100)
                return;

            _matchInitializer.SubmitDeck(playerId, packedDeck);
            RPC_ApplyPlayerDeck(playerId, packedDeck);
            TryInitializeMatch();
        }

        private void TrySubmitLocalDeckToAuthority(int[] packedDeck = null)
        {
            if (HasStateAuthority || _bridge == null)
                return;

            packedDeck ??= DeckRecordConverter.PackNetwork(
                DeckRecordConverter.FromPlacedCards(_bridge.Payload.LocalDeck));

            RPC_SubmitDeck(packedDeck);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ApplyPlayerDeck(int networkPlayerId, int[] packedDeck)
        {
            if (packedDeck == null || _cardCatalog == null || Runner == null)
                return;

            var deck = DeckRecordConverter.ToPlacedCardsFromNetworkPack(packedDeck, _cardCatalog);
            _deckStore.ApplyDeck(networkPlayerId, Runner.LocalPlayer.PlayerId, deck);
            _decksDirty = true;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestAction(string actionTypeId, int[] payload, RpcInfo info = default)
        {
            if (_engine.State.IsGameOver)
                return;

            if (!BattleActionCodec.TryDecode(actionTypeId, payload, out var action))
            {
                RPC_NotifyActionFailed(info.Source);
                return;
            }

            var result = _engine.TryExecuteAction(info.Source.PlayerId, action);
            if (!result.Success)
            {
                RPC_NotifyActionFailed(info.Source);
                return;
            }

            ApplySimulationToNetwork(_engine.State);

            if (!BattleActionCodec.TryEncodeResult(result, out var resultPayload))
                resultPayload = Array.Empty<int>();

            RPC_NotifyActionResolved(actionTypeId, result.ActorNetworkId, result.ActorDisplayName, resultPayload,
                result.TurnEnded);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyActionFailed([RpcTarget] PlayerRef player) => _actionFailedPending = true;

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyActionResolved(string actionTypeId, int actorId, string actorLabel, int[] resultPayload,
            bool turnEnded)
        {
            IBattleAction action = null;
            BattleActionResult result = BattleActionResult.Failed;

            if (actionTypeId == BattleActionTypes.CardResolve
                && BattleActionCodec.TryDecodeCardResult(resultPayload, out var deckCardIndex, out var damage,
                    out var consumedDice))
            {
                action = new CardBattleAction(deckCardIndex, consumedDice);
                result = new CardBattleActionResult(actorId, actorLabel, deckCardIndex, damage, consumedDice);
                if (turnEnded)
                    result = result.WithTurnEnded();
            }

            if (action == null || !result.Success)
                return;

            _pendingActionResolved = new BattleActionResolvedEventArgs(action, result,
                Runner.LocalPlayer.PlayerId == actorId);
        }

        private bool ValidateCardActionClient(CardBattleAction card)
        {
            var diceCount = _modeConfig.DiceCount;
            var turnDiceValues = new int[diceCount];
            var turnDiceConsumed = new bool[diceCount];

            for (var i = 0; i < diceCount; i++)
            {
                turnDiceValues[i] = GetTurnDiceValue(i);
                turnDiceConsumed[i] = IsTurnDiceConsumed(i);
            }

            var deckCardIndex = card.DeckCardIndex;
            if (deckCardIndex < 0 || deckCardIndex >= LocalDeck.Count)
                return false;

            var definition = LocalDeck[deckCardIndex].Definition;
            return DiceAssignmentValidator.TryValidateTurnDice(
                card.DieIndices,
                definition.GetFlatRequirements(),
                turnDiceValues,
                turnDiceConsumed,
                out _);
        }

        private void TryInitializeMatch()
        {
            if (!HasStateAuthority)
                return;

            if (!_matchInitializer.TryInitialize(
                    Runner,
                    _modeConfig,
                    _cardCatalog,
                    _engine,
                    MatchInitialized,
                    ApplySimulationToNetwork,
                    BroadcastDecks))
                return;

            _profilesDirty = true;
            _turnDirty = true;
        }

        private void BroadcastDecks()
        {
            if (!HasStateAuthority)
                return;

            foreach (var player in Runner.ActivePlayers)
            {
                var slot = _engine.State.FindSlotByNetworkId(player.PlayerId);
                if (slot < 0)
                    continue;

                RPC_ApplyPlayerDeck(player.PlayerId,
                    DeckRecordConverter.PackNetwork(
                        DeckRecordConverter.FromPlacedCards(_engine.State.PlayerDecks[slot])));
            }
        }

        private void CheckGameOver()
        {
            if (!MatchInitialized || !IsGameOver || _gameOverNotified)
                return;

            _gameOverNotified = true;
            GameOver?.Invoke(Runner.LocalPlayer.PlayerId == WinnerNetworkId);
        }

        private void TryForfeitIfOpponentLeftBeforeInit()
        {
            var activeCount = 0;
            PlayerRef remaining = default;

            foreach (var player in Runner.ActivePlayers)
            {
                activeCount++;
                remaining = player;
            }

            if (activeCount >= MatchmakingConstants.RequiredPlayers)
            {
                _hasSeenRequiredPlayersBeforeInit = true;
                return;
            }

            if (!_hasSeenRequiredPlayersBeforeInit || activeCount != 1)
                return;

            IsGameOver = true;
            WinnerNetworkId = remaining.PlayerId;
            MatchInitialized = true;
            CurrentTurnPlayerId = remaining.PlayerId;
        }

        private void TryForfeitDisconnectedPlayer()
        {
            _activeIdCache.Clear();
            foreach (var player in Runner.ActivePlayers)
                _activeIdCache.Add(player.PlayerId);

            for (var i = 0; i < MaxPlayers; i++)
            {
                var networkId = PlayerNetworkIds.Get(i);
                if (networkId == BattleState.UnassignedNetworkId || _activeIdCache.Contains(networkId))
                    continue;

                _engine.ForfeitPlayer(networkId);
                ApplySimulationToNetwork(_engine.State);
                return;
            }
        }

        private void ApplySimulationToNetwork(BattleState state)
        {
            BattleModeId = state.BattleModeId;
            CurrentTurnPlayerId = state.CurrentTurnPlayerId;
            WinnerNetworkId = state.WinnerNetworkId;
            MatchInitialized = state.IsInitialized;
            IsGameOver = state.IsGameOver;
            TurnDiceConsumedMask = BattleNetworkStateSync.BuildConsumedMask(state);

            for (var i = 0; i < BattleNetworkStateSync.MaxDiceCount; i++)
                TurnDiceValues.Set(i, i < state.TurnDiceValues.Length ? state.TurnDiceValues[i] : 0);

            for (var i = 0; i < MaxPlayers; i++)
            {
                PlayerNetworkIds.Set(i, state.PlayerNetworkIds[i]);
                PlayerProfiles.Set(i, new NetworkPlayerProfile(state.Profiles[i]));
            }
        }

        private BattlePresentationSnapshot CapturePresentationSnapshot()
        {
            for (var i = 0; i < MaxPlayers; i++)
            {
                _currentPlayerNetworkIds[i] = PlayerNetworkIds.Get(i);
                _currentPlayerProfiles[i] = PlayerProfiles.Get(i);
            }

            for (var i = 0; i < BattleNetworkStateSync.MaxDiceCount; i++)
                _currentTurnDiceValues[i] = TurnDiceValues.Get(i);

            return CreatePresentationSnapshot(
                MatchInitialized,
                IsGameOver,
                WinnerNetworkId,
                CurrentTurnPlayerId,
                TurnDiceConsumedMask,
                _currentPlayerNetworkIds,
                _currentTurnDiceValues,
                _currentPlayerProfiles);
        }

        private static BattlePresentationSnapshot CreatePresentationSnapshot(BattlePresentationSnapshot source,
            int[] playerNetworkIds, int[] turnDiceValues, NetworkPlayerProfile[] playerProfiles) =>
            CreatePresentationSnapshot(
                source.MatchInitialized,
                source.IsGameOver,
                source.WinnerNetworkId,
                source.CurrentTurnPlayerId,
                source.TurnDiceConsumedMask,
                playerNetworkIds,
                turnDiceValues,
                playerProfiles);

        private static BattlePresentationSnapshot CreatePresentationSnapshot(bool matchInitialized, bool isGameOver,
            int winnerNetworkId, int currentTurnPlayerId, int turnDiceConsumedMask, int[] playerNetworkIds,
            int[] turnDiceValues, NetworkPlayerProfile[] playerProfiles) =>
            new(matchInitialized, isGameOver, winnerNetworkId, currentTurnPlayerId, turnDiceConsumedMask,
                playerNetworkIds, turnDiceValues, playerProfiles);

        private static void CopyPresentationSnapshot(BattlePresentationSnapshot source, int[] playerNetworkIds,
            int[] turnDiceValues, NetworkPlayerProfile[] playerProfiles)
        {
            CopyTo(source.PlayerNetworkIds, playerNetworkIds);
            CopyTo(source.TurnDiceValues, turnDiceValues);
            CopyProfilesTo(source.PlayerProfiles, playerProfiles);
        }

        private static void CopyTo(int[] source, int[] destination)
        {
            for (var i = 0; i < source.Length; i++)
                destination[i] = source[i];
        }

        private static void CopyProfilesTo(NetworkPlayerProfile[] source, NetworkPlayerProfile[] destination)
        {
            for (var i = 0; i < source.Length; i++)
                destination[i] = source[i];
        }

        private PlayerProfile TryResolveOpponentPreview()
        {
            if (Runner == null || _modeConfig == null)
                return null;

            var localPlayerId = Runner.LocalPlayer.PlayerId;
            foreach (var player in Runner.ActivePlayers)
            {
                if (player.PlayerId == localPlayerId)
                    continue;

                var tokenProfile = ProfileConnectionToken.TryDecode(Runner.GetPlayerConnectionToken(player));
                if (tokenProfile == null)
                    continue;

                return tokenProfile.WithHp(_modeConfig.MaxHp, _modeConfig.MaxHp);
            }

            return null;
        }
    }
}
