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
            if (prefab != null) return _objectResolver.Instantiate(prefab);
            Debug.LogError($"[CubeClash] UiPrefabCatalog.{prefabName} is not assigned.");
            return null;
        }
    }
}
