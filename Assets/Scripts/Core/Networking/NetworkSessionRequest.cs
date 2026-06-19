using Core.Data;
using Core.Scenes;

namespace Core.Networking
{
    public sealed class NetworkSessionRequest
    {
        public NetworkGameMode GameMode { get; }
        public string SessionName { get; }
        public GameSceneId? InitialScene { get; }
        public string PlayerId { get; }
        public string DisplayName { get; }

        public NetworkSessionRequest(
            NetworkGameMode gameMode,
            string sessionName,
            GameSceneId? initialScene = null,
            string playerId = null,
            string displayName = null)
        {
            GameMode = gameMode;
            SessionName = sessionName;
            InitialScene = initialScene;
            PlayerId = playerId ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? PlayerProfile.DefaultDisplayName
                : displayName;
        }
    }
}
