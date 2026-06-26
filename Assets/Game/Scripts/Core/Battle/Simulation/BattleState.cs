using System.Collections.Generic;
using Game.Scripts.Core.Data.Cards;
using Game.Scripts.Core.Data;

namespace Game.Scripts.Core.Battle.Simulation
{
    public sealed class BattleState
    {
        public const int MaxPlayers = 2;
        public const int MaxDiceCount = 8;
        public const int UnassignedNetworkId = -1;

        public int BattleModeId { get; set; }
        public int CurrentTurnPlayerId { get; set; }
        public int[] PlayerNetworkIds { get; }
        public PlayerProfile[] Profiles { get; }
        public int[] TurnDiceValues { get; }
        public bool[] TurnDiceConsumed { get; }
        public List<PlacedCard>[] PlayerDecks { get; }
        public bool IsInitialized { get; set; }
        public bool IsGameOver { get; set; }
        public int WinnerNetworkId { get; set; }

        public BattleState(BattleModeConfig mode)
        {
            PlayerNetworkIds = new int[MaxPlayers];
            for (var i = 0; i < MaxPlayers; i++)
                PlayerNetworkIds[i] = UnassignedNetworkId;

            Profiles = new PlayerProfile[MaxPlayers];
            TurnDiceValues = new int[mode.DiceCount];
            TurnDiceConsumed = new bool[mode.DiceCount];
            PlayerDecks = new List<PlacedCard>[MaxPlayers];

            for (var i = 0; i < MaxPlayers; i++)
                PlayerDecks[i] = new List<PlacedCard>();
        }

        public int FindSlotByNetworkId(int networkPlayerId)
        {
            for (var i = 0; i < MaxPlayers; i++)
            {
                if (PlayerNetworkIds[i] == networkPlayerId)
                    return i;
            }

            return -1;
        }

        public static int GetOpponentSlot(int slot) => slot == 0 ? 1 : 0;

        public void SetProfile(int slot, PlayerProfile profile) => Profiles[slot] = profile;

        public void SetPlayerDeck(int slot, IReadOnlyList<PlacedCard> deck)
        {
            PlayerDecks[slot].Clear();
            if (deck == null)
                return;

            PlayerDecks[slot].AddRange(deck);
        }

        public void ResetTurnDiceConsumption()
        {
            for (var i = 0; i < TurnDiceConsumed.Length; i++)
                TurnDiceConsumed[i] = false;
        }

        public bool AreAllTurnDiceConsumed()
        {
            for (var i = 0; i < TurnDiceConsumed.Length; i++)
            {
                if (!TurnDiceConsumed[i])
                    return false;
            }

            return TurnDiceConsumed.Length > 0;
        }
    }
}
