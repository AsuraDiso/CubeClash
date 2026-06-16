using System;
using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Core.Data
{
    public interface IDeckService
    {
        int MaxDecks { get; }
        IReadOnlyList<PlacedCard> GetDeck(int deckIndex);
        bool TryPlaceCard(int deckIndex, CardDefinition card, Vector2Int origin, int catalogIndex);
        bool TryRemoveCard(int deckIndex, int catalogIndex);
        void ClearDeck(int deckIndex);
        
        event Action<int> OnDeckChanged;
    }
}
