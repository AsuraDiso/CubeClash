using Game.Features.Loading.Scripts;
using Game.Scripts.Bootstrap.EntryPoints;
using Game.Scripts.Core.Audio;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.LifetimeScopes
{
    public sealed class LoadingLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingView _loadingViewPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_loadingViewPrefab == null)
                throw new System.InvalidOperationException($"{nameof(LoadingLifetimeScope)}: assign the view prefab.");

            builder.RegisterComponentInNewPrefab(_loadingViewPrefab, Lifetime.Singleton);
            builder.RegisterInstance(MusicId.Loading);
            builder.RegisterEntryPoint<SceneMusicEntryPoint>();
            builder.RegisterEntryPoint<LoadingSceneEntryPoint>();
        }
    }
}
