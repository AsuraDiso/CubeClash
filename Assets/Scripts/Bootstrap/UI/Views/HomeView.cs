using System;
using UnityEngine;

namespace Bootstrap.UI.Views
{
    public sealed class HomeView : MonoBehaviour
    {
        public event Action PlayClicked;
        public event Action EventsClicked;
        public event Action DeckClicked;
        public event Action ShopClicked;
        public event Action SettingsClicked;

        public void OnPlayButtonClicked() => PlayClicked?.Invoke();

        public void OnEventsButtonClicked() => EventsClicked?.Invoke();

        public void OnDeckButtonClicked() => DeckClicked?.Invoke();

        public void OnShopButtonClicked() => ShopClicked?.Invoke();

        public void OnSettingsButtonClicked() => SettingsClicked?.Invoke();
    }
}
