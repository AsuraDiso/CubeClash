using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Game.Features.AppBootstrap.Scripts
{
    public sealed class UiCameraRoot : MonoBehaviour
    {
        [field: SerializeField] public Camera Camera { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            BindToMainCamera();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => BindToMainCamera();

        private void BindToMainCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
            if (!mainCameraData.cameraStack.Contains(Camera))
                mainCameraData.cameraStack.Add(Camera);
        }
    }
}
