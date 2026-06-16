using Bootstrap.Installers;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class LoadingLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            new LoadingInstaller().Install(builder);
        }
    }
}
