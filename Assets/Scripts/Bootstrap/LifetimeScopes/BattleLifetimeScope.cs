using Bootstrap.UI;
using Bootstrap.UI.Controllers;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.LifetimeScopes
{
    public sealed class BattleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<UiViewFactory>(Lifetime.Scoped).As<IUiViewFactory>();
            builder.RegisterEntryPoint<BattleController>();
        }
    }
}
