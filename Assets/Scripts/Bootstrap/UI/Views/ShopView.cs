using System;
using UnityEngine;

namespace Bootstrap.UI.Views
{
    public sealed class ShopView : MonoBehaviour, INavigableView
    {
        public event Action BackClicked;

        public void OnBackButtonClicked() => BackClicked?.Invoke();
    }
}
