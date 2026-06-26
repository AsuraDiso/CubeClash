using System;
using System.Collections.Generic;
using Game.Scripts.Core.Data.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Features.Deck.Scripts
{
    [DisallowMultipleComponent]
    public sealed class CardGridLayout : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private RectTransform _container;
        [SerializeField] private Vector2 _cellSize = new(197.0655f, 205f);
        [SerializeField] private Vector2 _spacing = new(8f, 8f);
        [SerializeField] private int _columns = DeckLayout.Columns;
        [SerializeField] private int _rows = DeckLayout.Rows;

        [Header("Scaling")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _scaleRoot;
        [SerializeField] private RectTransform _fitBounds;
        [SerializeField] private bool _scaleToFit = true;
        [SerializeField] private int _scaleToFitRows;

        private Vector2 _lastFitBoundsSize = Vector2.negativeInfinity;

        public int Columns => Mathf.Max(1, _columns);
        public int Rows => Mathf.Max(1, _rows);
        public RectTransform Container => _container;

        private void OnEnable() => _lastFitBoundsSize = Vector2.negativeInfinity;

        private void LateUpdate()
        {
            if (!IsScaleToFitEnabled()) return;

            Vector2 size = _fitBounds.rect.size;
            if ((size - _lastFitBoundsSize).sqrMagnitude < 0.25f) return;

            _lastFitBoundsSize = size;
            ApplyScaleToFit(Array.Empty<PlacedCard>());
        }

        public bool TryGetGridOriginFromScreenPoint(Vector2 screenPoint, Camera eventCamera, out Vector2Int origin)
        {
            origin = Vector2Int.zero;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, screenPoint, eventCamera, out var localPoint))
                return false;

            Rect rect = _container.rect;
            int column = Mathf.FloorToInt((localPoint.x - rect.xMin) / (_cellSize.x + _spacing.x));
            int row = Mathf.FloorToInt((rect.yMax - localPoint.y) / (_cellSize.y + _spacing.y));

            if (column < 0 || column >= Columns || row < 0 || row >= Rows) return false;

            origin = new Vector2Int(column, row);
            return true;
        }

        public void LayoutCard(RectTransform rectTransform, Vector2Int origin, CardFootprintSize footprint)
        {
            var pos = new Vector2(origin.x * (_cellSize.x + _spacing.x), -origin.y * (_cellSize.y + _spacing.y));
            var size = new Vector2(
                footprint.Columns * _cellSize.x + (footprint.Columns - 1) * _spacing.x,
                footprint.Rows * _cellSize.y + (footprint.Rows - 1) * _spacing.y);

            rectTransform.SetParent(_container, false);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = pos;
            rectTransform.sizeDelta = size;
        }

        public void ApplyContentSize(IReadOnlyList<PlacedCard> placedCards)
        {
            if (_scrollRect == null)
                return;

            Vector2 size = ComputeGridSize(GetContentRows(placedCards));
            RectTransform content = _scrollRect.content;
            PreserveScrollPosition(() =>
            {
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            });
        }

        public void ApplyScaleToFit(IReadOnlyList<PlacedCard> placedCards)
        {
            if (_scaleRoot == null)
                return;

            PrepareFitBounds();
            if (!IsScaleToFitEnabled())
            {
                ResetScaleToDefault();
                return;
            }

            if (!TryComputeFitScale(placedCards, out var scale, out var boundsSize))
                return;

            if (Mathf.Approximately(_scaleRoot.localScale.x, scale))
            {
                _lastFitBoundsSize = boundsSize;
                return;
            }

            ApplyComputedScale(scale, boundsSize);
        }

        private void PrepareFitBounds()
        {
            if (_fitBounds != null && _fitBounds != _scaleRoot)
                _fitBounds.localScale = Vector3.one;
        }

        private bool IsScaleToFitEnabled() => _scaleToFit && _fitBounds != null && _scaleRoot != null;

        private void ResetScaleToDefault()
        {
            if (_scaleRoot == null)
                return;

            PreserveScrollPosition(() => _scaleRoot.localScale = Vector3.one);
        }

        private bool TryComputeFitScale(IReadOnlyList<PlacedCard> placedCards, out float scale, out Vector2 boundsSize)
        {
            scale = 1f;
            boundsSize = default;

            int rows = _scaleToFitRows > 0 ? _scaleToFitRows : GetContentRows(placedCards);
            Vector2 gridSize = ComputeGridSize(rows);
            Rect bounds = _fitBounds.rect;
            boundsSize = bounds.size;

            if (gridSize.x <= 0f || gridSize.y <= 0f || bounds.width <= 0f || bounds.height <= 0f)
                return false;

            scale = Mathf.Min(bounds.width / gridSize.x, bounds.height / gridSize.y);
            return true;
        }

        private void ApplyComputedScale(float scale, Vector2 boundsSize)
        {
            PreserveScrollPosition(() => _scaleRoot.localScale = new Vector3(scale, scale, 1f));
            _lastFitBoundsSize = boundsSize;
        }

        private void PreserveScrollPosition(Action mutate)
        {
            if (_scrollRect == null)
            {
                mutate();
                return;
            }

            float vertical = _scrollRect.verticalNormalizedPosition;
            float horizontal = _scrollRect.horizontalNormalizedPosition;
            mutate();
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = vertical;
            _scrollRect.horizontalNormalizedPosition = horizontal;
        }

        private int GetContentRows(IReadOnlyList<PlacedCard> placedCards)
        {
            int maxRow = 0;
            if (placedCards != null)
            {
                foreach (var placed in placedCards)
                    maxRow = Mathf.Max(maxRow, placed.Origin.y + placed.Definition.Footprint.Rows);
            }

            return Mathf.Clamp(Mathf.Max(maxRow, 1), 1, Rows);
        }

        private Vector2 ComputeGridSize(int rows) =>
            new(Columns * _cellSize.x + (Columns - 1) * _spacing.x, rows * _cellSize.y + (rows - 1) * _spacing.y);
    }
}
