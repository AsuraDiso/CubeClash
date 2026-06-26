using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Core.Battle;

namespace Game.Scripts.Infrastructure.Battle
{
    public sealed class FusionBattleControllerRegistry : IBattleControllerRegistry
    {
        private const int GatewayWaitTimeoutSeconds = 30;

        public IBattleGateway Current { get; private set; }

        public void Register(IBattleGateway gateway)
        {
            Current = gateway;
        }

        public void Unregister(IBattleGateway gateway)
        {
            if (Current == gateway)
                Current = null;
        }

        public async UniTask<IBattleGateway> WaitForGatewayAsync(CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(GatewayWaitTimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await UniTask.WaitUntil(() => Current != null, cancellationToken: linkedCts.Token);
            return Current;
        }
    }
}
