using System;

namespace Core.Battle
{
    public interface IBattleAttackGateway
    {
        bool IsMyTurn { get; }
        int LocalPlayerHp { get; }
        int OpponentHp { get; }

        event Action TurnChanged;
        event Action<int, int> HealthUpdated;
        event Action<bool> GameOver;
        event Action<int, string> AttackReceived;

        void SendAttack();
    }
}
