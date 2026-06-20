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
        private readonly UiCameraRoot _uiCamera;

        public UiViewFactory(IObjectResolver objectResolver, UiPrefabCatalog catalog, UiCameraRoot uiCamera)
        {
            _objectResolver = objectResolver;
            _catalog = catalog;
            _uiCamera = uiCamera;
        }

        public MainMenuView CreateMainMenuView() => InstantiateView(_catalog.MainMenuViewPrefab);

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
                var screen = InstantiateView(_catalog.GetMainMenuScreenPrefab(tab), screensRoot);
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
            InstantiateView(_catalog.MatchmakingOverlayViewPrefab);

        public BattleView CreateBattleView() => InstantiateView(_catalog.BattleViewPrefab);

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

        private T InstantiateView<T>(T prefab, Transform parent = null) where T : Component
        {
            var view = parent == null
                ? _objectResolver.Instantiate(prefab)
                : _objectResolver.Instantiate(prefab, parent);
            _uiCamera.ConfigureCanvasesInHierarchy(view.gameObject);
            return view;
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
