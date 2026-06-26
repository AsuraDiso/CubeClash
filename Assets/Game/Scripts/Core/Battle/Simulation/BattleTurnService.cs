using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Actions;

namespace Game.Scripts.Core.Battle.Simulation
{
    public sealed class BattleTurnService
    {
        private readonly BattleModeConfig _mode;
        private readonly IBattleRandom _random;
        private readonly BattleActionPipeline _actions;

        public BattleTurnService(BattleModeConfig mode, IBattleRandom random, BattleActionPipeline actions)
        {
            _mode = mode;
            _random = random;
            _actions = actions;
        }

        public void BeginTurn(BattleState state)
        {
            state.ResetTurnDiceConsumption();
            RollTurnDice(state);
        }

        public void BeginOpeningTurn(BattleState state)
        {
            const int maxRerolls = 32;

            for (var reroll = 0; reroll < maxRerolls; reroll++)
            {
                BeginTurn(state);

                for (var slot = 0; slot < BattleState.MaxPlayers; slot++)
                {
                    if (!_actions.HasAnyLegalAction(state, slot))
                        continue;

                    state.CurrentTurnPlayerId = state.PlayerNetworkIds[slot];
                    return;
                }
            }

            state.CurrentTurnPlayerId = state.PlayerNetworkIds[0];
        }

        public BattleActionResult ApplyPostAction(BattleState state, BattleActionResult result)
        {
            if (result.GameEnded || !state.AreAllTurnDiceConsumed())
                return result;

            PassTurn(state);
            return result.WithTurnEnded();
        }

        public bool TryPassIfNoLegalPlays(BattleState state)
        {
            if (!state.IsInitialized || state.IsGameOver)
                return false;

            var startingPlayerId = state.CurrentTurnPlayerId;
            var guard = 0;

            while (!state.IsGameOver && guard++ < BattleState.MaxPlayers * 4)
            {
                var slot = state.FindSlotByNetworkId(state.CurrentTurnPlayerId);
                if (slot < 0)
                    return false;

                if (_actions.HasAnyLegalAction(state, slot))
                    return guard > 1;

                PassTurn(state);

                if (state.CurrentTurnPlayerId == startingPlayerId)
                {
                    DeclareStalemate(state);
                    return true;
                }
            }

            return false;
        }

        private void PassTurn(BattleState state)
        {
            var currentSlot = state.FindSlotByNetworkId(state.CurrentTurnPlayerId);
            if (currentSlot < 0)
                return;

            var nextSlot = BattleState.GetOpponentSlot(currentSlot);
            state.CurrentTurnPlayerId = state.PlayerNetworkIds[nextSlot];
            BeginTurn(state);
        }

        private void RollTurnDice(BattleState state)
        {
            for (var i = 0; i < state.TurnDiceValues.Length; i++)
                state.TurnDiceValues[i] = _random.Range(_mode.DiceMin, _mode.DiceMax + 1);
        }

        private void DeclareStalemate(BattleState state)
        {
            state.IsGameOver = true;
            state.WinnerNetworkId = BattleState.UnassignedNetworkId;
        }

        public void ForfeitPlayer(BattleState state, int networkPlayerId)
        {
            if (!state.IsInitialized || state.IsGameOver)
                return;

            var forfeitingSlot = state.FindSlotByNetworkId(networkPlayerId);
            if (forfeitingSlot < 0)
                return;

            state.IsGameOver = true;
            state.WinnerNetworkId = state.PlayerNetworkIds[BattleState.GetOpponentSlot(forfeitingSlot)];
        }
    }
}
