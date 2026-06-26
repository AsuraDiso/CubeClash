using Fusion;
using Game.Scripts.Core.Networking;

namespace Game.Scripts.Infrastructure.Networking
{
    public static class FusionGameModeMapper
    {
        public static GameMode ToFusion(NetworkGameMode gameMode)
        {
            return gameMode switch
            {
                NetworkGameMode.Single => GameMode.Single,
                NetworkGameMode.Shared => GameMode.Shared,
                NetworkGameMode.Host => GameMode.Host,
                NetworkGameMode.Client => GameMode.Client,
                NetworkGameMode.Server => GameMode.Server,
                NetworkGameMode.AutoHostOrClient => GameMode.AutoHostOrClient,
                _ => GameMode.AutoHostOrClient
            };
        }
    }
}
