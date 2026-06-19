using Core.Battle;
using Core.Data;
using UnityEngine;

namespace Infrastructure.Photon
{
    public sealed class FusionSessionBridge : MonoBehaviour
    {
        private IBattleControllerRegistry _battleControllerRegistry;

        public PlayerProfile LocalProfile { get; set; }

        public void Initialize(IBattleControllerRegistry battleControllerRegistry)
        {
            _battleControllerRegistry = battleControllerRegistry;
        }

        public IBattleControllerRegistry BattleControllerRegistry => _battleControllerRegistry;
    }
}
