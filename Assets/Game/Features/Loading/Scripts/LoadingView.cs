using Game.Scripts.Bootstrap.UI.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Features.Loading.Scripts
{
    public sealed class LoadingView : ScreenView
    {
        [SerializeField] private Slider _slider;

        public void SetPercent(float percent) => _slider.value = percent;
    }
}
