using Bootstrap.EntryPoints;
using Bootstrap.Installers;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class LoadingLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterUi();
            builder.RegisterEntryPoint<LoadingSceneEntryPoint>();
        }
    }
}
