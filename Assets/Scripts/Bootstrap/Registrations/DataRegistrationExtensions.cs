using Core.Data;
using Infrastructure.Data.Mock;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.Installers
{
    public sealed class DataInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<MockGameDatabase>(Lifetime.Singleton)
                .As<IGameDatabase>();
        }
    }
}
