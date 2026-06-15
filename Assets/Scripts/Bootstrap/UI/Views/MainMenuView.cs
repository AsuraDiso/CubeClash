using System;
using UnityEngine;

namespace Bootstrap.UI.Views
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private MainMenuScreenTransition _screenTransition;
        [SerializeField] private Transform _navButtonsRoot;

        public event Action<MainMenuTab> TabChanged;

        public MainMenuTab CurrentTab => _screenTransition.CurrentTab;

        public MainMenuScreenTransition ScreenTransition => _screenTransition;

        public RectTransform ScreensRoot => _screenTransition.ScreensRoot;

        public void InitializeScreens(RectTransform[] screens)
        {
            _screenTransition.Initialize(screens);
            _screenTransition.ShowTab(MainMenuTab.Home, instant: true);
        }

        public void OnEventsButtonClick() => SelectTab(MainMenuTab.Events);

        public void OnDeckButtonClick() => SelectTab(MainMenuTab.Deck);

        public void OnHomeButtonClick() => SelectTab(MainMenuTab.Home);

        public void OnShopButtonClick() => SelectTab(MainMenuTab.Shop);

        public void OnSettingsButtonClick() => SelectTab(MainMenuTab.Settings);

        private void SelectTab(MainMenuTab tab)
        {
            if (tab == _screenTransition.CurrentTab)
                return;

            _screenTransition.ShowTab(tab);
            TabChanged?.Invoke(tab);
        }
    }
}
