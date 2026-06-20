using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Bootstrap.UI
{
    public sealed class UiCameraRoot : MonoBehaviour
    {
        private const float DefaultPlaneDistance = 100f;
        private const int UiLayer = 5;

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

        public void ConfigureCanvasesInHierarchy(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            SetLayerRecursively(root, UiLayer);

            var canvases = root.GetComponentsInChildren<Canvas>(includeInactive: true);
            foreach (var canvas in canvases)
            {
                ConfigureCanvas(canvas);
            }

            Canvas.ForceUpdateCanvases();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => BindToMainCamera();

        private void BindToMainCamera()
        {
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var uiCameraData = Camera.GetUniversalAdditionalCameraData();
            uiCameraData.renderType = CameraRenderType.Overlay;

            var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
            if (!mainCameraData.cameraStack.Contains(Camera))
            {
                mainCameraData.cameraStack.Add(Camera);
            }
        }

        private void ConfigureCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera;
            canvas.planeDistance = DefaultPlaneDistance;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;

            var transform = root.transform;
            for (var i = 0; i < transform.childCount; i++)
            {
                SetLayerRecursively(transform.GetChild(i).gameObject, layer);
            }
        }
    }
}
