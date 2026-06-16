using Bootstrap.EntryPoints;
using Bootstrap.Flow;
using Bootstrap.Installers;
using Bootstrap.Scenes;
using Bootstrap.UI;
using Cards;
using Core.Scenes;
using Infrastructure.Photon;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GamePrefabCatalog _prefabCatalog;
        [FormerlySerializedAs("_cardcatalog")]
        [SerializeField] private CardCatalog _cardCatalog;
        [SerializeField] private UiPrefabCatalog _uiPrefabCatalog;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_prefabCatalog);
            builder.RegisterInstance(_uiPrefabCatalog);
            builder.RegisterInstance(_cardCatalog);
            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .As<ISceneLoaderService>();

            builder.InstallData();
            builder.InstallMultiplayer();

            builder.RegisterEntryPoint<GameBootstrapEntryPoint>();
            builder.RegisterEntryPoint<MatchFlowCoordinator>();
        }
    }
}
