using System;
using UnityEngine;

namespace Bootstrap.UI.Views
{
    public sealed class MainMenuView : MonoBehaviour
    {
        public event Action PlayClicked;

        public void OnPlayButtonClick() => PlayClicked?.Invoke();
    }
}
