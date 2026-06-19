using System;
using Bootstrap.UI;
using Bootstrap.UI.Views;
using VContainer.Unity;

namespace Bootstrap.UI.Controllers
{
    public sealed class MainMenuController : IStartable, IDisposable
    {
        private readonly IUiViewFactory _viewFactory;
        private readonly HomeController _homeController;
        private readonly CardController _cardController;

        private readonly SettingsController _settingsController;

        private MainMenuView _view;

        public MainMenuController(
            IUiViewFactory viewFactory,
            HomeController homeController,
            CardController cardController,
            SettingsController settingsController)
        {
            _viewFactory = viewFactory;
            _homeController = homeController;
            _cardController = cardController;
            _settingsController = settingsController;
        }

        public void Start()
        {
            _view = _viewFactory.CreateMainMenuView();
            var (homeView, deckView, settingsView) = _viewFactory.PopulateMainMenuScreens(_view);
            _homeController.Bind(homeView);
            _cardController.Bind(deckView);
            _settingsController.Bind(settingsView);
        }

        public void Dispose()
        {
            _settingsController.Bind(null);
            _homeController.Bind(null);

            if (_view == null)
            {
                return;
            }

            _viewFactory.Destroy(_view);
            _view = null;
        }
    }
}
