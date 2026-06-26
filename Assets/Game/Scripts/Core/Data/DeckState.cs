using System;
using System.Collections.Generic;
using Game.Scripts.Core.Data.Cards;
using UnityEngine;

namespace Game.Scripts.Core.Data
{
    public sealed class DeckState
    {
        private readonly List<PlacedCard>[] _decks;

        public DeckState(int maxDecks)
        {
            _decks = new List<PlacedCard>[maxDecks];
            for (var i = 0; i < maxDecks; i++)
            {
                _decks[i] = new List<PlacedCard>();
            }
        }

        public int MaxDecks => _decks.Length;

        public int SelectedDeckIndex { get; set; }

        public IReadOnlyList<PlacedCard> GetDeck(int deckIndex) =>
            IsValidIndex(deckIndex) ? _decks[deckIndex] : Array.Empty<PlacedCard>();

        public void SetDeck(int deckIndex, IReadOnlyList<PlacedCard> cards)
        {
            if (!TryGetDeck(deckIndex, out var deck))
            {
                return;
            }

            deck.Clear();
            if (cards != null)
            {
                deck.AddRange(cards);
            }
        }

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
            return true;
        }

        public bool TryRemoveCard(int deckIndex, int catalogIndex)
        {
            if (!TryGetDeck(deckIndex, out var deck) || catalogIndex < 0)
            {
                return false;
            }

            return deck.RemoveAll(placed => placed.CatalogIndex == catalogIndex) > 0;
        }

        public void ClearDeck(int deckIndex)
        {
            if (TryGetDeck(deckIndex, out var deck))
            {
                deck.Clear();
            }
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
    }
}
