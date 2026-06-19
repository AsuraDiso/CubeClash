using System;
using Bootstrap.Common;
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
            UnbindView();
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
            UnbindView();
            _deckService.OnDeckChanged -= HandleDeckChanged;
        }

        private void UnbindView() =>
            ViewBinding.Unbind(ref _view, view =>
            {
                view.OnDeckSelected -= SelectDeck;
                view.OnCardDropped -= HandleCardDropped;
                view.OnCardDragEnded -= HandleCardDragEnded;
            });

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
            if (index == _currentDeckIndex)
            {
                Refresh();
            }
        }

        private void SelectDeck(int index)
        {
            _currentDeckIndex = index;
            _view?.SetActiveDeckIndex(index);
            Refresh();
        }

        private void Refresh()
        {
            if (_view == null)
            {
                return;
            }

            var deck = _deckService.GetDeck(_currentDeckIndex);
            _view.SetActiveDeckCards(deck);
            _view.SetInventoryCards(_cardCatalog.PackInventoryNotInDeck(
                deck,
                _view.InventoryColumns,
                DeckLayout.InventoryRows));
        }
    }
}
