using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Scenes
{
    public interface ISceneLoaderService
    {
        UniTask LoadSceneAsync(
            GameSceneId sceneId,
            SceneLoadMode mode = SceneLoadMode.Single,
            CancellationToken cancellationToken = default);
    }
}
