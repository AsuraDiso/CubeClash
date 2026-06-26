using Fusion;
using UnityEngine;

namespace Game.Scripts.Infrastructure.Networking
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkRunner))]
    public sealed class FusionNetworkRunnerSetup : MonoBehaviour
    {
        [SerializeField] private FusionSessionBridge _sessionBridge;
        [SerializeField] private NetworkSceneManagerDefault _sceneManager;
        [SerializeField] private NetworkObjectProviderDefault _objectProvider;

        private NetworkRunner _runner;

        public NetworkRunner Runner => _runner;

        public FusionSessionBridge SessionBridge => _sessionBridge;

        public INetworkSceneManager SceneManager => _sceneManager;

        public INetworkObjectProvider ObjectProvider => _objectProvider;

        private void Awake() => _runner = GetComponent<NetworkRunner>();
    }
}
