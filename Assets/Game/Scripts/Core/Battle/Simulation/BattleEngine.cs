using System.Collections.Generic;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Actions;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Data.Cards;

namespace Game.Scripts.Core.Battle.Simulation
{
    public sealed class BattleEngine
    {
        private readonly BattleModeConfig _mode;
        private readonly BattleTurnService _turns;
        private readonly BattleActionPipeline _actions;

        public BattleState State { get; }

        public BattleEngine(BattleModeConfig mode, IBattleRandom random)
            : this(mode, random, BattleActionHandlers.CreateDefault(mode))
        {
        }

        public BattleEngine(BattleModeConfig mode, IBattleRandom random, IEnumerable<IBattleActionHandler> handlers)
        {
            _mode = mode;
            State = new BattleState(mode);
            _actions = new BattleActionPipeline(handlers);
            _turns = new BattleTurnService(mode, random, _actions);
        }

        public void Initialize(int[] networkIds, PlayerProfile[] profiles, IReadOnlyList<PlacedCard>[] decks)
        {
            State.BattleModeId = _mode.ModeId;
            State.IsInitialized = true;
            State.IsGameOver = false;
            State.WinnerNetworkId = BattleState.UnassignedNetworkId;

            for (var i = 0; i < BattleState.MaxPlayers; i++)
            {
                State.PlayerNetworkIds[i] = networkIds[i];
                State.SetProfile(i, profiles[i]);
                State.SetPlayerDeck(i, decks[i]);
            }

            State.CurrentTurnPlayerId = networkIds[0];
            _turns.BeginOpeningTurn(State);
        }

        public BattleActionResult TryExecuteAction(int actorNetworkId, IBattleAction action)
        {
            if (!State.IsInitialized || State.IsGameOver)
                return BattleActionResult.Failed;

            if (actorNetworkId != State.CurrentTurnPlayerId)
                return BattleActionResult.Failed;

            var result = _actions.TryExecute(State, actorNetworkId, action);
            if (!result.Success)
                return result;

            result = _turns.ApplyPostAction(State, result);
            _turns.TryPassIfNoLegalPlays(State);
            return result;
        }

        public bool TryPassTurnIfNoLegalPlays() => _turns.TryPassIfNoLegalPlays(State);

        public void ForfeitPlayer(int networkPlayerId) => _turns.ForfeitPlayer(State, networkPlayerId);

        public bool CanExecuteAction(int actorNetworkId, IBattleAction action) =>
            _actions.CanExecute(State, actorNetworkId, action);
    }
}
