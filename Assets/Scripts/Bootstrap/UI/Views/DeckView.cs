using System;
using System.Collections.Generic;
using Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public sealed class DeckView : MonoBehaviour
    {
        [SerializeField] private CardGridView _cardGrid;
        [SerializeField] private CardGridView _inventoryGrid;
        [SerializeField] private DeckDropZone _dropZone;
        [SerializeField] private Canvas _dragCanvas;
        [SerializeField] private List<Button> _deckButtons = new();
        [SerializeField] private Color _selectedTabColor = Color.green;
        [SerializeField] private Color _normalTabColor = Color.white;

        public event Action<int> OnDeckSelected;
        public event Action<CardView, PointerEventData> OnCardDropped;
        public event Action<CardView, PointerEventData> OnCardDragEnded;

        public int InventoryColumns => _inventoryGrid != null ? _inventoryGrid.Columns : DeckLayout.InventoryColumns;

        private void Awake()
        {
            if (_dragCanvas != null)
            {
                _cardGrid?.SetDragCanvas(_dragCanvas);
                _inventoryGrid?.SetDragCanvas(_dragCanvas);
            }

            for (int i = 0; i < _deckButtons.Count; i++)
            {
                int index = i;
                _deckButtons[i].onClick.AddListener(() => OnDeckSelected?.Invoke(index));
            }

            if (_dropZone != null)
            {
                _dropZone.OnCardDropped += (card, data) => OnCardDropped?.Invoke(card, data);
            }

            if (_cardGrid != null)
            {
                _cardGrid.CardDragEnded += (card, data) => OnCardDragEnded?.Invoke(card, data);
            }

            if (_inventoryGrid != null)
            {
                _inventoryGrid.CardDragEnded += (card, data) => OnCardDragEnded?.Invoke(card, data);
            }
        }

        private void OnDestroy()
        {
            foreach (var button in _deckButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        public void SetActiveDeckCards(IReadOnlyList<PlacedCard> placedCards)
        {
            if (_cardGrid != null)
            {
                _cardGrid.SetCards(placedCards);
            }
        }

        public bool TryGetActiveGridOrigin(Vector2 screenPoint, Camera eventCamera, out Vector2Int origin)
        {
            origin = Vector2Int.zero;
            return _cardGrid != null
                   && _cardGrid.TryGetGridOriginFromScreenPoint(screenPoint, eventCamera, out origin);
        }

        public bool IsInActiveDeck(CardView card) => card != null && card.OwnerGrid == _cardGrid;

        public bool ConsumeDeckDrop() => _dropZone != null && _dropZone.ConsumeDrop();

        public void SetInventoryCards(IReadOnlyList<PlacedCard> inventoryCards)
        {
            if (_inventoryGrid != null)
            {
                _inventoryGrid.SetCards(inventoryCards);
            }
        }

        public void SetActiveDeckIndex(int index)
        {
            for (int i = 0; i < _deckButtons.Count; i++)
            {
                var colors = _deckButtons[i].colors;
                colors.normalColor = i == index ? _selectedTabColor : _normalTabColor;
                _deckButtons[i].colors = colors;
            }
        }
    }
}
