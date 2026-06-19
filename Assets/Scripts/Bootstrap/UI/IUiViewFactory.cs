using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    public interface IUiViewFactory
    {
        public MainMenuView CreateMainMenuView();

        public (HomeView Home, DeckView Deck) PopulateMainMenuScreens(MainMenuView mainMenuView);

        public MatchmakingOverlayView CreateMatchmakingOverlayView();

        public BattleView CreateBattleView();

        public void Destroy(Component view);
    }
}
