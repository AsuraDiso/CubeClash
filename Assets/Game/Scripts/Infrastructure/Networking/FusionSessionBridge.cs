using Game.Scripts.Core.Battle;
using Game.Scripts.Infrastructure.Battle.Session;
using UnityEngine;
namespace Game.Scripts.Infrastructure.Networking
{
    public sealed class FusionSessionBridge : MonoBehaviour
    {
        private IBattleControllerRegistry _battleControllerRegistry;

        public BattleSessionPayload Payload { get; private set; }

        public void Initialize(IBattleControllerRegistry battleControllerRegistry,
            BattleSessionPayload payload)
        {
            _battleControllerRegistry = battleControllerRegistry;
            Payload = payload;
        }

        public IBattleControllerRegistry BattleControllerRegistry => _battleControllerRegistry;
    }
}
