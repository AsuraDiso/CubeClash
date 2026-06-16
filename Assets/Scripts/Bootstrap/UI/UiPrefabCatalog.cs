using Bootstrap.UI;
using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    [CreateAssetMenu(menuName = "CubeClash/UI Prefab Catalog", fileName = "UiPrefabCatalog")]
    public sealed class UiPrefabCatalog : ScriptableObject
    {
        [SerializeField] private MainMenuView _mainMenuViewPrefab;
        [SerializeField] private RectTransform[] _mainMenuScreenPrefabs;
        [SerializeField] private HomeView _homeScreenPrefab;
        [SerializeField] private DeckView _deckScreenPrefab;
        [SerializeField] private MatchmakingOverlayView _matchmakingOverlayViewPrefab;
        [SerializeField] private BattleView _battleViewPrefab;

        public MainMenuView MainMenuViewPrefab => _mainMenuViewPrefab;
        public HomeView HomeScreenPrefab => _homeScreenPrefab;
        public DeckView DeckScreenPrefab => _deckScreenPrefab;
        public MatchmakingOverlayView MatchmakingOverlayViewPrefab => _matchmakingOverlayViewPrefab;
        public BattleView BattleViewPrefab => _battleViewPrefab;

        public RectTransform GetMainMenuScreenPrefab(MainMenuTab tab)
        {
            return tab switch
            {
                MainMenuTab.Home => _homeScreenPrefab.transform as RectTransform,
                MainMenuTab.Deck => _deckScreenPrefab.transform as RectTransform,
                _ => GetGenericMainMenuScreenPrefab(tab)
            };
        }

        private RectTransform GetGenericMainMenuScreenPrefab(MainMenuTab tab)
        {
            var index = (int)tab;
            if (index < 0 || index >= _mainMenuScreenPrefabs.Length)
            {
                return null;
            }

            return _mainMenuScreenPrefabs[index];
        }
    }
}
