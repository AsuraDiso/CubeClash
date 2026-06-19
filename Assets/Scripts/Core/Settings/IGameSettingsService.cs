namespace Core.Settings
{
    public interface IGameSettingsService
    {
        public float MusicVolume { get; set; }
        public float SfxVolume { get; set; }
        public bool HapticsEnabled { get; set; }

        public void Load();
        public void Save();
    }

}
