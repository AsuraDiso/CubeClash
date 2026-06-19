using System;
using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Core.Data
{
    public sealed class InMemoryDeckService : IDeckService
    {
        private readonly List<PlacedCard>[] _decks;

        public int MaxDecks { get; } = DeckLayout.MaxDecks;

        public event Action<int> OnDeckChanged;

        public InMemoryDeckService()
        {
            _decks = new List<PlacedCard>[MaxDecks];
            for (var i = 0; i < MaxDecks; i++)
            {
                _decks[i] = new List<PlacedCard>();
            }
        }

        public IReadOnlyList<PlacedCard> GetDeck(int deckIndex) =>
            IsValidIndex(deckIndex) ? _decks[deckIndex] : Array.Empty<PlacedCard>();

        public bool TryPlaceCard(int deckIndex, CardDefinition card, Vector2Int origin, int catalogIndex)
        {
            if (!TryGetDeck(deckIndex, out var deck)
                || !CardGridPacker.FitsWithinGrid(origin, card.Footprint, DeckLayout.Columns, DeckLayout.Rows))
            {
                return false;
            }

            PlacedCard? restore = RemoveByCatalogIndex(deck, catalogIndex);

            var occupied = CardGridPacker.BuildOccupiedGrid(deck, DeckLayout.Columns, DeckLayout.Rows);
            if (!CardGridPacker.CanPlace(occupied, origin, card.Footprint))
            {
                if (restore.HasValue)
                {
                    deck.Add(restore.Value);
                }

                return false;
            }

            deck.Add(new PlacedCard(card, origin, catalogIndex));
            NotifyChanged(deckIndex);
            return true;
        }

        public bool TryRemoveCard(int deckIndex, int catalogIndex)
        {
            if (!TryGetDeck(deckIndex, out var deck) || catalogIndex < 0)
            {
                return false;
            }

            if (deck.RemoveAll(placed => placed.CatalogIndex == catalogIndex) == 0)
            {
                return false;
            }

            NotifyChanged(deckIndex);
            return true;
        }

        public void ClearDeck(int deckIndex)
        {
            if (!TryGetDeck(deckIndex, out var deck) || deck.Count == 0)
            {
                return;
            }

            deck.Clear();
            NotifyChanged(deckIndex);
        }

        private bool IsValidIndex(int deckIndex) =>
            deckIndex >= 0 && deckIndex < MaxDecks;

        private bool TryGetDeck(int deckIndex, out List<PlacedCard> deck)
        {
            if (IsValidIndex(deckIndex))
            {
                deck = _decks[deckIndex];
                return true;
            }

            deck = null;
            return false;
        }

        private static PlacedCard? RemoveByCatalogIndex(List<PlacedCard> deck, int catalogIndex)
        {
            if (catalogIndex < 0)
            {
                return null;
            }

            var existingIndex = deck.FindIndex(placed => placed.CatalogIndex == catalogIndex);
            if (existingIndex < 0)
            {
                return null;
            }

            var removed = deck[existingIndex];
            deck.RemoveAt(existingIndex);
            return removed;
        }

        private void NotifyChanged(int deckIndex) => OnDeckChanged?.Invoke(deckIndex);
    }
}
