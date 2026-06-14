using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    [CreateAssetMenu(menuName = "CubeClash/UI Prefab Catalog", fileName = "UiPrefabCatalog")]
    public sealed class UiPrefabCatalog : ScriptableObject
    {
        [SerializeField] private MainMenuView _mainMenuViewPrefab;
        [SerializeField] private MatchmakingOverlayView _matchmakingOverlayViewPrefab;
        [SerializeField] private BattleView _battleViewPrefab;

        public MainMenuView MainMenuViewPrefab => _mainMenuViewPrefab;

        public MatchmakingOverlayView MatchmakingOverlayViewPrefab => _matchmakingOverlayViewPrefab;

        public BattleView BattleViewPrefab => _battleViewPrefab;
    }
}
