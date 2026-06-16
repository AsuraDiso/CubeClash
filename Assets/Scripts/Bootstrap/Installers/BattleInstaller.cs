using Bootstrap.UI.Controllers;
using VContainer;
using VContainer.Unity;

namespace Bootstrap.Installers
{
    public sealed class BattleInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterUi();
            builder.RegisterEntryPoint<BattleController>();
        }
    }
}
