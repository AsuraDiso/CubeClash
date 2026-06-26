using Fusion;
using Game.Scripts.Core.Data;

namespace Game.Scripts.Infrastructure.Battle.Network
{
    public struct NetworkPlayerProfile : INetworkStruct
    {
        public NetworkString<_64> PlayerId;
        public NetworkString<_64> DisplayName;
        public int Hp;

        public NetworkPlayerProfile(PlayerProfile profile)
        {
            PlayerId = profile.PlayerId;
            DisplayName = profile.DisplayName;
            Hp = profile.Hp;
        }

        public PlayerProfile ToProfile() => new(PlayerId.ToString(), DisplayName.ToString(), Hp);
    }
}
