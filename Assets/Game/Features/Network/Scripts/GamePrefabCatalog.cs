using Fusion;
using UnityEngine;

namespace Game.Features.Network.Scripts
{
    [CreateAssetMenu(menuName = "CubeClash/Game Prefab Catalog", fileName = "GamePrefabCatalog")]
    public sealed class GamePrefabCatalog : ScriptableObject
    {
        [field: SerializeField] public NetworkObject BattleSessionPrefab { get; private set; }
        [field: SerializeField] public NetworkRunner NetworkRunnerPrefab { get; private set; }
    }
}
