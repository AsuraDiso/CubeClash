using System;
using System.Collections.Generic;
using Game.Scripts.Core.Data.Cards;
using Game.Shared.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Game.Features.Deck.Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CardGridLayout))]
    public sealed class CardGridView : MonoBehaviour
    {
        private const int CardPoolDefaultCapacity = 12;
        private const int CardPoolMaxSize = 32;

        [SerializeField] private CardGridLayout _layout;
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private Canvas _dragCanvas;
        [SerializeField] private bool _cardDragEnabled = true;

        private readonly List<CardView> _spawnedCards = new();
        private IReadOnlyList<PlacedCard> _placedCards = Array.Empty<PlacedCard>();
        private ObjectPool<CardView> _cardPool;

        public event Action<CardView, PointerEventData> CardDragEnded;

        public int Columns => _layout.Columns;
        public int Rows => _layout.Rows;

        private void Awake()
        {
            _cardPool = new ObjectPool<CardView>(
                createFunc: () => Instantiate(_cardPrefab, _layout.Container),
                actionOnGet: card => card.gameObject.SetActive(true),
                actionOnRelease: card => card.gameObject.SetActive(false),
                actionOnDestroy: card => Destroy(card.gameObject),
                collectionCheck: true,
                defaultCapacity: CardPoolDefaultCapacity,
                maxSize: CardPoolMaxSize);
        }

        private void OnDestroy() => _cardPool.Dispose();

        public void SetCards(IReadOnlyList<PlacedCard> placedCards)
        {
            _placedCards = placedCards ?? Array.Empty<PlacedCard>();
            ClearSpawnedCards();
            SpawnCards();
            _layout.ApplyScaleToFit(_placedCards);
        }

        public void ForEachCard(Action<CardView> action)
        {
            foreach (var card in _spawnedCards)
                action(card);
        }

        public void ClearAllDiceAssignments() => ForEachCard(card => card.ClearDiceAssignments());

        public HashSet<int> CollectAssignedDieIndices()
        {
            var indices = new HashSet<int>();
            ForEachCard(card => card.CollectAssignedDieIndices(indices));
            return indices;
        }

        public void ReleaseAssignedConsumedDice(Func<int, bool> isConsumed, Action<DiceView> releaseDie) =>
            ForEachCard(card => card.ReleaseAssignedConsumedDice(isConsumed, releaseDie));

        public void SetCardDragEnabled(bool enabled) => _cardDragEnabled = enabled;

        public void ClearCardDiceAssignments(int deckCardIndex)
        {
            ForEachCard(card =>
            {
                if (card.DeckIndex == deckCardIndex)
                    card.ClearDiceAssignments();
            });
        }

        public bool TryGetGridOriginFromScreenPoint(Vector2 screenPoint, Camera eventCamera, out Vector2Int origin) =>
            _layout.TryGetGridOriginFromScreenPoint(screenPoint, eventCamera, out origin);

        private void SpawnCards()
        {
            _layout.ApplyContentSize(_placedCards);

            for (var i = 0; i < _placedCards.Count; i++)
            {
                var placed = _placedCards[i];
                var view = _cardPool.Get();
                view.Initialize(this, _dragCanvas, _cardDragEnabled);
                view.Bind(placed.Definition, placed.CatalogIndex, i);
                view.CardDragEnded += HandleCardDragEnded;
                _layout.LayoutCard(view.RectTransform, placed.Origin, placed.Definition.Footprint);
                _spawnedCards.Add(view);
            }
        }

        private void HandleCardDragEnded(CardView card, PointerEventData eventData) =>
            CardDragEnded?.Invoke(card, eventData);

        private void ClearSpawnedCards()
        {
            for (var i = _spawnedCards.Count - 1; i >= 0; i--)
            {
                var card = _spawnedCards[i];
                card.CardDragEnded -= HandleCardDragEnded;
                _cardPool.Release(card);
            }

            _spawnedCards.Clear();
        }
    }
}
