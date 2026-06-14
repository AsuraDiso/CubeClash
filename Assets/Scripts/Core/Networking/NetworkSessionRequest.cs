using Core.Scenes;

namespace Core.Networking
{
    public sealed class NetworkSessionRequest
    {
        public NetworkGameMode GameMode { get; }
        public string SessionName { get; }
        public GameSceneId? InitialScene { get; }

        public NetworkSessionRequest(NetworkGameMode gameMode, string sessionName, GameSceneId? initialScene = null)
        {
            GameMode = gameMode;
            SessionName = sessionName;
            InitialScene = initialScene;
        }
    }
}
