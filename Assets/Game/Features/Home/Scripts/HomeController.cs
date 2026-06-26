using System;
using Game.Features.Deck.Scripts;
using Game.Features.Matchmaking.Scripts;
using Game.Features.Settings.Scripts;
using Game.Scripts.Bootstrap.Navigation;
using Game.Scripts.Core.Settings;

namespace Game.Features.Home.Scripts
{
    public sealed class HomeController : IDisposable
    {
        private readonly HomeView _view;
        private readonly ScreenNavigator _navigator;
        private readonly IHapticsService _haptics;

        public HomeController(HomeView view, ScreenNavigator navigator, IHapticsService haptics)
        {
            _view = view;
            _navigator = navigator;
            _haptics = haptics;

            _view.PlayClicked += HandlePlayClicked;
            _view.SettingsClicked += HandleSettingsClicked;
            _view.DeckClicked += HandleDeckClicked;
        }

        public void Dispose()
        {
            _view.PlayClicked -= HandlePlayClicked;
            _view.SettingsClicked -= HandleSettingsClicked;
            _view.DeckClicked -= HandleDeckClicked;
        }

        private void HandlePlayClicked()
        {
            _haptics.PlayLight();
            _navigator.NavigateTo<MatchmakingController>();
        }

        private void HandleSettingsClicked()
        {
            _haptics.PlayLight();
            _navigator.NavigateTo<SettingsController>();
        }

        private void HandleDeckClicked()
        {
            _haptics.PlayLight();
            _navigator.NavigateTo<DeckController>();
        }
    }
}
