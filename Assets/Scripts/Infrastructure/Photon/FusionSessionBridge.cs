using Core.Battle;
using UnityEngine;

namespace Infrastructure.Photon
{
    public sealed class FusionSessionBridge : MonoBehaviour
    {
        private IBattleControllerRegistry _battleControllerRegistry;

        public void Initialize(IBattleControllerRegistry battleControllerRegistry)
        {
            _battleControllerRegistry = battleControllerRegistry;
        }

        public IBattleControllerRegistry BattleControllerRegistry => _battleControllerRegistry;
    }
}
