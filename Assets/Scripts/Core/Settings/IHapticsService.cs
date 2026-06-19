namespace Core.Settings
{
    public interface IHapticsService
    {
        public bool IsEnabled { get; }

        public void PlayLight();
    }

}
