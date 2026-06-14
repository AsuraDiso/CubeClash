using Core.Battle;
using UnityEngine;

namespace Infrastructure.Photon.Battle
{
    public sealed class FusionBattleSessionSpawner : IBattleSessionSpawner
    {
        private readonly IFusionRunnerAccessor _runnerAccessor;
        private readonly IBattleControllerRegistry _controllerRegistry;
        private readonly GamePrefabCatalog _prefabCatalog;

        public FusionBattleSessionSpawner(
            IFusionRunnerAccessor runnerAccessor,
            IBattleControllerRegistry controllerRegistry,
            GamePrefabCatalog prefabCatalog)
        {
            _runnerAccessor = runnerAccessor;
            _controllerRegistry = controllerRegistry;
            _prefabCatalog = prefabCatalog;
        }

        public void Start()
        {
            var runner = _runnerAccessor.ActiveRunner;
            if (runner == null || !runner.IsRunning)
            {
                return;
            }

            if (_controllerRegistry.Current != null)
            {
                return;
            }

            if (!runner.IsServer)
            {
                return;
            }

            var prefab = _prefabCatalog != null ? _prefabCatalog.BattleSessionPrefab : null;
            if (prefab == null)
            {
                Debug.LogError(
                    "[CubeClash] Battle session prefab is not assigned in GamePrefabCatalog.");
                return;
            }

            runner.Spawn(prefab);
        }
    }
}
