using System;
using Game.Scripts.Bootstrap.UI.Views;

namespace Game.Features.Home.Scripts
{
    public sealed class HomeView : ScreenView
    {
        public event Action PlayClicked;
        public event Action DeckClicked;
        public event Action SettingsClicked;

        public void OnPlayButtonClicked() => PlayClicked?.Invoke();

        public void OnDeckButtonClicked() => DeckClicked?.Invoke();

        public void OnSettingsButtonClicked() => SettingsClicked?.Invoke();
    }
}
