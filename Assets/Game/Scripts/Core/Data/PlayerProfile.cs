using System;

namespace Game.Scripts.Core.Data
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
            DisplayName = NormalizeDisplayName(displayName);
            Hp = hp;
        }

        public static string NormalizeDisplayName(string displayName)
        {
            var trimmed = displayName?.Trim();
            return string.IsNullOrEmpty(trimmed) ? DefaultDisplayName : trimmed;
        }

        public static PlayerProfile CreateBattleDefault(string playerId = "", string displayName = null, int maxHp = 3) =>
            new(playerId, displayName, maxHp);

        public static PlayerProfile Unknown => new(string.Empty, DefaultDisplayName, 0);

        public PlayerProfile WithHp(int hp, int maxHp = int.MaxValue) =>
            new(PlayerId, DisplayName, Math.Clamp(hp, 0, maxHp));
    }
}
