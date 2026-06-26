using Game.Features.AppBootstrap.Scripts;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Bootstrap.UI.Views
{
    public abstract class ScreenView : MonoBehaviour
    {
        private const int UiLayer = 5;

        [SerializeField] private Canvas _canvas;

        [Inject]
        public void Construct(UiCameraRoot uiCameraRoot) => BindUiCamera(uiCameraRoot);

        public void BindUiCamera(UiCameraRoot uiCameraRoot)
        {
            _canvas.gameObject.layer = UiLayer;
            _canvas.worldCamera = uiCameraRoot.Camera;
        }
    }
}
