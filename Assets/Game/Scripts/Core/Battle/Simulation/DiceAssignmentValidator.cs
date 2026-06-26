using System.Collections.Generic;
using Game.Scripts.Core.Data.Cards;

namespace Game.Scripts.Core.Battle.Simulation
{
    public static class DiceAssignmentValidator
    {
        public static bool IsSlotValid(int slotIndex, int dieValue, IReadOnlyList<DiceRequirementEntry> requirements, IReadOnlyList<int> priorSlotValues)
        {
            if (slotIndex < 0 || slotIndex >= requirements.Count)
                return false;

            return requirements[slotIndex].IsMet(dieValue, priorSlotValues, slotIndex);
        }

        public static bool TryValidateTurnDice(int[] dieIndices, IReadOnlyList<DiceRequirementEntry> requirements,
            int[] turnDiceValues, bool[] turnDiceConsumed, out int[] diceValues)
        {
            diceValues = null;

            if (dieIndices == null)
                return false;

            if (requirements.Count == 0)
                return dieIndices.Length == 0;

            if (dieIndices.Length != requirements.Count)
                return false;

            if (HasDuplicateIndices(dieIndices))
                return false;

            diceValues = new int[requirements.Count];
            for (var i = 0; i < requirements.Count; i++)
            {
                var dieIndex = dieIndices[i];
                if (dieIndex < 0 || dieIndex >= turnDiceValues.Length || turnDiceConsumed[dieIndex])
                    return false;

                diceValues[i] = turnDiceValues[dieIndex];
            }

            var priorValues = new List<int>(requirements.Count);
            for (var i = 0; i < requirements.Count; i++)
            {
                if (!requirements[i].IsMet(diceValues[i], priorValues, i))
                    return false;

                priorValues.Add(diceValues[i]);
            }

            return true;
        }

        private static bool HasDuplicateIndices(int[] dieIndices)
        {
            for (var i = 0; i < dieIndices.Length; i++)
            {
                for (var j = i + 1; j < dieIndices.Length; j++)
                {
                    if (dieIndices[i] == dieIndices[j])
                        return true;
                }
            }

            return false;
        }
    }
}
