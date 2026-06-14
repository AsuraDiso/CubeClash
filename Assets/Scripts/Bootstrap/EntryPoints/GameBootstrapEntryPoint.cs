using Core.Scenes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Bootstrap.EntryPoints
{
    public sealed class GameBootstrapEntryPoint : IStartable
    {
        private readonly ISceneLoaderService _sceneLoaderService;

        public GameBootstrapEntryPoint(ISceneLoaderService sceneLoaderService)
        {
            _sceneLoaderService = sceneLoaderService;
        }

        public void Start()
        {
            Debug.Log("[CubeClash] Bootstrap complete. Loading initial scene.");
            _sceneLoaderService.LoadSceneAsync(GameSceneId.Loading).Forget();
        }
    }
}
