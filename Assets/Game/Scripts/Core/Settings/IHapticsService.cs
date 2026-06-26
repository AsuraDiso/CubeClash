namespace Game.Scripts.Core.Settings
{
    public interface IHapticsService
    {
        bool IsEnabled { get; }

        void PlayLight();
    }
}
