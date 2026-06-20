using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    [CreateAssetMenu(menuName = "CubeClash/UI Prefab Catalog", fileName = "UiPrefabCatalog")]
    public sealed class UiPrefabCatalog : ScriptableObject
    {
        [SerializeField] private MainMenuView _mainMenuViewPrefab;
        [SerializeField] private RectTransform[] _mainMenuScreenPrefabs;
        [SerializeField] private MatchmakingOverlayView _matchmakingOverlayViewPrefab;
        [SerializeField] private BattleView _battleViewPrefab;
        [SerializeField] private LoadingView _loadingViewPrefab;

        public MainMenuView MainMenuViewPrefab => _mainMenuViewPrefab;
        public MatchmakingOverlayView MatchmakingOverlayViewPrefab => _matchmakingOverlayViewPrefab;
        public BattleView BattleViewPrefab => _battleViewPrefab;
        public LoadingView LoadingViewPrefab => _loadingViewPrefab;

        public RectTransform GetMainMenuScreenPrefab(MainMenuTab tab)
        {
            var index = (int)tab;
            return index >= 0 && index < _mainMenuScreenPrefabs.Length
                ? _mainMenuScreenPrefabs[index]
                : null;
        }
    }
}
