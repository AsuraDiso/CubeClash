using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    [CreateAssetMenu(menuName = "CubeClash/UI Prefab Catalog", fileName = "UiPrefabCatalog")]
    public sealed class UiPrefabCatalog : ScriptableObject
    {
        [SerializeField] private HomeView _homeViewPrefab;
        [SerializeField] private SettingsView _settingsViewPrefab;
        [SerializeField] private DeckView _deckViewPrefab;
        [SerializeField] private ShopView _shopViewPrefab;
        [SerializeField] private EventsView _eventsViewPrefab;
        [SerializeField] private MatchmakingOverlayView _matchmakingOverlayViewPrefab;
        [SerializeField] private BattleView _battleViewPrefab;
        [SerializeField] private LoadingView _loadingViewPrefab;

        public HomeView HomeViewPrefab => _homeViewPrefab;
        public MatchmakingOverlayView MatchmakingOverlayViewPrefab => _matchmakingOverlayViewPrefab;
        public BattleView BattleViewPrefab => _battleViewPrefab;
        public LoadingView LoadingViewPrefab => _loadingViewPrefab;

        public SettingsView SettingsViewPrefab => _settingsViewPrefab;
        public DeckView DeckViewPrefab => _deckViewPrefab;
        public ShopView ShopViewPrefab => _shopViewPrefab;
        public EventsView EventsViewPrefab => _eventsViewPrefab;
    }
}
