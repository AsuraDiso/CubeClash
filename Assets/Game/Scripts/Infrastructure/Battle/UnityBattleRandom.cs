using Game.Scripts.Core.Battle.Simulation;
using Random = UnityEngine.Random;

namespace Game.Scripts.Infrastructure.Battle
{
    public sealed class UnityBattleRandom : IBattleRandom
    {
        public int Range(int minInclusive, int maxExclusive) => Random.Range(minInclusive, maxExclusive);
    }
}
