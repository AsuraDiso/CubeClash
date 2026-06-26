namespace Game.Scripts.Core.Battle.Actions
{
    public class BattleActionResult
    {
        public static readonly BattleActionResult Failed = new(false);

        public bool Success { get; }
        public bool TurnEnded { get; protected set; }
        public bool GameEnded { get; protected set; }
        public int ActorNetworkId { get; protected set; }
        public string ActorDisplayName { get; protected set; }
        public int WinnerNetworkId { get; protected set; }

        protected BattleActionResult(bool success)
        {
            Success = success;
        }

        protected BattleActionResult(bool success, int actorNetworkId, string actorDisplayName)
        {
            Success = success;
            ActorNetworkId = actorNetworkId;
            ActorDisplayName = actorDisplayName;
        }

        public BattleActionResult WithTurnEnded()
        {
            TurnEnded = true;
            return this;
        }

        public BattleActionResult WithGameEnd(int winnerNetworkId)
        {
            GameEnded = true;
            WinnerNetworkId = winnerNetworkId;
            return this;
        }
    }
}
