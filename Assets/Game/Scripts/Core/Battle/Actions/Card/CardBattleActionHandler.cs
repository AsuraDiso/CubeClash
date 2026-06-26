using System.Collections.Generic;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Actions;
using Game.Scripts.Core.Battle.Simulation;
using Game.Scripts.Core.Data.Cards;

namespace Game.Scripts.Core.Battle.Actions.Card
{
    public sealed class CardBattleActionHandler : IBattleActionHandler
    {
        private readonly BattleModeConfig _mode;

        public CardBattleActionHandler(BattleModeConfig mode) => _mode = mode;

        public string TypeId => BattleActionTypes.CardResolve;

        public bool CanExecute(BattleState state, int actorNetworkId, IBattleAction action)
        {
            if (action is not CardBattleAction cardAction)
                return false;

            return TryCreateContext(state, actorNetworkId, cardAction, out _, out _, out _);
        }

        public BattleActionResult TryExecute(BattleState state, int actorNetworkId, IBattleAction action)
        {
            if (action is not CardBattleAction cardAction)
                return BattleActionResult.Failed;

            if (!TryCreateContext(state, actorNetworkId, cardAction, out var attackerSlot, out var placed,
                    out var diceValues))
                return BattleActionResult.Failed;

            ConsumeDice(state, cardAction.DieIndices);
            var damage = CardDamageCalculator.Resolve(placed.Definition.ValueFormula, diceValues);
            var defenderSlot = BattleState.GetOpponentSlot(attackerSlot);
            var defender = state.Profiles[defenderSlot];
            state.SetProfile(defenderSlot, defender.WithHp(defender.Hp - damage, _mode.MaxHp));

            var attackerLabel = state.Profiles[attackerSlot].DisplayName;
            var result = new CardBattleActionResult(actorNetworkId, attackerLabel, cardAction.DeckCardIndex, damage,
                cardAction.DieIndices);

            if (state.Profiles[defenderSlot].Hp <= 0)
            {
                state.IsGameOver = true;
                state.WinnerNetworkId = actorNetworkId;
                return result.WithGameEnd(actorNetworkId);
            }

            return result;
        }

        public bool HasAnyLegalAction(BattleState state, int actorSlot)
        {
            var availableDice = GetAvailableDieIndices(state);
            foreach (var placed in state.PlayerDecks[actorSlot])
            {
                if (placed.Definition == null)
                    continue;

                var requirements = placed.Definition.GetFlatRequirements();
                if (requirements.Count == 0)
                    return true;

                if (requirements.Count > availableDice.Count)
                    continue;

                if (HasValidAssignment(state, requirements, availableDice))
                    return true;
            }

            return false;
        }

        private bool TryCreateContext(BattleState state, int actorNetworkId, CardBattleAction action,
            out int attackerSlot, out PlacedCard placed, out int[] diceValues)
        {
            placed = default;
            diceValues = null;
            attackerSlot = state.FindSlotByNetworkId(actorNetworkId);

            if (!state.IsInitialized || state.IsGameOver || attackerSlot < 0)
                return false;

            if (actorNetworkId != state.CurrentTurnPlayerId)
                return false;

            var deck = state.PlayerDecks[attackerSlot];
            if (action.DeckCardIndex < 0 || action.DeckCardIndex >= deck.Count)
                return false;

            placed = deck[action.DeckCardIndex];
            if (placed.Definition == null)
                return false;

            return DiceAssignmentValidator.TryValidateTurnDice(
                action.DieIndices,
                placed.Definition.GetFlatRequirements(),
                state.TurnDiceValues,
                state.TurnDiceConsumed,
                out diceValues);
        }

        private static void ConsumeDice(BattleState state, int[] dieIndices)
        {
            foreach (var dieIndex in dieIndices)
                state.TurnDiceConsumed[dieIndex] = true;
        }

        private static List<int> GetAvailableDieIndices(BattleState state)
        {
            var indices = new List<int>(state.TurnDiceValues.Length);
            for (var i = 0; i < state.TurnDiceValues.Length; i++)
            {
                if (!state.TurnDiceConsumed[i] && state.TurnDiceValues[i] > 0)
                    indices.Add(i);
            }

            return indices;
        }

        private static bool HasValidAssignment(BattleState state, IReadOnlyList<DiceRequirementEntry> requirements,
            IReadOnlyList<int> availableDieIndices)
        {
            if (requirements.Count > availableDieIndices.Count)
                return false;

            var used = new bool[availableDieIndices.Count];
            return TryAssign(state, requirements, availableDieIndices, used, 0, new List<int>());
        }

        private static bool TryAssign(BattleState state, IReadOnlyList<DiceRequirementEntry> requirements,
            IReadOnlyList<int> availableDieIndices, bool[] used, int slotIndex, List<int> priorValues)
        {
            if (slotIndex >= requirements.Count)
                return true;

            for (var i = 0; i < availableDieIndices.Count; i++)
            {
                if (used[i])
                    continue;

                var dieIndex = availableDieIndices[i];
                var dieValue = state.TurnDiceValues[dieIndex];
                if (!DiceAssignmentValidator.IsSlotValid(slotIndex, dieValue, requirements, priorValues))
                    continue;

                used[i] = true;
                priorValues.Add(dieValue);
                if (TryAssign(state, requirements, availableDieIndices, used, slotIndex + 1, priorValues))
                    return true;

                priorValues.RemoveAt(priorValues.Count - 1);
                used[i] = false;
            }

            return false;
        }
    }
}
