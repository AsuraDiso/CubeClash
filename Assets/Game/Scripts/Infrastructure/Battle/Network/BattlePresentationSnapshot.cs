using Fusion;
using Game.Scripts.Core.Battle.Simulation;

namespace Game.Scripts.Infrastructure.Battle.Network
{
    internal readonly struct BattlePresentationSnapshot
    {
        public bool MatchInitialized { get; }
        public bool IsGameOver { get; }
        public int WinnerNetworkId { get; }
        public int CurrentTurnPlayerId { get; }
        public int TurnDiceConsumedMask { get; }
        public int[] PlayerNetworkIds { get; }
        public int[] TurnDiceValues { get; }
        public NetworkPlayerProfile[] PlayerProfiles { get; }

        public BattlePresentationSnapshot(bool matchInitialized, bool isGameOver, int winnerNetworkId,
            int currentTurnPlayerId, int turnDiceConsumedMask, int[] playerNetworkIds, int[] turnDiceValues,
            NetworkPlayerProfile[] playerProfiles)
        {
            MatchInitialized = matchInitialized;
            IsGameOver = isGameOver;
            WinnerNetworkId = winnerNetworkId;
            CurrentTurnPlayerId = currentTurnPlayerId;
            TurnDiceConsumedMask = turnDiceConsumedMask;
            PlayerNetworkIds = playerNetworkIds;
            TurnDiceValues = turnDiceValues;
            PlayerProfiles = playerProfiles;
        }

        public bool Equals(BattlePresentationSnapshot other) =>
            MatchInitialized == other.MatchInitialized
            && IsGameOver == other.IsGameOver
            && WinnerNetworkId == other.WinnerNetworkId
            && CurrentTurnPlayerId == other.CurrentTurnPlayerId
            && TurnDiceConsumedMask == other.TurnDiceConsumedMask
            && ArraysEqual(PlayerNetworkIds, other.PlayerNetworkIds)
            && ArraysEqual(TurnDiceValues, other.TurnDiceValues)
            && ProfilesEqual(PlayerProfiles, other.PlayerProfiles);

        public bool TurnChangedFrom(BattlePresentationSnapshot previous) =>
            previous.MatchInitialized != MatchInitialized
            || previous.CurrentTurnPlayerId != CurrentTurnPlayerId
            || previous.TurnDiceConsumedMask != TurnDiceConsumedMask
            || !ArraysEqual(previous.TurnDiceValues, TurnDiceValues);

        public bool ProfilesChangedFrom(BattlePresentationSnapshot previous) =>
            previous.MatchInitialized != MatchInitialized
            || !ArraysEqual(previous.PlayerNetworkIds, PlayerNetworkIds)
            || !ProfilesEqual(previous.PlayerProfiles, PlayerProfiles);

        public bool GameOverChangedFrom(BattlePresentationSnapshot previous) =>
            previous.IsGameOver != IsGameOver
            || previous.WinnerNetworkId != WinnerNetworkId;

        private static bool ArraysEqual(int[] left, int[] right)
        {
            if (left.Length != right.Length)
                return false;

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                    return false;
            }

            return true;
        }

        private static bool ProfilesEqual(NetworkPlayerProfile[] left, NetworkPlayerProfile[] right)
        {
            if (left.Length != right.Length)
                return false;

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i].Hp != right[i].Hp
                    || left[i].PlayerId != right[i].PlayerId
                    || left[i].DisplayName != right[i].DisplayName)
                    return false;
            }

            return true;
        }
    }
}
