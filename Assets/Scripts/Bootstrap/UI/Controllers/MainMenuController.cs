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

        private MainMenuView _view;

        public MainMenuController(IUiViewFactory viewFactory, HomeController homeController, CardController cardController)
        {
            _viewFactory = viewFactory;
            _homeController = homeController;
            _cardController = cardController;
        }

        public void Start()
        {
            _view = _viewFactory.CreateMainMenuView();

            var screens = _viewFactory.PopulateMainMenuScreens(_view);

            _homeController.Bind(screens.HomeView);
            _cardController.Bind(screens.DeckView);
        }

        public void Dispose()
        {
            _cardController.Bind(null);
            _homeController.Bind(null);

            if (_view == null) return;

            _viewFactory.Destroy(_view);
            _view = null;
        }
    }
}
