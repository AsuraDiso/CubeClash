using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    [CreateAssetMenu(menuName = "CubeClash/Cards/Card Definition")]
    public sealed class CardDefinition : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public CardRarity Rarity { get; private set; }

        [field: SerializeField] public CardFootprint Footprint { get; private set; }
        [field: SerializeField] public CardLayout Layout { get; private set; }

        [field: SerializeField] public int DiceSlotCount { get; private set; }
        [SerializeField] private List<DiceSlotDefinition> _diceSlots;

        [field: SerializeField] public ValueFormula ValueFormula { get; private set; }

        [SerializeReference] private List<CardEffectDefinition> _effects;
    }
}
