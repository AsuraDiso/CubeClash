using System;
using System.Collections.Generic;
using System.Linq;
using Bootstrap.UI.Views;
using Cards;
using Core.Data;

namespace Bootstrap.UI.Controllers
{
    public sealed class CardController : IDisposable
    {
        private readonly CardCatalog _cardCatalog;
        private readonly IDeckService _deckService;

        private DeckView _view;
        private int _currentDeckIndex;

        public CardController(CardCatalog cardCatalog, IDeckService deckService)
        {
            _cardCatalog = cardCatalog;
            _deckService = deckService;
            _deckService.OnDeckChanged += HandleDeckChanged;
        }

        public void Bind(DeckView view)
        {
            if (_view != null)
            {
                _view.OnDeckSelected -= SelectDeck;
                _view.OnCardDropped -= HandleCardDropped;
                _view.OnCardDragEnded -= HandleCardDragEnded;
            }

            _view = view;

            if (_view == null)
            {
                return;
            }

            _view.OnDeckSelected += SelectDeck;
            _view.OnCardDropped += HandleCardDropped;
            _view.OnCardDragEnded += HandleCardDragEnded;

            SelectDeck(0);
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnDeckSelected -= SelectDeck;
                _view.OnCardDropped -= HandleCardDropped;
                _view.OnCardDragEnded -= HandleCardDragEnded;
            }

            _deckService.OnDeckChanged -= HandleDeckChanged;
            _view = null;
        }

        private void HandleCardDragEnded(CardView cardView, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (_view == null || cardView == null)
            {
                return;
            }

            if (_view.ConsumeDeckDrop())
            {
                return;
            }

            if (_view.IsInActiveDeck(cardView))
            {
                _deckService.TryRemoveCard(_currentDeckIndex, cardView.CatalogIndex);
            }
        }

        private void HandleCardDropped(CardView cardView, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (_view == null || cardView == null)
            {
                return;
            }

            if (_view.TryGetActiveGridOrigin(eventData.position, eventData.pressEventCamera, out var origin))
            {
                _deckService.TryPlaceCard(
                    _currentDeckIndex,
                    cardView.Definition,
                    origin,
                    cardView.CatalogIndex);
            }
        }

        private void HandleDeckChanged(int index)
        {
            if (_view == null || index != _currentDeckIndex)
            {
                return;
            }

            RefreshDeck();
            RefreshInventory();
        }

        private void SelectDeck(int index)
        {
            _currentDeckIndex = index;
            if (_view == null)
            {
                return;
            }

            _view.SetActiveDeckIndex(index);
            RefreshDeck();
            RefreshInventory();
        }

        private void RefreshDeck()
        {
            if (_view == null)
            {
                return;
            }

            _view.SetActiveDeckCards(_deckService.GetDeck(_currentDeckIndex));
        }

        private void RefreshInventory()
        {
            if (_view == null)
            {
                return;
            }

            var deckCatalogIndices = new HashSet<int>(
                _deckService.GetDeck(_currentDeckIndex)
                    .Where(placed => placed.CatalogIndex >= 0)
                    .Select(placed => placed.CatalogIndex));

            var availableCards = new List<CardDefinition>();
            var availableCatalogIndices = new List<int>();

            for (var i = 0; i < _cardCatalog.Cards.Count; i++)
            {
                var card = _cardCatalog.Cards[i];
                if (card == null || deckCatalogIndices.Contains(i))
                {
                    continue;
                }

                availableCards.Add(card);
                availableCatalogIndices.Add(i);
            }

            var inventoryCards = CardGridPacker.AutoPack(
                availableCards,
                _view.InventoryColumns,
                DeckLayout.InventoryRows,
                availableCatalogIndices);

            _view.SetInventoryCards(inventoryCards);
        }
    }
}
