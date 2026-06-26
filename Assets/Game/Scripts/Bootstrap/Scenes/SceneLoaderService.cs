using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Core.Scenes;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Bootstrap.Scenes
{
    public sealed class SceneLoaderService : ISceneLoaderService
    {
        public async UniTask LoadSceneAsync(GameSceneId sceneId, SceneLoadMode mode = SceneLoadMode.Single, CancellationToken cancellationToken = default)
        {
            var unityMode = mode == SceneLoadMode.Single ? LoadSceneMode.Single : LoadSceneMode.Additive;
            var sceneName = sceneId.ToString();
            var operation = SceneManager.LoadSceneAsync(sceneName, unityMode);
            if (operation == null)
                throw new InvalidOperationException($"Failed to load scene '{sceneName}'.");

            await operation.ToUniTask(cancellationToken: cancellationToken);
        }
    }
}
