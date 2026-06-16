using Bootstrap.UI.Controllers;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.Installers
{
    public sealed class MainMenuInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterUi();
            builder.Register<HomeController>(Lifetime.Scoped);
            builder.Register<CardController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<MainMenuController>();
            builder.RegisterEntryPoint<MatchmakingOverlayController>();
        }
    }
}
