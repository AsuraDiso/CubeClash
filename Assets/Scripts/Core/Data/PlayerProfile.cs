namespace Core.Data
{
    public sealed class PlayerProfile
    {
        public string PlayerId { get; }
        public string DisplayName { get; }

        public PlayerProfile(string playerId, string displayName)
        {
            PlayerId = playerId;
            DisplayName = displayName;
        }
    }
}
