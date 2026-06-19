using Bootstrap.Installers;
using Bootstrap.UI.Controllers;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class MainMenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterUi();
            builder.Register<HomeController>(Lifetime.Scoped);
            builder.Register<CardController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<MainMenuController>();
            builder.RegisterEntryPoint<MatchmakingOverlayController>();
        }
    }
}
