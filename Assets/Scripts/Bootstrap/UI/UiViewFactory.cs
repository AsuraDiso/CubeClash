using Bootstrap.UI.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace Bootstrap.UI
{
    public sealed class UiViewFactory : IUiViewFactory
    {
        private readonly IObjectResolver _objectResolver;
        private readonly UiPrefabCatalog _catalog;

        public UiViewFactory(IObjectResolver objectResolver, UiPrefabCatalog catalog)
        {
            _objectResolver = objectResolver;
            _catalog = catalog;
        }

        public MainMenuView CreateMainMenuView() => _objectResolver.Instantiate(_catalog.MainMenuViewPrefab);

        public (HomeView Home, DeckView Deck, SettingsView Settings) PopulateMainMenuScreens(MainMenuView mainMenuView)
        {
            var screensRoot = mainMenuView.ScreensRoot;
            ClearChildren(screensRoot);

            var screenCount = (int)MainMenuTab.Settings + 1;
            var screens = new RectTransform[screenCount];
            HomeView homeView = null;
            DeckView deckView = null;
            SettingsView settingsView = null;

            for (var tab = MainMenuTab.Events; tab <= MainMenuTab.Settings; tab++)
            {
                var screen = _objectResolver.Instantiate(_catalog.GetMainMenuScreenPrefab(tab), screensRoot);
                StretchToParent(screen);
                screen.gameObject.SetActive(false);
                screens[(int)tab] = screen;

                if (tab == MainMenuTab.Home)
                {
                    homeView = screen.GetComponent<HomeView>();
                }
                else if (tab == MainMenuTab.Deck)
                {
                    deckView = screen.GetComponent<DeckView>();
                }
                else if (tab == MainMenuTab.Settings)
                {
                    settingsView = screen.GetComponent<SettingsView>();
                }
            }

            mainMenuView.InitializeScreens(screens);
            return (homeView, deckView, settingsView);
        }

        public MatchmakingOverlayView CreateMatchmakingOverlayView() =>
            _objectResolver.Instantiate(_catalog.MatchmakingOverlayViewPrefab);

        public BattleView CreateBattleView() => _objectResolver.Instantiate(_catalog.BattleViewPrefab);

        public void Destroy(Component view)
        {
            if (view != null)
            {
                Object.Destroy(view.gameObject);
            }
        }

        private static void ClearChildren(RectTransform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
