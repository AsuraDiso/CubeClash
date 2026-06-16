using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    public readonly struct PlacedCard
    {
        public CardDefinition Definition { get; }
        public Vector2Int Origin { get; }
        public int CatalogIndex { get; }

        public PlacedCard(CardDefinition definition, Vector2Int origin, int catalogIndex = -1)
        {
            Definition = definition;
            Origin = origin;
            CatalogIndex = catalogIndex;
        }
    }

    public static class CardGridPacker
    {
        public static List<PlacedCard> AutoPack(
            IReadOnlyList<CardDefinition> cards,
            int columns,
            int rows,
            IReadOnlyList<int> catalogIndices = null)
        {
            var result = new List<PlacedCard>();
            var occupied = new bool[columns, rows];

            if (cards == null)
            {
                return result;
            }

            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card == null)
                {
                    continue;
                }

                var catalogIndex = catalogIndices != null && i < catalogIndices.Count
                    ? catalogIndices[i]
                    : -1;

                if (!TryFindSlot(occupied, columns, rows, card.Footprint, out var origin))
                {
                    Debug.LogWarning($"[CubeClash] No grid slot for card '{card.DisplayName}' ({card.Footprint}).");
                    continue;
                }

                Occupy(occupied, origin, card.Footprint);
                result.Add(new PlacedCard(card, origin, catalogIndex));
            }

            return result;
        }

        public static bool CanPlace(
            bool[,] occupied,
            Vector2Int origin,
            CardFootprintSize footprint)
        {
            var columns = occupied.GetLength(0);
            var rows = occupied.GetLength(1);

            for (var column = 0; column < footprint.Columns; column++)
            {
                for (var row = 0; row < footprint.Rows; row++)
                {
                    var x = origin.x + column;
                    var y = origin.y + row;

                    if (x < 0 || y < 0 || x >= columns || y >= rows || occupied[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static void Occupy(bool[,] occupied, Vector2Int origin, CardFootprintSize footprint)
        {
            for (var column = 0; column < footprint.Columns; column++)
            {
                for (var row = 0; row < footprint.Rows; row++)
                {
                    occupied[origin.x + column, origin.y + row] = true;
                }
            }
        }

        public static bool TryFindSlot(
            bool[,] occupied,
            int columns,
            int rows,
            CardFootprintSize footprint,
            out Vector2Int origin)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    origin = new Vector2Int(column, row);
                    if (CanPlace(occupied, origin, footprint))
                    {
                        return true;
                    }
                }
            }

            origin = default;
            return false;
        }
    }
}
