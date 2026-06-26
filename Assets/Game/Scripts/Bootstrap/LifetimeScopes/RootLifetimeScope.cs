using System;
using Game.Features.AppBootstrap.Scripts;
using Game.Shared.Scripts.UI.Theme;
using Game.Scripts.Core.Data.Cards;
using Game.Features.Network.Scripts;
using Game.Scripts.Bootstrap.Audio;
using Game.Scripts.Bootstrap.EntryPoints;
using Game.Scripts.Bootstrap.Scenes;
using Game.Scripts.Bootstrap.Settings;
using Game.Scripts.Core.Audio;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Firebase;
using Game.Scripts.Core.Matchmaking;
using Game.Scripts.Core.Networking;
using Game.Scripts.Core.Scenes;
using Game.Scripts.Core.Settings;
using Game.Scripts.Infrastructure.Battle;
using Game.Scripts.Infrastructure.Data;
using Game.Scripts.Infrastructure.Data.Firestore;
using Game.Scripts.Infrastructure.Firebase;
using Game.Scripts.Infrastructure.Networking;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.LifetimeScopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GamePrefabCatalog _prefabCatalog;
        [SerializeField] private AudioSystem _audioSystemPrefab;
        [SerializeField] private UiCameraRoot _uiCameraPrefab;
        [FormerlySerializedAs("_cardcatalog")]
        [SerializeField] private CardCatalog _cardCatalog;
        [SerializeField] private AudioCatalog _audioCatalog;
        [SerializeField] private BattleModeConfigAsset _defaultBattleMode;
        [SerializeField] private UiTheme _uiTheme;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeUiTheme();
            base.Awake();
        }

        private void InitializeUiTheme()
        {
            if (_uiTheme == null)
            {
#if UNITY_EDITOR
                var assets = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(UiTheme)}");
                if (assets.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[0]);
                    _uiTheme = UnityEditor.AssetDatabase.LoadAssetAtPath<UiTheme>(path);
                }
#endif
            }

            if (_uiTheme != null)
                UiThemeAccess.Set(_uiTheme);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterGameAssets(builder);
            RegisterBootstrapServices(builder);
            RegisterDataServices(builder);
            RegisterNetworkingServices(builder);
            RegisterEntryPoints(builder);
        }

        private void RegisterGameAssets(IContainerBuilder builder)
        {
            if (_prefabCatalog == null)
                throw MissingRef(nameof(_prefabCatalog));
            if (_cardCatalog == null)
                throw MissingRef(nameof(_cardCatalog));
            if (_audioCatalog == null)
                throw MissingRef(nameof(_audioCatalog));
            if (_audioSystemPrefab == null)
                throw MissingRef(nameof(_audioSystemPrefab));

            var battleMode = ResolveDefaultBattleMode();
            if (battleMode == null)
                throw MissingRef(nameof(_defaultBattleMode));

            builder.RegisterInstance(_prefabCatalog);
            builder.RegisterInstance(_cardCatalog);
            builder.RegisterInstance(_audioCatalog);
            builder.RegisterInstance(_audioSystemPrefab);
            builder.RegisterInstance(battleMode);

            if (_uiTheme != null)
                builder.RegisterInstance(_uiTheme);
        }

        private BattleModeConfigAsset ResolveDefaultBattleMode()
        {
            if (_defaultBattleMode != null)
                return _defaultBattleMode;

#if UNITY_EDITOR
            var assets = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(BattleModeConfigAsset)}");
            if (assets.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[0]);
                return UnityEditor.AssetDatabase.LoadAssetAtPath<BattleModeConfigAsset>(path);
            }
#endif
            return null;
        }

        private static InvalidOperationException MissingRef(string fieldName) =>
            new($"RootLifetimeScope.{fieldName} is not assigned. Open the Boot scene and wire it on the Bootstrap object.");

        private void RegisterBootstrapServices(IContainerBuilder builder)
        {
            builder.Register<AudioService>(Lifetime.Singleton)
                .As<IAudioService>();

            var uiCameraRoot = Instantiate(_uiCameraPrefab);
            builder.RegisterComponent(uiCameraRoot);

            builder.Register<GameSettingsService>(Lifetime.Singleton)
                .As<IGameSettingsService>();
            builder.Register<HapticsService>(Lifetime.Singleton)
                .As<IHapticsService>();
            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .As<ISceneLoaderService>();
        }

        private void RegisterDataServices(IContainerBuilder builder)
        {
            builder.Register<FirebaseAppService>(Lifetime.Singleton)
                .As<IFirebaseAppService>();
            builder.Register<LocalUserIdProvider>(Lifetime.Singleton)
                .As<IUserIdProvider>();
            builder.Register<FirestoreUserDocumentStore>(Lifetime.Singleton);
            builder.Register<FirestorePlayerRepository>(Lifetime.Singleton)
                .As<IPlayerRepository>();
            builder.Register<FirestoreDeckRepository>(Lifetime.Singleton)
                .As<IDeckRepository>();
            builder.Register<DeckService>(Lifetime.Singleton)
                .As<IDeckService>();
        }

        private void RegisterNetworkingServices(IContainerBuilder builder)
        {
            builder.Register<FusionBattleControllerRegistry>(Lifetime.Singleton)
                .As<IBattleControllerRegistry>();
            builder.Register<FusionNetworkSessionService>(Lifetime.Singleton)
                .As<INetworkSession>()
                .AsSelf();
            builder.Register<FusionMatchmakingService>(Lifetime.Singleton)
                .As<IMatchmakingService>();
        }

        private static void RegisterEntryPoints(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameBootstrapEntryPoint>();
        }
    }
}
