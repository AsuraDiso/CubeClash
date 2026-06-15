using System;
using UnityEngine;

namespace Bootstrap.UI.Views
{
    public class HomeView : MonoBehaviour
    {
        public event Action PlayClicked;

        public void OnPlayButtonClicked() => PlayClicked?.Invoke();
    }
}
