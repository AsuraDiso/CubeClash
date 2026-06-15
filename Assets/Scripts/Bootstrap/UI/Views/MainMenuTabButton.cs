using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public class MainMenuTabButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MainMenuTab _tabType;
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _textCanvasGroup;

        [Header("Sprites")]
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _selectedSprite;

        [Header("Animation")]
        [SerializeField] private float _fadeDuration = 0.2f;

        private void OnEnable()
        {
            _mainMenuView.TabChanged += OnTabChanged;
            // Initialize to current state without animation
            UpdateState(_mainMenuView.CurrentTab, instant: true);
        }

        private void OnDisable()
        {
            _mainMenuView.TabChanged -= OnTabChanged;
        }

        private void OnTabChanged(MainMenuTab newTab)
        {
            UpdateState(newTab, instant: false);
        }

        private void UpdateState(MainMenuTab activeTab, bool instant)
        {
            bool isSelected = (activeTab == _tabType);

            // Change texture (Sprite)
            _backgroundImage.sprite = isSelected ? _selectedSprite : _normalSprite;

            // Fade Text
            _textCanvasGroup.DOKill();
            float targetAlpha = isSelected ? 1f : 0f;

            if (instant)
            {
                _textCanvasGroup.alpha = targetAlpha;
            }
            else
            {
                _textCanvasGroup.DOFade(targetAlpha, _fadeDuration);
            }
        }
    }
}
