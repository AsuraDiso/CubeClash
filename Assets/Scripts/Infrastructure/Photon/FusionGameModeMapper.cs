using Core.Networking;
using Fusion;

namespace Infrastructure.Photon
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
