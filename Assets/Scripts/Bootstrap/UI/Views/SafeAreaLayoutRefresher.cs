using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaLayoutRefresher : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            CacheScreenState();
        }

        private void LateUpdate()
        {
            if (Screen.width == _lastScreenSize.x
                && Screen.height == _lastScreenSize.y
                && Screen.orientation == _lastOrientation
                && Screen.safeArea == _lastSafeArea)
            {
                return;
            }

            CacheScreenState();
            Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            Refresh();
        }

        private void CacheScreenState()
        {
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            _lastOrientation = Screen.orientation;
            _lastSafeArea = Screen.safeArea;
        }

        private void Refresh()
        {
            _rectTransform ??= GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
    }
}
