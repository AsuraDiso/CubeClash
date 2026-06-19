using Core.Data;
using Fusion;

namespace Infrastructure.Photon.Battle
{
    public struct NetworkPlayerProfile : INetworkStruct
    {
        public NetworkString<_64> PlayerId;
        public NetworkString<_64> DisplayName;
        public int Hp;

        public static NetworkPlayerProfile From(PlayerProfile profile)
        {
            return new NetworkPlayerProfile
            {
                PlayerId = profile.PlayerId,
                DisplayName = profile.DisplayName,
                Hp = profile.Hp
            };
        }

        public PlayerProfile ToProfile()
        {
            return new PlayerProfile(PlayerId.ToString(), DisplayName.ToString(), Hp);
        }
    }
}

