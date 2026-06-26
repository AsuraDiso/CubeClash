using Game.Features.Battle.Scripts;
using Game.Features.Battle.Scripts.Presentation;
using Game.Scripts.Bootstrap.EntryPoints;
using Game.Scripts.Core.Audio;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.LifetimeScopes
{
    public sealed class BattleLifetimeScope : LifetimeScope
    {
        [SerializeField] private BattleView _battleViewPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_battleViewPrefab == null)
                throw new System.InvalidOperationException($"{nameof(BattleLifetimeScope)}: assign the view prefab.");

            builder.RegisterComponentInNewPrefab(_battleViewPrefab, Lifetime.Singleton);
            builder.RegisterInstance(MusicId.Battle);
            builder.RegisterEntryPoint<SceneMusicEntryPoint>();
            builder.Register<CardBattlePresentationHandler>(Lifetime.Scoped).As<IBattleActionPresentationHandler>();
            builder.Register<BattlePresentationRouter>(Lifetime.Scoped);
            builder.Register<BattleController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<BattleSceneEntryPoint>();
        }
    }
}
