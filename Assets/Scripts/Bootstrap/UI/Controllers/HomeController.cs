using System;
using System.Threading;
using Bootstrap.Common;
using Bootstrap.UI;
using Bootstrap.UI.Views;
using Core.Matchmaking;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Bootstrap.UI.Controllers
{
    public sealed class HomeController : IDisposable, IStartable
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly IUiViewFactory _viewFactory;
        private readonly CardController _cardController;
        private readonly SettingsController _settingsController;

        private Component _screenView;
        private CancellationTokenSource _matchmakingCts;

        public HomeController(IMatchmakingService matchmakingService, IUiViewFactory viewFactory,
            CardController cardController, SettingsController settingsController)
        {
            _matchmakingService = matchmakingService;
            _viewFactory = viewFactory;
            _cardController = cardController;
            _settingsController = settingsController;
        }

        public void Start() => ShowHome();

        public void Dispose()
        {
            _matchmakingCts?.Cancel();
            _matchmakingCts?.Dispose();
            _matchmakingCts = null;
            CloseScreen();
        }

        private void ShowHome()
        {
            CloseScreen();
            var view = _viewFactory.CreateHomeView();
            view.PlayClicked += OnPlay;
            view.SettingsClicked += ShowSettings;
            view.EventsClicked += ShowEvents;
            view.DeckClicked += ShowDeck;
            view.ShopClicked += ShowShop;
            _screenView = view;
        }

        private void ShowSettings() => ShowScreen(_viewFactory.CreateSettingsView(), _settingsController.Bind);
        private void ShowDeck() => ShowScreen(_viewFactory.CreateDeckView(), _cardController.Bind);
        private void ShowEvents() => ShowScreen(_viewFactory.CreateEventsView());
        private void ShowShop() => ShowScreen(_viewFactory.CreateShopView());

        private void ShowScreen<T>(T view, Action<T> bind = null) where T : Component, INavigableView
        {
            CloseScreen();
            view.BackClicked += ShowHome;
            bind?.Invoke(view);
            _screenView = view;
        }

        private void CloseScreen()
        {
            if (_screenView == null)
                return;

            switch (_screenView)
            {
                case HomeView home:
                    home.PlayClicked -= OnPlay;
                    home.SettingsClicked -= ShowSettings;
                    home.EventsClicked -= ShowEvents;
                    home.DeckClicked -= ShowDeck;
                    home.ShopClicked -= ShowShop;
                    break;
                case SettingsView settings:
                    settings.BackClicked -= ShowHome;
                    _settingsController.Bind(null);
                    break;
                case DeckView deck:
                    deck.BackClicked -= ShowHome;
                    _cardController.Bind(null);
                    break;
                case INavigableView navigable:
                    navigable.BackClicked -= ShowHome;
                    break;
            }

            _viewFactory.Destroy(_screenView);
            _screenView = null;
        }

        private void OnPlay()
        {
            _matchmakingCts?.Cancel();
            _matchmakingCts?.Dispose();
            _matchmakingCts = new CancellationTokenSource();
            FireAndForget.Run(StartMatchAsync, _matchmakingCts.Token);
        }

        private UniTask StartMatchAsync(CancellationToken cancellationToken) =>
            _matchmakingService.StartQuickMatchAsync(cancellationToken);
    }
}
