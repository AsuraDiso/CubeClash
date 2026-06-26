using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Core.Data.Cards
{
    [CreateAssetMenu(menuName = "CubeClash/CardCatalog", fileName = "CardCatalog")]
    public sealed class CardCatalog : ScriptableObject
    {
        [field: SerializeField] public List<CardDefinition> Cards { get; private set; }

        public List<PlacedCard> PackInventoryNotInDeck(IReadOnlyList<PlacedCard> deck, int columns, int maxRows)
        {
            var excluded = new HashSet<int>();
            foreach (var placed in deck)
                if (placed.CatalogIndex >= 0)
                    excluded.Add(placed.CatalogIndex);

            var availableCards = new List<CardDefinition>();
            var availableIndices = new List<int>();

            for (var i = 0; i < Cards.Count; i++)
            {
                var card = Cards[i];
                if (card == null || excluded.Contains(i))
                    continue;

                availableCards.Add(card);
                availableIndices.Add(i);
            }

            return CardGridPacker.AutoPack(availableCards, columns, maxRows, availableIndices);
        }
    }
}
