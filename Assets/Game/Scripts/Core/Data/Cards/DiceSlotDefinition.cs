using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scripts.Core.Data.Cards
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
        [field: SerializeField] public List<DiceRequirementEntry> Requirements { get; private set; } = new();
    }

    [Serializable]
    public sealed class DiceRequirementEntry
    {
        [field: SerializeField] public DiceRequirementKind Kind { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

        public bool IsMet(int value, IReadOnlyList<int> priorSlotValues, int slotIndex) => Kind switch
        {
            DiceRequirementKind.LessThan => value < Value,
            DiceRequirementKind.Even => value % 2 == 0,
            DiceRequirementKind.DistinctFromOtherSlots => priorSlotValues.All(v => v != value),
            _ => false,
        };

        public static string FormatLabel(DiceRequirementEntry entry) => entry.Kind switch
        {
            DiceRequirementKind.LessThan => $"< {entry.Value}",
            DiceRequirementKind.Even => "Even",
            DiceRequirementKind.DistinctFromOtherSlots => "Unique",
            _ => "?"
        };
    }
}
