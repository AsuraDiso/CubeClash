using Cysharp.Threading.Tasks;
using Game.Scripts.Core.Scenes;
using UnityEngine;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.EntryPoints
{
    public sealed class GameBootstrapEntryPoint : IStartable
    {
        private readonly ISceneLoaderService _sceneLoaderService;

        public GameBootstrapEntryPoint(ISceneLoaderService sceneLoaderService)
        {
            _sceneLoaderService = sceneLoaderService;
        }

        public void Start() =>
            _sceneLoaderService.LoadSceneAsync(GameSceneId.Loading).Forget(Debug.LogException);
    }
}
