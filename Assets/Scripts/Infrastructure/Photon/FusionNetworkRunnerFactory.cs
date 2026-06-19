using Fusion;
using UnityEngine;

namespace Infrastructure.Photon
{
    public sealed class FusionNetworkRunnerFactory
    {
        private readonly GamePrefabCatalog _prefabCatalog;

        public FusionNetworkRunnerFactory(GamePrefabCatalog prefabCatalog)
        {
            _prefabCatalog = prefabCatalog;
        }

        public NetworkRunner CreateRunner()
        {
            var runner = Object.Instantiate(_prefabCatalog.NetworkRunnerPrefab);
            Object.DontDestroyOnLoad(runner.gameObject);
            return runner;
        }
    }
}
