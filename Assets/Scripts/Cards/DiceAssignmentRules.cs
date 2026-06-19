using System.Collections.Generic;

namespace Cards
{
    public static class DiceAssignmentRules
    {
        public static bool CanAssign(
            IReadOnlyList<int> priorSlotValues,
            int slotIndex,
            DiceRequirementEntry requirement,
            int diceValue)
        {
            return requirement.IsMet(diceValue, new DiceAssignmentContext(priorSlotValues), slotIndex);
        }
    }
}
