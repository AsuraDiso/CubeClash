using Bootstrap.Installers;
using Bootstrap.UI.Controllers;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class BattleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterUi();
            builder.RegisterEntryPoint<BattleController>();
        }
    }
}
