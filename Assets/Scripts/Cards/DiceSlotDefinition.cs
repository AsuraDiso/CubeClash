using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cards
{
    public enum DiceRequirementKind
    {
        LessThan,
        Even,
        DistinctFromOtherSlots,
    }

    [Serializable]
    public sealed class DiceSlotDefinition
    {
        public List<DiceRequirementEntry> Requirements = new();
    }

    [Serializable]
    public sealed class DiceRequirementEntry
    {
        public DiceRequirementKind Kind;
        [SerializeField] int _maxExclusive;

        public bool IsMet(int value, DiceAssignmentContext ctx, int slotIndex) => Kind switch
        {
            DiceRequirementKind.LessThan => value < _maxExclusive,
            DiceRequirementKind.Even => value % 2 == 0,
            DiceRequirementKind.DistinctFromOtherSlots => ctx.Values.Take(slotIndex).All(v => v != value),
            _ => false,
        };
    }
}
