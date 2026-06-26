using Cysharp.Threading.Tasks;
using Game.Features.Battle.Scripts;
using Game.Scripts.Core.Battle;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.EntryPoints
{
    public sealed class BattleSceneEntryPoint : IStartable
    {
        private readonly IObjectResolver _resolver;
        private readonly IBattleControllerRegistry _registry;

        public BattleSceneEntryPoint(IObjectResolver resolver, IBattleControllerRegistry registry)
        {
            _resolver = resolver;
            _registry = registry;
        }

        public void Start() => StartAsync().Forget(Debug.LogException);

        private async UniTask StartAsync()
        {
            await _registry.WaitForGatewayAsync();
            _resolver.Resolve<BattleController>();
        }
    }
}
