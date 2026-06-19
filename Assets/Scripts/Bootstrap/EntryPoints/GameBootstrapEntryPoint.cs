using Bootstrap.Common;
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
            Debug.Log("Bootstrap done.");
            FireAndForget.Run(() => _sceneLoaderService.LoadSceneAsync(GameSceneId.Loading));
        }
    }
}
