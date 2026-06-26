using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Core.Scenes
{
    public interface ISceneLoaderService
    {
        public UniTask LoadSceneAsync(GameSceneId sceneId, SceneLoadMode mode = SceneLoadMode.Single,
            CancellationToken cancellationToken = default);
    }
}
