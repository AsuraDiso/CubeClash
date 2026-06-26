using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Core.Data.Cards
{
    [CreateAssetMenu(menuName = "CubeClash/Cards/Card Definition")]
    public sealed class CardDefinition : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public CardRarity Rarity { get; private set; }

        [field: SerializeField] public CardFootprintSize Footprint { get; private set; } = CardFootprintSize.OneByOne;
        [field: SerializeField] public CardLayout Layout { get; private set; }

        [field: SerializeField] public List<DiceSlotDefinition> DiceSlots { get; private set; } = new();

        [field: SerializeField] public ValueFormula ValueFormula { get; private set; }

        private IReadOnlyList<DiceRequirementEntry> _cachedRequirements;

        public IReadOnlyList<DiceRequirementEntry> GetFlatRequirements()
        {
            if (_cachedRequirements != null)
                return _cachedRequirements;

            var requirements = new List<DiceRequirementEntry>();
            if (DiceSlots == null)
                return requirements;

            foreach (var diceSlot in DiceSlots)
            {
                if (diceSlot?.Requirements == null)
                    continue;

                foreach (var requirement in diceSlot.Requirements)
                {
                    if (requirement != null)
                        requirements.Add(requirement);
                }
            }

            _cachedRequirements = requirements;
            return requirements;
        }
    }
}
