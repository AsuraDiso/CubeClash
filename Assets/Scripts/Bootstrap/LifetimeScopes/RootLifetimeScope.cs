using Bootstrap.Audio;
using Bootstrap.Settings;
using Bootstrap;
using Core.Settings;
using Bootstrap.EntryPoints;
using Bootstrap.Flow;
using Bootstrap.Scenes;
using Bootstrap.UI;
using Cards;
using Core.Audio;
using Core.Battle;
using Core.Data;
using Core.Firebase;
using Core.Matchmaking;
using Core.Networking;
using Core.Scenes;
using Infrastructure.Data;
using Infrastructure.Data.Firestore;
using Infrastructure.Firebase;
using Infrastructure.Photon;
using Infrastructure.Photon.Battle;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GamePrefabCatalog _prefabCatalog;
        [SerializeField] private BootstrapPrefabCatalog _bootstrapPrefabCatalog;
        [FormerlySerializedAs("_cardcatalog")]
        [SerializeField] private CardCatalog _cardCatalog;
        [SerializeField] private UiPrefabCatalog _uiPrefabCatalog;
        [SerializeField] private AudioCatalog _audioCatalog;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_prefabCatalog);
            builder.RegisterInstance(_bootstrapPrefabCatalog);
            builder.RegisterInstance(_uiPrefabCatalog);
            builder.RegisterInstance(_cardCatalog);
            builder.RegisterInstance(_audioCatalog).As<IAudioCatalog>();
            builder.Register<AudioService>(Lifetime.Singleton)
                .As<IAudioService>();

            var uiCameraRoot = Instantiate(_bootstrapPrefabCatalog.UiCameraPrefab);
            builder.RegisterComponent(uiCameraRoot);

            builder.Register<GameSettingsService>(Lifetime.Singleton)
                .As<IGameSettingsService>();
            builder.Register<HapticsService>(Lifetime.Singleton)
                .As<IHapticsService>();
            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .As<ISceneLoaderService>();

            builder.Register<FirebaseAppService>(Lifetime.Singleton)
                .As<IFirebaseAppService>();
            builder.Register<LocalUserIdProvider>(Lifetime.Singleton)
                .As<IUserIdProvider>();
            builder.Register<FirestorePlayerRepository>(Lifetime.Singleton)
                .As<IPlayerRepository>();
            builder.Register<FirestoreDeckRepository>(Lifetime.Singleton)
                .As<IDeckRepository>();
            builder.Register<DeckService>(Lifetime.Singleton)
                .As<IDeckService>();
            builder.Register<DeckBattleLoadoutProvider>(Lifetime.Singleton)
                .As<IBattleLoadoutProvider>();

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

            builder.RegisterEntryPoint<GameBootstrapEntryPoint>();
            builder.RegisterEntryPoint<MatchFlowCoordinator>();
        }
    }
}
