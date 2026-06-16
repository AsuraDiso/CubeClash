using Bootstrap.Installers;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class MainMenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            new MainMenuInstaller().Install(builder);
        }
    }
}
