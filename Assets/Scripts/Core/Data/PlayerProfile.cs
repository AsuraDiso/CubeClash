using Core.Battle;

namespace Core.Data
{
    public sealed class PlayerProfile
    {
        public const string DefaultDisplayName = "Cube Fighter";

        public string PlayerId { get; }
        public string DisplayName { get; }
        public int Hp { get; }

        public PlayerProfile(string playerId, string displayName, int hp)
        {
            PlayerId = playerId ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? DefaultDisplayName : displayName;
            Hp = hp;
        }

        public static PlayerProfile CreateBattleDefault(string playerId = "", string displayName = null) =>
            new(playerId, displayName, BattleConstants.MaxHp);

        public static PlayerProfile Unknown => new(string.Empty, DefaultDisplayName, 0);

        public PlayerProfile WithHp(int hp) => new(PlayerId, DisplayName, hp);
    }
}
