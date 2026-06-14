using Bootstrap.EntryPoints;
using Bootstrap.Loading;
using Bootstrap.UI;
using Core.Loading;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class LoadingLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<UiViewFactory>(Lifetime.Scoped).As<IUiViewFactory>();
            builder.Register<GameDatabaseLoadingDataPreparer>(Lifetime.Scoped)
                .As<ILoadingDataPreparer>();

            builder.UseEntryPoints(entryPoints =>
            {
                entryPoints.Add<LoadingSceneEntryPoint>();
            });
        }
    }
}
