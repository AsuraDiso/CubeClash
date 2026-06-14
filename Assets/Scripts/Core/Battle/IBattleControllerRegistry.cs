using System;

namespace Core.Battle
{
    public interface IBattleControllerRegistry
    {
        IBattleAttackGateway Current { get; }

        event Action<IBattleAttackGateway> GatewayAvailable;

        void Register(IBattleAttackGateway gateway);

        void Unregister(IBattleAttackGateway gateway);
    }
}
