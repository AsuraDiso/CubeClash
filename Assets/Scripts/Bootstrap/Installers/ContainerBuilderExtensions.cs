using Bootstrap.UI;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.Installers
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterUi(this IContainerBuilder builder)
        {
            builder.Register<UiViewFactory>(Lifetime.Scoped).As<IUiViewFactory>();
        }

        public static void InstallData(this IContainerBuilder builder)
        {
            new DataInstaller().Install(builder);
        }

        public static void InstallMultiplayer(this IContainerBuilder builder)
        {
            new MultiplayerInstaller().Install(builder);
        }
    }
}
