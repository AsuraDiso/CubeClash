using Bootstrap.UI.Views;
using UnityEngine;

namespace Bootstrap.UI
{
    public interface IUiViewFactory
    {
        HomeView CreateHomeView();

        DeckView CreateDeckView();

        SettingsView CreateSettingsView();

        EventsView CreateEventsView();

        ShopView CreateShopView();

        MatchmakingOverlayView CreateMatchmakingOverlayView();

        BattleView CreateBattleView();

        LoadingView CreateLoadingView();

        void Destroy(Component view);
    }
}
