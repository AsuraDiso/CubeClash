using System;
using System.Collections.Generic;
using System.Threading;
using Cards;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bootstrap.UI.Views
{
    [DisallowMultipleComponent]
    public sealed class CardGridView : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private List<RectTransform> _backgroundCells = new();
        [SerializeField] private int _columns = DeckLayout.Columns;
        [SerializeField] private int _rows = DeckLayout.Rows;
        [SerializeField] private Vector2 _spacing = new(8f, 8f);

        private readonly List<CardView> _spawnedCards = new();
        private IReadOnlyList<PlacedCard> _placedCards = Array.Empty<PlacedCard>();

        private CancellationTokenSource _refreshCts;

        public event Action<CardView, PointerEventData> CardDragEnded;

        public int Columns => Mathf.Max(1, _columns);
        public int Rows => Mathf.Max(1, _rows);

        private Canvas _dragCanvas;

        public void SetDragCanvas(Canvas dragCanvas) => _dragCanvas = dragCanvas;

        private void OnEnable()
        {
            ScheduleRefresh(true);
        }

        private void OnDisable()
        {
            CancelRefresh();
        }

        private void OnDestroy()
        {
            CancelRefresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            ScheduleRefresh(false);
        }

        public void SetCards(IReadOnlyList<PlacedCard> placedCards)
        {
            _placedCards = placedCards ?? Array.Empty<PlacedCard>();

            ClearSpawnedCards();
            ScheduleRefresh(true);
        }

        private void ScheduleRefresh(bool forceRestart)
        {
            if (_refreshCts != null)
            {
                if (!forceRestart) return;

                CancelRefresh();
            }

            _refreshCts = new CancellationTokenSource();
            RefreshAfterLayoutAsync(_refreshCts.Token).Forget();
        }

        private async UniTaskVoid RefreshAfterLayoutAsync(CancellationToken cancellationToken)
        {
            const int maxAttempts = 5;

            try
            {
                for (var attempt = 0; attempt < maxAttempts; attempt++)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                    Canvas.ForceUpdateCanvases();

                    if (!isActiveAndEnabled) continue;

                    TryRebuildIfNeeded();
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_refreshCts is { IsCancellationRequested: false })
                {
                    _refreshCts.Dispose();
                    _refreshCts = null;
                }
            }
        }

        private bool TryRebuildIfNeeded()
        {
            if (_spawnedCards.Count > 0 || _backgroundCells.Count > 0)
            {
                Relayout();
                if (_spawnedCards.Count > 0) return true;
            }

            if (_container == null)
                return false;

            var cellSize = ComputeCellSize();

            if (cellSize.x <= 0f || cellSize.y <= 0f)
                return false;

            RelayoutBackgroundCells(cellSize);

            if (_cardPrefab == null)
                return false;

            foreach (var placed in _placedCards)
            {
                if (placed.Definition == null) continue;

                var view = Instantiate(_cardPrefab, _container);

                view.Initialize(this, _dragCanvas);
                view.Bind(placed.Definition, placed.CatalogIndex);
                view.DragEnded += HandleCardDragEnded;

                LayoutCard(
                    view.RectTransform,
                    placed.Origin,
                    placed.Definition.Footprint,
                    cellSize);

                _spawnedCards.Add(view);
            }

            return _spawnedCards.Count > 0;
        }

        private void RelayoutBackgroundCells(Vector2 cellSize)
        {
            if (_backgroundCells == null || _backgroundCells.Count == 0) return;

            int cellIdx = 0;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (cellIdx < _backgroundCells.Count)
                    {
                        var cellGo = _backgroundCells[cellIdx];
                        if (cellGo != null)
                        {
                            LayoutCard(cellGo, new Vector2Int(c, r), new CardFootprintSize(1, 1), cellSize);
                            cellGo.SetAsFirstSibling();
                        }
                        cellIdx++;
                    }
                }
            }
        }

        private void Relayout()
        {
            if (_container == null)
                return;

            var cellSize = ComputeCellSize();

            if (cellSize.x <= 0f || cellSize.y <= 0f)
                return;

            RelayoutBackgroundCells(cellSize);

            if (_spawnedCards.Count == 0 || _placedCards.Count == 0)
                return;

            var count = Mathf.Min(
                _spawnedCards.Count,
                _placedCards.Count);

            for (var i = 0; i < count; i++)
            {
                var view = _spawnedCards[i];
                var placed = _placedCards[i];

                if (view == null || placed.Definition == null)
                    continue;

                LayoutCard(
                    view.RectTransform,
                    placed.Origin,
                    placed.Definition.Footprint,
                    cellSize);
            }
        }

        private Vector2 ComputeCellSize()
        {
            var rect = _container.rect;

            var columns = Columns;
            var rows = Rows;

            var cellWidth =
                (rect.width - _spacing.x * (columns - 1)) / columns;

            var cellHeight =
                (rect.height - _spacing.y * (rows - 1)) / rows;

            return new Vector2(cellWidth, cellHeight);
        }

        public bool TryGetGridOriginFromScreenPoint(Vector2 screenPoint, Camera eventCamera, out Vector2Int origin)
        {
            origin = Vector2Int.zero;
            if (_container == null) return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, screenPoint, eventCamera, out var localPoint))
            {
                return false;
            }

            var cellSize = ComputeCellSize();
            if (cellSize.x <= 0f || cellSize.y <= 0f) return false;

            var rect = _container.rect;
            var xFromLeft = localPoint.x - rect.xMin;
            var yFromTop = rect.yMax - localPoint.y;

            int col = Mathf.FloorToInt(xFromLeft / (cellSize.x + _spacing.x));
            int row = Mathf.FloorToInt(yFromTop / (cellSize.y + _spacing.y));

            if (col < 0 || col >= Columns || row < 0 || row >= Rows)
            {
                return false;
            }

            origin = new Vector2Int(col, row);
            return true;
        }

        private void LayoutCard(RectTransform rectTransform, Vector2Int origin, CardFootprintSize footprint,
            Vector2 cellSize)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);

            var x = origin.x * (cellSize.x + _spacing.x);
            var y = -origin.y * (cellSize.y + _spacing.y);

            var width =
                footprint.Columns * cellSize.x +
                (footprint.Columns - 1) * _spacing.x;

            var height =
                footprint.Rows * cellSize.y +
                (footprint.Rows - 1) * _spacing.y;

            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        private void HandleCardDragEnded(CardView card, PointerEventData eventData)
        {
            CardDragEnded?.Invoke(card, eventData);
        }

        private void ClearSpawnedCards()
        {
            for (var i = _spawnedCards.Count - 1; i >= 0; i--)
            {
                var card = _spawnedCards[i];
                if (card != null)
                {
                    card.DragEnded -= HandleCardDragEnded;
                    Destroy(card.gameObject);
                }
            }

            _spawnedCards.Clear();
        }

        private void CancelRefresh()
        {
            if (_refreshCts == null)
                return;

            _refreshCts.Cancel();
            _refreshCts.Dispose();
            _refreshCts = null;
        }
    }
}
