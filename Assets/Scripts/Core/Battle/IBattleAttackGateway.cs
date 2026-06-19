using System;
using Core.Data;

namespace Core.Battle
{
    public interface IBattleAttackGateway
    {
        bool IsMyTurn { get; }
        int TurnDice1 { get; }
        int TurnDice2 { get; }
        PlayerProfile LocalProfile { get; }
        PlayerProfile OpponentProfile { get; }

        event Action TurnChanged;
        event Action ProfilesUpdated;
        event Action<bool> GameOver;
        event Action<int, string> AttackReceived;

        void SendAttack();
    }
}
