using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public sealed class MainMenuScreenTransition : MonoBehaviour
    {
        [SerializeField] private RectTransform _screensRoot;
        [SerializeField] private float _duration = 0.4f;

        private RectTransform[] _screens;
        private bool _isTransitioning;
        private Sequence _activeSequence;

        public MainMenuTab CurrentTab { get; private set; } = MainMenuTab.Home;
        public RectTransform ScreensRoot => _screensRoot;

        private void OnDestroy()
        {
            _activeSequence?.Kill();
        }

        public void Initialize(RectTransform[] screens)
        {
            _screens = screens;
        }

        public void ShowTab(MainMenuTab tab, bool instant = false)
        {
            if (_screens.Length == 0) return;

            tab = ClampTab(tab);

            if (tab == CurrentTab && !instant) return;

            if (instant)
            {
                _activeSequence?.Kill();
                ApplyInstant(tab);
                return;
            }

            if (_isTransitioning) return;

            _activeSequence?.Kill();

            var outgoing = _screens[(int)CurrentTab];
            var incoming = _screens[(int)tab];

            var direction = tab > CurrentTab ? 1 : -1;
            var width = GetTransitionWidth();

            _isTransitioning = true;
            PrepareScreen(outgoing, Vector2.zero, Vector3.zero);
            PrepareScreen(incoming, new Vector2(direction * width, 0f), Vector3.zero);

            incoming.gameObject.SetActive(true);

            _activeSequence = DOTween.Sequence();
            _activeSequence.Join(outgoing.DOAnchorPosX(-direction * width, _duration));
            _activeSequence.Join(incoming.DOAnchorPosX(0f, _duration));
            _activeSequence.Join(incoming.DOLocalRotate(Vector3.zero, _duration));

            _activeSequence.OnComplete(() =>
            {
                outgoing.gameObject.SetActive(false);
                PrepareScreen(outgoing, Vector2.zero, Vector3.zero);
                CurrentTab = tab;
                _isTransitioning = false;
            });
        }

        private static MainMenuTab ClampTab(MainMenuTab tab)
        {
            var tabIndex = (int)tab;

            return tabIndex switch
            {
                < 0 => MainMenuTab.Events,
                > (int)MainMenuTab.Settings => MainMenuTab.Settings,
                _ => tab
            };
        }

        private void ApplyInstant(MainMenuTab tab)
        {
            tab = ClampTab(tab);

            for (var i = 0; i < _screens.Length; i++)
            {
                var screen = _screens[i];

                var isActive = i == (int)tab;
                screen.gameObject.SetActive(isActive);
                if (isActive) PrepareScreen(screen, Vector2.zero, Vector3.zero);
            }

            CurrentTab = tab;
            _isTransitioning = false;
        }

        private static void PrepareScreen(RectTransform screen, Vector2 anchoredPosition, Vector3 localEulerAngles)
        {
            screen.anchoredPosition = anchoredPosition;
            screen.localRotation = Quaternion.Euler(localEulerAngles);
        }

        private float GetTransitionWidth()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_screensRoot);

            var width = _screensRoot.rect.width;
            if (width > 1f) return width;

            var parent = _screensRoot.parent as RectTransform;

            return parent != null && parent.rect.width > 1f ? parent.rect.width : Screen.width;
        }
    }
}
