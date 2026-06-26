using System;
using System.Collections.Generic;
using Game.Scripts.Core.Data.Cards;
using Game.Scripts.Bootstrap.UI.Views;
using Game.Shared.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Features.Deck.Scripts
{
    public sealed class DeckView : NavigableScreenView
    {
        [SerializeField] private CardGridView _cardGrid;
        [SerializeField] private CardGridView _inventoryGrid;
        [SerializeField] private DeckDropZone _dropZone;
        [SerializeField] private TabGroup _deckTabs;

        public event Action<int> DeckSelected;
        public event Action<CardView, PointerEventData> CardDropped;
        public event Action<CardView, PointerEventData> CardDragEnded;

        public int InventoryColumns => _inventoryGrid.Columns;

        private void Awake()
        {
            _deckTabs.SelectionChanged += index => DeckSelected?.Invoke(index);
            _dropZone.CardDropped += (card, data) => CardDropped?.Invoke(card, data);
            _cardGrid.CardDragEnded += HandleCardDragEnded;
            _inventoryGrid.CardDragEnded += HandleCardDragEnded;
        }

        private void HandleCardDragEnded(CardView card, PointerEventData data)
        {
            _dropZone.ResetDropFlag();
            CardDragEnded?.Invoke(card, data);
        }

        public void SetActiveDeckCards(IReadOnlyList<PlacedCard> placedCards) => _cardGrid.SetCards(placedCards);

        public bool TryGetActiveGridOrigin(Vector2 screenPoint, Camera eventCamera, out Vector2Int origin) =>
            _cardGrid.TryGetGridOriginFromScreenPoint(screenPoint, eventCamera, out origin);

        public bool IsInActiveDeck(CardView card) => card.OwnerGrid == _cardGrid;

        public bool ConsumeDeckDrop() => _dropZone.ConsumeDrop();

        public void SetInventoryCards(IReadOnlyList<PlacedCard> inventoryCards) =>
            _inventoryGrid.SetCards(inventoryCards);

        public void SetActiveDeckIndex(int index) => _deckTabs.Select(index);
    }
}
