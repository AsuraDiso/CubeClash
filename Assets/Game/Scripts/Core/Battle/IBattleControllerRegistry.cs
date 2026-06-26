using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Core.Battle
{
    public interface IBattleControllerRegistry
    {
        IBattleGateway Current { get; }

        void Register(IBattleGateway gateway);

        void Unregister(IBattleGateway gateway);

        UniTask<IBattleGateway> WaitForGatewayAsync(CancellationToken cancellationToken = default);
    }
}
