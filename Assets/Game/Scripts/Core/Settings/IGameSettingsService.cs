namespace Game.Scripts.Core.Settings
{
    public interface IGameSettingsService
    {
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
        bool HapticsEnabled { get; set; }

        void Load();
        void Save();
    }
}
