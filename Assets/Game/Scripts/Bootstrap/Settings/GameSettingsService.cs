using Game.Scripts.Core.Audio;
using Game.Scripts.Core.Settings;
using UnityEngine;

namespace Game.Scripts.Bootstrap.Settings
{
    public sealed class GameSettingsService : IGameSettingsService
    {
        private const string MusicVolumeKey = "settings.music_volume";
        private const string SfxVolumeKey = "settings.sfx_volume";
        private const string HapticsKey = "settings.haptics";

        private readonly IAudioService _audioService;

        public GameSettingsService(IAudioService audioService)
        {
            _audioService = audioService;
            Load();
        }

        public float MusicVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;
        public bool HapticsEnabled { get; set; } = true;

        public void Load()
        {
            MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            HapticsEnabled = PlayerPrefs.GetInt(HapticsKey, 1) == 1;
            ApplyToAudio();
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
            PlayerPrefs.SetInt(HapticsKey, HapticsEnabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyToAudio();
        }

        private void ApplyToAudio()
        {
            _audioService.MusicVolume = MusicVolume;
            _audioService.SfxVolume = SfxVolume;
            _audioService.MusicMuted = MusicVolume <= 0f;
            _audioService.SfxMuted = SfxVolume <= 0f;
        }
    }
}
