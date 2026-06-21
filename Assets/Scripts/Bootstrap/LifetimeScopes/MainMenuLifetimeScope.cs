using Bootstrap.Audio;
using Bootstrap.UI;
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
            builder.Register<UiViewFactory>(Lifetime.Scoped).As<IUiViewFactory>();
            builder.RegisterInstance(new SceneMusicBinding(MusicId.MainMenu));
            builder.RegisterEntryPoint<SceneMusicStarter>();
            builder.RegisterEntryPoint<HomeController>(Lifetime.Scoped);
            builder.Register<CardController>(Lifetime.Scoped);
            builder.Register<SettingsController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<MatchmakingOverlayController>();
        }
    }
}
