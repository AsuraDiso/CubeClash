using System.Text;using Core.Data;

namespace Core.Networking
{
    public static class ProfileConnectionToken
    {
        public static byte[] Encode(string playerId, string displayName) =>
            Encoding.UTF8.GetBytes($"{playerId ?? string.Empty}\n{displayName ?? string.Empty}");

        public static PlayerProfile TryDecode(byte[] token)
        {
            if (token == null || token.Length == 0)
            {
                return null;
            }

            var payload = Encoding.UTF8.GetString(token);
            var separatorIndex = payload.IndexOf('\n');
            if (separatorIndex < 0)
            {
                return string.IsNullOrWhiteSpace(payload)
                    ? null
                    : PlayerProfile.CreateBattleDefault(displayName: payload);
            }

            return PlayerProfile.CreateBattleDefault(
                payload[..separatorIndex],
                payload[(separatorIndex + 1)..]);
        }
    }
}
