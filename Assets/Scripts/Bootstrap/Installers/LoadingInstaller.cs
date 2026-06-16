using Bootstrap.EntryPoints;
using Bootstrap.Loading;
using Core.Loading;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.Installers
{
    public sealed class LoadingInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterUi();
            builder.Register<GameDatabaseLoadingDataPreparer>(Lifetime.Scoped)
                .As<ILoadingDataPreparer>();
            builder.RegisterEntryPoint<LoadingSceneEntryPoint>();
        }
    }
}
