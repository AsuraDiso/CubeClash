using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    public interface IUiViewFactory
    {
        MainMenuView CreateMainMenuView();

        (HomeView Home, DeckView Deck) PopulateMainMenuScreens(MainMenuView mainMenuView);

        MatchmakingOverlayView CreateMatchmakingOverlayView();

        BattleView CreateBattleView();

        void Destroy(Component view);
    }
}
