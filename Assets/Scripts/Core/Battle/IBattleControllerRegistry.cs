using System;

namespace Core.Battle
{
    public interface IBattleControllerRegistry
    {
        public IBattleAttackGateway Current { get; }

        public event Action<IBattleAttackGateway> GatewayAvailable;

        public void Register(IBattleAttackGateway gateway);

        public void Unregister(IBattleAttackGateway gateway);
    }
}
