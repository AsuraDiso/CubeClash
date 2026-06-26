using Game.Scripts.Core.Data;
using Game.Scripts.Core.Scenes;

namespace Game.Scripts.Core.Networking
{
    public sealed class NetworkSessionRequest
    {
        public NetworkGameMode GameMode { get; }
        public string SessionName { get; }
        public int? MaxPlayers { get; }
        public GameSceneId? InitialScene { get; }
        public string PlayerId { get; }
        public string DisplayName { get; }
        public object SessionPayload { get; }

        public NetworkSessionRequest(NetworkGameMode gameMode, string sessionName = null, GameSceneId? initialScene = null,
            string playerId = null, string displayName = null, int? maxPlayers = null, object sessionPayload = null)
        {
            GameMode = gameMode;
            SessionName = sessionName;
            MaxPlayers = maxPlayers;
            InitialScene = initialScene;
            PlayerId = playerId ?? string.Empty;
            DisplayName = PlayerProfile.NormalizeDisplayName(displayName);
            SessionPayload = sessionPayload;
        }
    }
}
