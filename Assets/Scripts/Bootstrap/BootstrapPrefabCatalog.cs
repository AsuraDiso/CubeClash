using Bootstrap.Audio;
using Bootstrap.UI;
using UnityEngine;

namespace Bootstrap
{
    [CreateAssetMenu(menuName = "CubeClash/Bootstrap Prefab Catalog", fileName = "BootstrapPrefabCatalog")]
    public sealed class BootstrapPrefabCatalog : ScriptableObject
    {
        [SerializeField] private AudioSystem _audioSystemPrefab;
        [SerializeField] private UiCameraRoot _uiCameraPrefab;

        public AudioSystem AudioSystemPrefab => _audioSystemPrefab;
        public UiCameraRoot UiCameraPrefab => _uiCameraPrefab;
    }
}
