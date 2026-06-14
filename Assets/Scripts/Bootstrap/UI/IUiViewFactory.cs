using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    public interface IUiViewFactory
    {
        MainMenuView CreateMainMenuView();

        MatchmakingOverlayView CreateMatchmakingOverlayView();

        BattleView CreateBattleView();

        void Destroy(Component view);
    }
}
