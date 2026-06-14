using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    [CreateAssetMenu(menuName = "CubeClash/CardCatalog", fileName = "CardCatalog")]
    public sealed class CardCatalog : ScriptableObject
    {
        [field: SerializeField] public List<CardDefinition> Cards { get; private set; }
    }
}
