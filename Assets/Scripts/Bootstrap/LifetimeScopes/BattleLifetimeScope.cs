using Bootstrap.Audio;
using Bootstrap.Installers;
using Bootstrap.UI.Controllers;
using Core.Audio;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class BattleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterUi();
            builder.RegisterInstance(new SceneMusicBinding(MusicId.Battle));
            builder.RegisterEntryPoint<SceneMusicStarter>();
            builder.RegisterEntryPoint<BattleController>();
        }
    }
}
