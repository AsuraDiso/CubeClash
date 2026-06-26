using UnityEngine;
using UnityEngine.UI;

namespace Game.Shared.Scripts.UI.Components
{
    [RequireComponent(typeof(Slider))]
    public sealed class UiSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;

        public Slider Slider => _slider != null ? _slider : _slider = GetComponent<Slider>();

        public float Value
        {
            get => Slider.value;
            set => Slider.SetValueWithoutNotify(Mathf.Clamp01(value));
        }

        private void Reset() => _slider = GetComponent<Slider>();
    }
}
