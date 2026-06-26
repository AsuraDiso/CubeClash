using Fusion;
using Game.Scripts.Core.Battle.Simulation;
using Game.Scripts.Core.Data;

namespace Game.Scripts.Infrastructure.Battle.Network
{
    internal static class BattlePlayerSlots
    {
        public static int GetOpponentNetworkId(NetworkArray<int> playerNetworkIds, int localNetworkId)
        {
            for (var i = 0; i < BattleState.MaxPlayers; i++)
            {
                if (playerNetworkIds.Get(i) != localNetworkId)
                    continue;

                return playerNetworkIds.Get(BattleState.GetOpponentSlot(i));
            }

            return BattleState.UnassignedNetworkId;
        }

        public static PlayerProfile GetProfile(NetworkArray<int> playerNetworkIds,
            NetworkArray<NetworkPlayerProfile> playerProfiles, int networkPlayerId)
        {
            if (networkPlayerId == BattleState.UnassignedNetworkId)
                return PlayerProfile.Unknown;

            for (var i = 0; i < BattleState.MaxPlayers; i++)
            {
                if (playerNetworkIds.Get(i) != networkPlayerId)
                    continue;

                return playerProfiles.Get(i).ToProfile();
            }

            return PlayerProfile.Unknown;
        }
    }
}
