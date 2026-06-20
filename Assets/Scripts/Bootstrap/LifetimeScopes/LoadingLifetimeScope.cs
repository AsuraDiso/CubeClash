using Bootstrap.EntryPoints;
using Bootstrap.Scenes;
using Bootstrap.UI;
using Bootstrap.UI.Controllers;
using Core.Scenes;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class LoadingLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<UiViewFactory>(Lifetime.Scoped).As<IUiViewFactory>();
            builder.Register<LoadingProgress>(Lifetime.Scoped).As<ILoadingProgress>();
            builder.RegisterEntryPoint<LoadingController>();
            builder.RegisterEntryPoint<LoadingSceneEntryPoint>();
        }
    }
}
