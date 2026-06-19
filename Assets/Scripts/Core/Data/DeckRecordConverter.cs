using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Core.Data
{
    public static class DeckRecordConverter
    {
        public static List<DeckCardRecord> FromPlacedCards(IReadOnlyList<PlacedCard> deck)
        {
            var records = new List<DeckCardRecord>();
            if (deck == null)
            {
                return records;
            }

            foreach (var placed in deck)
            {
                records.Add(new DeckCardRecord(
                    placed.CatalogIndex,
                    placed.Origin.x,
                    placed.Origin.y));
            }

            return records;
        }

        public static List<PlacedCard> ToPlacedCards(IReadOnlyList<DeckCardRecord> records, CardCatalog catalog)
        {
            var result = new List<PlacedCard>();
            if (records == null || catalog?.Cards == null)
            {
                return result;
            }

            foreach (var record in records)
            {
                if (record.CatalogIndex < 0 || record.CatalogIndex >= catalog.Cards.Count)
                {
                    continue;
                }

                var definition = catalog.Cards[record.CatalogIndex];
                if (definition == null)
                {
                    continue;
                }

                var origin = new Vector2Int(record.OriginX, record.OriginY);
                if (!CardGridPacker.FitsWithinGrid(origin, definition.Footprint, DeckLayout.Columns, DeckLayout.Rows))
                {
                    continue;
                }

                result.Add(new PlacedCard(definition, origin, record.CatalogIndex));
            }

            return result;
        }

        public static int[] PackNetwork(IReadOnlyList<DeckCardRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                return new[] { 0 };
            }

            var packed = new int[1 + records.Count * 3];
            packed[0] = records.Count;

            for (var i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var offset = 1 + i * 3;
                packed[offset] = record.CatalogIndex;
                packed[offset + 1] = record.OriginX;
                packed[offset + 2] = record.OriginY;
            }

            return packed;
        }

        public static List<DeckCardRecord> UnpackNetwork(int[] packed)
        {
            var records = new List<DeckCardRecord>();
            if (packed == null || packed.Length == 0)
            {
                return records;
            }

            var count = Mathf.Min(packed[0], (packed.Length - 1) / 3);
            for (var i = 0; i < count; i++)
            {
                var offset = 1 + i * 3;
                records.Add(new DeckCardRecord(packed[offset], packed[offset + 1], packed[offset + 2]));
            }

            return records;
        }

        public static DeckPersistenceData FromState(DeckState state)
        {
            var decks = new List<IReadOnlyList<DeckCardRecord>>(state.MaxDecks);
            for (var deckIndex = 0; deckIndex < state.MaxDecks; deckIndex++)
            {
                decks.Add(FromPlacedCards(state.GetDeck(deckIndex)));
            }

            return new DeckPersistenceData
            {
                SelectedDeckIndex = state.SelectedDeckIndex,
                Decks = decks
            };
        }
    }
}
