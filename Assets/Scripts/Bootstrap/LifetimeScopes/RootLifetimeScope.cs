using Bootstrap.EntryPoints;
using Bootstrap.Flow;
using Bootstrap.Installers;
using Bootstrap.Scenes;
using Bootstrap.UI;
using Core.Scenes;
using Infrastructure.Photon;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GamePrefabCatalog prefabCatalog;
        [SerializeField] private UiPrefabCatalog uiPrefabCatalog;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            if (prefabCatalog == null)
            {
                Debug.LogError("[CubeClash] RootLifetimeScope is missing GamePrefabCatalog.");
            }
            else
            {
                builder.RegisterInstance(prefabCatalog);
            }

            if (uiPrefabCatalog == null)
            {
                Debug.LogError("[CubeClash] RootLifetimeScope is missing UiPrefabCatalog.");
            }
            else
            {
                builder.RegisterInstance(uiPrefabCatalog);
            }

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
