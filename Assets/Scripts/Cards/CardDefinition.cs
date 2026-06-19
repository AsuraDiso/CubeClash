using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    [CreateAssetMenu(menuName = "CubeClash/Cards/Card Definition")]
    public sealed class CardDefinition : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public CardRarity Rarity { get; private set; }

        [field: SerializeField] public CardFootprintSize Footprint { get; private set; } = CardFootprintSize.OneByOne;
        [field: SerializeField] public CardLayout Layout { get; private set; }

        [field: SerializeField] public List<DiceSlotDefinition> DiceSlots;

        [field: SerializeField] public ValueFormula ValueFormula { get; private set; }
    }
}
