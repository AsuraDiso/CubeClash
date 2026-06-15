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

        public MainMenuView CreateMainMenuView()
        {
            return CreateView(_catalog.MainMenuViewPrefab, nameof(_catalog.MainMenuViewPrefab));
        }

        public HomeView PopulateMainMenuScreens(MainMenuView mainMenuView)
        {
            if (mainMenuView == null)
            {
                return null;
            }

            var screensRoot = mainMenuView.ScreensRoot;
            var screenTransition = mainMenuView.ScreenTransition;


            ClearChildren(screensRoot);

            var screenCount = (int)MainMenuTab.Settings + 1;
            var screens = new RectTransform[screenCount];

            for (var tab = MainMenuTab.Events; tab <= MainMenuTab.Settings; tab++)
            {
                var prefab = _catalog.GetMainMenuScreenPrefab(tab);


                var screen = _objectResolver.Instantiate(prefab, screensRoot);
                StretchToParent(screen);
                screen.gameObject.SetActive(false);
                screens[(int)tab] = screen;
            }

            mainMenuView.InitializeScreens(screens);

            var homeScreen = screens[(int)MainMenuTab.Home];
            if (homeScreen == null)
            {
                return null;
            }

            var homeView = homeScreen.GetComponentInChildren<HomeView>(true);
            if (homeView == null)
            {
                Debug.LogError("[CubeClash] Home screen prefab is missing HomeView.");
            }

            return homeView;
        }

        public MatchmakingOverlayView CreateMatchmakingOverlayView()
        {
            return CreateView(_catalog.MatchmakingOverlayViewPrefab, nameof(_catalog.MatchmakingOverlayViewPrefab));
        }

        public BattleView CreateBattleView()
        {
            return CreateView(_catalog.BattleViewPrefab, nameof(_catalog.BattleViewPrefab));
        }

        public void Destroy(Component view)
        {
            if (view == null) return;
            Object.Destroy(view.gameObject);
        }

        private T CreateView<T>(T prefab, string prefabName) where T : Component
        {
            return _objectResolver.Instantiate(prefab);
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

