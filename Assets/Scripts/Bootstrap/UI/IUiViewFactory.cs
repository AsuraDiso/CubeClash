using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    public interface IUiViewFactory
    {
        MainMenuView CreateMainMenuView();

        MainMenuScreenViews PopulateMainMenuScreens(MainMenuView mainMenuView);

        MatchmakingOverlayView CreateMatchmakingOverlayView();

        BattleView CreateBattleView();

        void Destroy(Component view);
    }
}
