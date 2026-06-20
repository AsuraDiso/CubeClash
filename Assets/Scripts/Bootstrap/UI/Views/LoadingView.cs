using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private Slider _loadingSlider;

        public void SetPercent(float percent) => _loadingSlider.value = percent;
    }
}
