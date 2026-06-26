using System;
using Game.Shared.Scripts.UI;

namespace Game.Scripts.Bootstrap.UI.Views
{
    public class NavigableScreenView : ScreenView, INavigableView
    {
        public event Action BackClicked;

        public void OnBackButtonClicked() => BackClicked?.Invoke();
    }
}
