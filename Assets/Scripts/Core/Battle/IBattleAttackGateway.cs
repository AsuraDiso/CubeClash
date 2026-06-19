using System;
using System.Collections.Generic;
using Cards;
using Core.Data;

namespace Core.Battle
{
    public interface IBattleAttackGateway
    {
        public bool IsMyTurn { get; }
        public int TurnDice1 { get; }
        public int TurnDice2 { get; }
        public PlayerProfile LocalProfile { get; }
        public PlayerProfile OpponentProfile { get; }
        public IReadOnlyList<PlacedCard> LocalDeck { get; }
        public IReadOnlyList<PlacedCard> OpponentDeck { get; }

        public event Action TurnChanged;
        public event Action ProfilesUpdated;
        public event Action DecksUpdated;
        public event Action<bool> GameOver;
        public event Action<int, string> AttackReceived;

        public void SendAttack();
    }
}
