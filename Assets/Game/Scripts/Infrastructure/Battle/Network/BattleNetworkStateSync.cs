using Fusion;
using Game.Scripts.Core.Battle.Simulation;

namespace Game.Scripts.Infrastructure.Battle.Network
{
    internal static class BattleNetworkStateSync
    {
        public const int MaxDiceCount = BattleState.MaxDiceCount;

        public static int BuildConsumedMask(BattleState state)
        {
            var consumedMask = 0;
            for (var i = 0; i < state.TurnDiceConsumed.Length; i++)
            {
                if (state.TurnDiceConsumed[i])
                    consumedMask |= 1 << i;
            }

            return consumedMask;
        }

        public static int GetTurnDiceValue(NetworkArray<int> turnDiceValues, int dieIndex) =>
            dieIndex >= 0 && dieIndex < MaxDiceCount ? turnDiceValues.Get(dieIndex) : 0;
    }
}
