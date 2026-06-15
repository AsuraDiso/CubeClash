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

        private MainMenuView _view;

        public MainMenuController(IUiViewFactory viewFactory, HomeController homeController)
        {
            _viewFactory = viewFactory;
            _homeController = homeController;
        }

        public void Start()
        {
            _view = _viewFactory.CreateMainMenuView();
            if (_view == null)
            {
                return;
            }

            var homeView = _viewFactory.PopulateMainMenuScreens(_view);
            _homeController.Bind(homeView);
        }

        public void Dispose()
        {
            _homeController.Bind(null);

            if (_view == null) return;
            _viewFactory.Destroy(_view);
            _view = null;
        }
    }
}
