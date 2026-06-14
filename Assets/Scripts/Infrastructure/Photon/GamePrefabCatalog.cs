using Fusion;
using UnityEngine;

namespace Infrastructure.Photon
{
    [CreateAssetMenu(menuName = "CubeClash/Game Prefab Catalog", fileName = "GamePrefabCatalog")]
    public sealed class GamePrefabCatalog : ScriptableObject
    {
        [SerializeField] private NetworkObject _battleSessionPrefab;
        [SerializeField] private NetworkRunner _networkRunnerPrefab;

        public NetworkRunner NetworkRunnerPrefab => _networkRunnerPrefab;
        public NetworkObject BattleSessionPrefab => _battleSessionPrefab;
    }
}
