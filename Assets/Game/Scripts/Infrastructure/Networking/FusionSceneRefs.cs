using Fusion;
using Game.Scripts.Core.Scenes;

namespace Game.Scripts.Infrastructure.Networking
{
    internal static class FusionSceneRefs
    {
        public static SceneRef FromGameScene(GameSceneId sceneId) => SceneRef.FromIndex((int)sceneId);
    }
}
