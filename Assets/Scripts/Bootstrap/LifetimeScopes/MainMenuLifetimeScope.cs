using Bootstrap.Audio;
using Bootstrap.Installers;
using Bootstrap.UI.Controllers;
using Core.Audio;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class MainMenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterUi();
            builder.RegisterInstance(new SceneMusicBinding(MusicId.MainMenu));
            builder.RegisterEntryPoint<SceneMusicStarter>();
            builder.Register<HomeController>(Lifetime.Scoped);
            builder.Register<CardController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<MainMenuController>();
            builder.RegisterEntryPoint<MatchmakingOverlayController>();
        }
    }
}
