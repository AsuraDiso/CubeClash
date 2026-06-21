using System;
using System.Collections.Generic;
using Cards;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bootstrap.UI.Views
{
    public sealed class DeckView : MonoBehaviour, INavigableView
    {
        [SerializeField] private CardGridView _cardGrid;
        [SerializeField] private CardGridView _inventoryGrid;
        [SerializeField] private DeckDropZone _dropZone;
        [SerializeField] private Canvas _dragCanvas;
        [SerializeField] private TabGroup _deckTabs;

        public event Action BackClicked;
        public event Action<int> OnDeckSelected;
        public event Action<CardView, PointerEventData> OnCardDropped;
        public event Action<CardView, PointerEventData> OnCardDragEnded;

        public int InventoryColumns => _inventoryGrid != null ? _inventoryGrid.Columns : DeckLayout.InventoryColumns;

        private void Awake()
        {
            _cardGrid.SetDragCanvas(_dragCanvas);
            _inventoryGrid.SetDragCanvas(_dragCanvas);
            _deckTabs.SelectionChanged += index => OnDeckSelected?.Invoke(index);
            _dropZone.OnCardDropped += (card, data) => OnCardDropped?.Invoke(card, data);
            _cardGrid.CardDragEnded += (card, data) => OnCardDragEnded?.Invoke(card, data);
            _inventoryGrid.CardDragEnded += (card, data) => OnCardDragEnded?.Invoke(card, data);
        }


        public void SetActiveDeckCards(IReadOnlyList<PlacedCard> placedCards) => _cardGrid?.SetCards(placedCards);

        public bool TryGetActiveGridOrigin(Vector2 screenPoint, Camera eventCamera, out Vector2Int origin)
        {
            origin = Vector2Int.zero;
            return _cardGrid != null && _cardGrid.TryGetGridOriginFromScreenPoint(screenPoint, eventCamera, out origin);
        }

        public bool IsInActiveDeck(CardView card) => card != null && card.OwnerGrid == _cardGrid;

        public bool ConsumeDeckDrop() => _dropZone != null && _dropZone.ConsumeDrop();

        public void SetInventoryCards(IReadOnlyList<PlacedCard> inventoryCards) => _inventoryGrid?.SetCards(inventoryCards);

        public void SetActiveDeckIndex(int index) => _deckTabs?.Select(index);

        public void OnBackButtonClicked() => BackClicked?.Invoke();
    }
}
