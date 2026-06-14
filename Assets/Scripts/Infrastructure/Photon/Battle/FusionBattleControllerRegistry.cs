using System;
using Core.Battle;

namespace Infrastructure.Photon.Battle
{
    public sealed class FusionBattleControllerRegistry : IBattleControllerRegistry
    {
        public IBattleAttackGateway Current { get; private set; }

        public event Action<IBattleAttackGateway> GatewayAvailable;

        public void Register(IBattleAttackGateway gateway)
        {
            if (gateway == null)
            {
                return;
            }

            Current = gateway;
            GatewayAvailable?.Invoke(gateway);
        }

        public void Unregister(IBattleAttackGateway gateway)
        {
            if (Current == gateway)
            {
                Current = null;
            }
        }
    }
}
