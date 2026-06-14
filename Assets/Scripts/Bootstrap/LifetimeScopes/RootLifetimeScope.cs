using Bootstrap.EntryPoints;
using Bootstrap.Flow;
using Bootstrap.Installers;
using Bootstrap.Scenes;
using Bootstrap.UI;
using Cards;
using Core.Scenes;
using Infrastructure.Photon;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GamePrefabCatalog _prefabCatalog;
        [SerializeField] private CardCatalog _cardcatalog;
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
            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .As<ISceneLoaderService>();

            new DataInstaller().Install(builder);
            new MultiplayerInstaller().Install(builder);

            builder.UseEntryPoints(entryPoints =>
            {
                entryPoints.Add<GameBootstrapEntryPoint>();
                entryPoints.Add<MatchFlowCoordinator>();
            });
        }
    }
}
