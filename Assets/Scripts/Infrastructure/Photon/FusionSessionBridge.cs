using Core.Battle;
using UnityEngine;

namespace Infrastructure.Photon
{
    public sealed class FusionSessionBridge : MonoBehaviour
    {
        private IBattleControllerRegistry _battleControllerRegistry;

        public BattleSessionPayload Payload { get; private set; }

        public void Initialize(IBattleControllerRegistry battleControllerRegistry, BattleSessionPayload payload)
        {
            _battleControllerRegistry = battleControllerRegistry;
            Payload = payload;
        }

        public IBattleControllerRegistry BattleControllerRegistry => _battleControllerRegistry;
    }
}
