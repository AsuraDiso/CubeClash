using System;
using System.Threading;
using Core.Scenes;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine.SceneManagement;

namespace Infrastructure.Photon
{
    public sealed class FusionBattleSceneLoader : IBattleSceneLoader
    {
        private readonly IFusionRunnerAccessor _runnerAccessor;
        private readonly ISceneLoaderService _sceneLoaderService;

        public FusionBattleSceneLoader(
            IFusionRunnerAccessor runnerAccessor,
            ISceneLoaderService sceneLoaderService)
        {
            _runnerAccessor = runnerAccessor;
            _sceneLoaderService = sceneLoaderService;
        }

        public async UniTask LoadBattleSceneAsync(CancellationToken cancellationToken = default)
        {
            var runner = _runnerAccessor.ActiveRunner;
            if (runner != null && runner.IsRunning)
            {
                if (!runner.IsServer)
                {
                    return;
                }

                var sceneName = GameSceneId.Battle.ToString();
                var operation = runner.LoadScene(sceneName, LoadSceneMode.Single);
                await WaitForSceneOpAsync(operation, cancellationToken);
                return;
            }

            await _sceneLoaderService.LoadSceneAsync(GameSceneId.Battle, cancellationToken: cancellationToken);
        }

        private static async UniTask WaitForSceneOpAsync(NetworkSceneAsyncOp operation, CancellationToken cancellationToken)
        {
            while (operation.IsValid && !operation.IsDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield(cancellationToken);
            }

            if (operation.IsValid && operation.Error != null)
            {
                throw new InvalidOperationException($"Failed to load battle scene: {operation.Error}");
            }
        }
    }
}
