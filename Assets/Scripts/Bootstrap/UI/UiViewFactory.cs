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

        public HomeView CreateHomeView() => InstantiateView(_catalog.HomeViewPrefab);
        public DeckView CreateDeckView() => InstantiateView(_catalog.DeckViewPrefab);
        public SettingsView CreateSettingsView() => InstantiateView(_catalog.SettingsViewPrefab);
        public EventsView CreateEventsView() => InstantiateView(_catalog.EventsViewPrefab);
        public ShopView CreateShopView() => InstantiateView(_catalog.ShopViewPrefab);
        public MatchmakingOverlayView CreateMatchmakingOverlayView() => InstantiateView(_catalog.MatchmakingOverlayViewPrefab);
        public BattleView CreateBattleView() => InstantiateView(_catalog.BattleViewPrefab);
        public LoadingView CreateLoadingView() => InstantiateView(_catalog.LoadingViewPrefab);

        public void Destroy(Component view)
        {
            if (view != null)
                Object.Destroy(view.gameObject);
        }

        private T InstantiateView<T>(T prefab) where T : Component
        {
            var view = _objectResolver.Instantiate(prefab);
            _uiCamera.ConfigureCanvasesInHierarchy(view.gameObject);
            return view;
        }
    }
}
