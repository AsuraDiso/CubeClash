using Game.Features.Home.Scripts;
using Game.Scripts.Bootstrap.Navigation;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.EntryPoints
{
    public sealed class MainMenuSceneEntryPoint : IStartable
    {
        private readonly ScreenNavigator _navigator;

        public MainMenuSceneEntryPoint(ScreenNavigator navigator) => _navigator = navigator;

        public void Start() => _navigator.Show<HomeController>();
    }
}
