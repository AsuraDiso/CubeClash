using Core.Battle;
using Core.Matchmaking;
using Core.Networking;
using Core.Scenes;
using Infrastructure.Photon;
using Infrastructure.Photon.Battle;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.Installers
{
    public sealed class MultiplayerInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<FusionNetworkRunnerFactory>(Lifetime.Singleton);
            builder.Register<FusionBattleControllerRegistry>(Lifetime.Singleton)
                .As<IBattleControllerRegistry>();
            builder.Register<FusionNetworkSessionService>(Lifetime.Singleton)
                .As<INetworkSession>()
                .As<IFusionRunnerAccessor>();
            builder.Register<FusionMatchmakingService>(Lifetime.Singleton)
                .As<IMatchmakingService>();
            builder.Register<FusionBattleSceneLoader>(Lifetime.Singleton)
                .As<IBattleSceneLoader>();
            builder.Register<FusionBattleSessionSpawner>(Lifetime.Singleton)
                .As<IBattleSessionSpawner>();
        }
    }
}
