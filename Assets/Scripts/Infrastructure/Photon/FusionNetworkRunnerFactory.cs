using Core.Battle;
using Fusion;
using UnityEngine;

namespace Infrastructure.Photon
{
    public sealed class FusionNetworkRunnerFactory
    {
        private readonly IBattleControllerRegistry _battleControllerRegistry;
        private readonly GamePrefabCatalog _prefabCatalog;

        public FusionNetworkRunnerFactory(IBattleControllerRegistry battleControllerRegistry, GamePrefabCatalog prefabCatalog)
        {
            _battleControllerRegistry = battleControllerRegistry;
            _prefabCatalog = prefabCatalog;
        }

        public NetworkRunner CreateRunner()
        {
            var runner = Object.Instantiate(_prefabCatalog.NetworkRunnerPrefab);
            Object.DontDestroyOnLoad(runner.gameObject);
            var bridge = runner.GetComponent<FusionSessionBridge>();
            bridge.Initialize(_battleControllerRegistry);
            return runner;
        }
    }
}
