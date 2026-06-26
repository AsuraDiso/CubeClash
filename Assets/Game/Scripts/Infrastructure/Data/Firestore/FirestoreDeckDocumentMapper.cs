using System.Collections.Generic;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Data.Cards;

namespace Game.Scripts.Infrastructure.Data.Firestore
{
    internal static class FirestoreDeckDocumentMapper
    {
        public static DeckPersistenceData ToPersistenceData(FirestoreUserDocument document)
        {
            var decks = new List<IReadOnlyList<DeckCardRecord>>();
            if (document.Decks != null)
            {
                foreach (var deck in document.Decks)
                    decks.Add(ToRecords(deck.Cards));
            }

            var selectedIndex = document.SelectedDeckIndex;
            if (selectedIndex < 0 || selectedIndex >= DeckLayout.MaxDecks)
                selectedIndex = 0;

            return new DeckPersistenceData
            {
                SelectedDeckIndex = selectedIndex,
                Decks = decks
            };
        }
        public static List<FirestoreDeckSlotDocument> ToFirestoreDecks(DeckPersistenceData data)
        {
            var decks = new List<FirestoreDeckSlotDocument>();
            if (data.Decks == null)
                return decks;

            foreach (var deck in data.Decks)
            {
                var cards = new List<FirestorePlacedCardDocument>();
                if (deck != null)
                {
                    foreach (var record in deck)
                    {
                        cards.Add(new FirestorePlacedCardDocument
                        {
                            CatalogIndex = record.CatalogIndex,
                            OriginX = record.OriginX,
                            OriginY = record.OriginY
                        });
                    }
                }

                decks.Add(new FirestoreDeckSlotDocument { Cards = cards });
            }

            return decks;
        }

        private static List<DeckCardRecord> ToRecords(List<FirestorePlacedCardDocument> cards)
        {
            var records = new List<DeckCardRecord>();
            if (cards == null)
                return records;

            foreach (var card in cards)
            {
                if (card.CatalogIndex < 0)
                    continue;

                records.Add(new DeckCardRecord(card.CatalogIndex, card.OriginX, card.OriginY));
            }

            return records;
        }
    }
}
