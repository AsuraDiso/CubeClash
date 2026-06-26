using System;
using Game.Scripts.Core.Battle.Simulation;

namespace Game.Scripts.Core.Battle
{
    public sealed class BattleModeConfig
    {
        public int ModeId { get; }
        public int MaxPlayers { get; }
        public int MaxHp { get; }
        public int DiceCount { get; }
        public int DiceMin { get; }
        public int DiceMax { get; }

        public BattleModeConfig(int modeId, int maxHp, int diceCount, int diceMin, int diceMax,
            int maxPlayers = BattleState.MaxPlayers)
        {
            ModeId = modeId;
            MaxPlayers = Math.Max(1, maxPlayers);
            MaxHp = Math.Max(1, maxHp);
            DiceCount = Math.Clamp(diceCount, 1, BattleState.MaxDiceCount);
            DiceMin = Math.Max(1, diceMin);
            DiceMax = Math.Max(DiceMin, diceMax);
        }
    }
}
