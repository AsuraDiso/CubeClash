using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using UnityEngine;

namespace Core.Data
{
    public sealed class InMemoryDeckService : IDeckService
    {
        private readonly Dictionary<int, List<PlacedCard>> _decks = new();

        public int MaxDecks { get; } = DeckLayout.MaxDecks;

        public event Action<int> OnDeckChanged;

        public InMemoryDeckService()
        {
            for (int i = 0; i < MaxDecks; i++)
            {
                _decks[i] = new List<PlacedCard>();
            }
        }

        public IReadOnlyList<PlacedCard> GetDeck(int deckIndex)
        {
            if (_decks.TryGetValue(deckIndex, out var deck))
            {
                return deck;
            }

            return Array.Empty<PlacedCard>();
        }

        public bool TryPlaceCard(int deckIndex, CardDefinition card, Vector2Int origin, int catalogIndex)
        {
            if (!_decks.TryGetValue(deckIndex, out var deck))
            {
                return false;
            }

            if (origin.x < 0 || origin.y < 0
                || origin.x + card.Footprint.Columns > DeckLayout.Columns
                || origin.y + card.Footprint.Rows > DeckLayout.Rows)
            {
                return false;
            }

            PlacedCard previousPlacement = default;
            var hadPrevious = false;

            if (catalogIndex >= 0)
            {
                var existingIndex = deck.FindIndex(placed => placed.CatalogIndex == catalogIndex);
                if (existingIndex >= 0)
                {
                    previousPlacement = deck[existingIndex];
                    hadPrevious = true;
                    deck.RemoveAt(existingIndex);
                }
            }

            var occupied = GetOccupiedGrid(deck);

            if (!CardGridPacker.CanPlace(occupied, origin, card.Footprint))
            {
                if (hadPrevious)
                {
                    deck.Add(previousPlacement);
                }

                return false;
            }

            deck.Add(new PlacedCard(card, origin, catalogIndex));
            OnDeckChanged?.Invoke(deckIndex);
            return true;
        }

        public bool TryRemoveCard(int deckIndex, int catalogIndex)
        {
            if (!_decks.TryGetValue(deckIndex, out var deck))
            {
                return false;
            }

            if (catalogIndex < 0)
            {
                return false;
            }

            if (deck.RemoveAll(placed => placed.CatalogIndex == catalogIndex) > 0)
            {
                OnDeckChanged?.Invoke(deckIndex);
                return true;
            }

            return false;
        }

        public void ClearDeck(int deckIndex)
        {
            if (!_decks.TryGetValue(deckIndex, out var deck) || deck.Count == 0)
            {
                return;
            }

            deck.Clear();
            OnDeckChanged?.Invoke(deckIndex);
        }

        private static bool[,] GetOccupiedGrid(List<PlacedCard> deck)
        {
            var occupied = new bool[DeckLayout.Columns, DeckLayout.Rows];
            foreach (var placed in deck)
            {
                CardGridPacker.Occupy(occupied, placed.Origin, placed.Definition.Footprint);
            }

            return occupied;
        }
    }
}
