using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class ResponsiveGridLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform _widthSource;
        [SerializeField] private float _cellAspectRatio = 280f / 215f;
        [SerializeField] private int _portraitColumnCount = 3;
        [SerializeField] private float _landscapeTargetCellWidth = 170f;
        [SerializeField] private GridLayoutGroup _grid;

        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;
        private Coroutine _refreshCoroutine;

        private void Awake()
        {
            _grid = GetComponent<GridLayoutGroup>();
            CacheScreenState();
        }

        private void OnEnable()
        {
            ScheduleRefresh(forceRestart: true);
        }

        private void OnRectTransformDimensionsChange()
        {
            ScheduleRefresh(forceRestart: false);
        }

        private void Update()
        {
            if (Screen.width == _lastScreenSize.x
                && Screen.height == _lastScreenSize.y
                && Screen.orientation == _lastOrientation)
            {
                return;
            }

            CacheScreenState();
            ScheduleRefresh(forceRestart: true);
        }

        private void CacheScreenState()
        {
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            _lastOrientation = Screen.orientation;
        }

        private void ScheduleRefresh(bool forceRestart)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_refreshCoroutine != null)
            {
                if (!forceRestart)
                {
                    return;
                }

                StopCoroutine(_refreshCoroutine);
                _refreshCoroutine = null;
            }

            _refreshCoroutine = StartCoroutine(RefreshAfterLayout());
        }

        private IEnumerator RefreshAfterLayout()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            Refresh();

            yield return null;
            Canvas.ForceUpdateCanvases();
            Refresh();

            _refreshCoroutine = null;
        }

        public void Refresh()
        {
            _grid ??= GetComponent<GridLayoutGroup>();
            if (_grid == null)
            {
                return;
            }

            var source = _widthSource != null ? _widthSource : transform.parent as RectTransform;
            if (source == null)
            {
                return;
            }

            var availableWidth = source.rect.width;

            var verticalLayout = source.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout != null)
            {
                availableWidth -= verticalLayout.padding.horizontal;
            }

            availableWidth -= _grid.padding.horizontal;
            if (availableWidth <= 0f)
            {
                return;
            }

            ApplyColumnCount(availableWidth);

            var columns = _grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                ? _grid.constraintCount
                : 1;

            if (columns <= 0)
            {
                return;
            }

            var spacing = _grid.spacing;
            var cellWidth = (availableWidth - spacing.x * (columns - 1)) / columns;
            if (cellWidth <= 0f)
            {
                return;
            }

            var cellHeight = cellWidth * _cellAspectRatio;
            _grid.cellSize = new Vector2(cellWidth, cellHeight);

            var rect = transform as RectTransform;
            if (rect != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rect);
            }

            if (_widthSource != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(_widthSource);
            }
        }

        private void ApplyColumnCount(float availableWidth)
        {
            var isLandscape = Screen.width > Screen.height;
            var columns = isLandscape
                ? Mathf.Max(4, Mathf.FloorToInt((availableWidth + _grid.spacing.x) / (_landscapeTargetCellWidth + _grid.spacing.x)))
                : _portraitColumnCount;

            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = columns;
        }
    }
}
