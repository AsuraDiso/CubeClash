using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Core.Data.Cards;
using UnityEngine;

namespace Game.Scripts.Core.Data
{
    public interface IDeckService
    {
        public int MaxDecks { get; }
        public bool IsLoaded { get; }
        public int SelectedDeckIndex { get; }

        public UniTask LoadAsync(CancellationToken cancellationToken = default);
        public IReadOnlyList<PlacedCard> GetDeck(int deckIndex);
        public bool TryPlaceCard(int deckIndex, CardDefinition card, Vector2Int origin, int catalogIndex);
        public bool TryRemoveCard(int deckIndex, int catalogIndex);
        public void ClearDeck(int deckIndex);
        public void SetSelectedDeckIndex(int deckIndex);

        public event Action<int> DeckChanged;
    }
}
