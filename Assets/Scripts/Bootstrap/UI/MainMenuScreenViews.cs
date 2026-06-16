using Bootstrap.UI.Views;

namespace Bootstrap.UI
{
    public readonly struct MainMenuScreenViews
    {
        public HomeView HomeView { get; }
        public DeckView DeckView { get; }

        public MainMenuScreenViews(HomeView homeView, DeckView deckView)
        {
            HomeView = homeView;
            DeckView = deckView;
        }
    }
}
