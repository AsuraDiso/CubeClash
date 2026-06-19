using Bootstrap.UI;
using VContainer;

namespace Bootstrap.Installers
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterUi(this IContainerBuilder builder)
        {
            builder.Register<UiViewFactory>(Lifetime.Scoped).As<IUiViewFactory>();
        }
    }
}
