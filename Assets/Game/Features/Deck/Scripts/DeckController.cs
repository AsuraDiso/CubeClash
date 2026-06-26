using System;
using System.Collections.Generic;
using Game.Scripts.Bootstrap.Navigation;
using Game.Scripts.Core.Data.Cards;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Settings;
using UnityEngine.EventSystems;

namespace Game.Features.Deck.Scripts
{
    public sealed class DeckController : IDisposable, IScreenShownHandler
    {
        private readonly DeckView _view;
        private readonly CardCatalog _cardCatalog;
        private readonly IDeckService _deckService;
        private readonly IHapticsService _haptics;

        private int _currentDeckIndex;
        private bool _inventoryInitialized;
        private HashSet<int> _deckCatalogIndices = new();

        public DeckController(DeckView view, CardCatalog cardCatalog, IDeckService deckService, IHapticsService haptics)
        {
            _view = view;
            _cardCatalog = cardCatalog;
            _deckService = deckService;
            _haptics = haptics;

            _view.DeckSelected += SelectDeck;
            _view.CardDropped += HandleCardDropped;
            _view.CardDragEnded += HandleCardDragEnded;
            _deckService.DeckChanged += HandleDeckChanged;
        }

        public void OnScreenShown() => SelectDeck(_deckService.SelectedDeckIndex);

        public void Dispose()
        {
            _view.DeckSelected -= SelectDeck;
            _view.CardDropped -= HandleCardDropped;
            _view.CardDragEnded -= HandleCardDragEnded;
            _deckService.DeckChanged -= HandleDeckChanged;
        }

        private void HandleCardDragEnded(CardView cardView, PointerEventData eventData)
        {
            if (_view.ConsumeDeckDrop())
                return;

            if (_view.IsInActiveDeck(cardView))
            {
                if (_deckService.TryRemoveCard(_currentDeckIndex, cardView.CatalogIndex))
                    _haptics.PlayLight();
            }
        }

        private void HandleCardDropped(CardView cardView, PointerEventData eventData)
        {
            if (_view.TryGetActiveGridOrigin(eventData.position, eventData.pressEventCamera, out var origin)
                && _deckService.TryPlaceCard(_currentDeckIndex, cardView.Definition, origin, cardView.CatalogIndex))
            {
                _haptics.PlayLight();
            }
        }

        private void HandleDeckChanged(int index)
        {
            if (index == _currentDeckIndex)
                Refresh();
        }

        private void SelectDeck(int index)
        {
            _currentDeckIndex = index;
            _deckService.SetSelectedDeckIndex(index);
            _view.SetActiveDeckIndex(index);
            Refresh();
        }

        private void Refresh()
        {
            var deck = _deckService.GetDeck(_currentDeckIndex);
            _view.SetActiveDeckCards(deck);

            if (_inventoryInitialized && !HaveInventoryMembershipChanged(deck))
                return;

            _inventoryInitialized = true;
            _view.SetInventoryCards(_cardCatalog.PackInventoryNotInDeck(deck, _view.InventoryColumns, DeckLayout.InventoryRows));
        }

        private readonly HashSet<int> _nextDeckCatalogIndices = new();

        private bool HaveInventoryMembershipChanged(IReadOnlyList<PlacedCard> deck)
        {
            _nextDeckCatalogIndices.Clear();
            foreach (var placed in deck)
            {
                if (placed.CatalogIndex >= 0)
                    _nextDeckCatalogIndices.Add(placed.CatalogIndex);
            }

            if (_nextDeckCatalogIndices.SetEquals(_deckCatalogIndices))
                return false;

            var temp = _deckCatalogIndices;
            _deckCatalogIndices = _nextDeckCatalogIndices;
            temp.Clear();

            return true;
        }
    }
}
