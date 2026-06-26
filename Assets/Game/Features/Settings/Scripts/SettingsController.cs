using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.Bootstrap.Navigation;
using Game.Scripts.Core.Audio;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Settings;
using UnityEngine;

namespace Game.Features.Settings.Scripts
{
    public sealed class SettingsController : IDisposable, IScreenShownHandler
    {
        private readonly SettingsView _view;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameSettingsService _settings;
        private readonly IAudioService _audioService;
        private readonly IHapticsService _haptics;

        public SettingsController(SettingsView view, IPlayerRepository playerRepository, IGameSettingsService settings,
            IAudioService audioService, IHapticsService haptics)
        {
            _view = view;
            _playerRepository = playerRepository;
            _settings = settings;
            _audioService = audioService;
            _haptics = haptics;

            _view.MusicVolumeChanged += HandleMusicVolumeChanged;
            _view.SfxVolumeChanged += HandleSfxVolumeChanged;
            _view.HapticsChanged += HandleHapticsChanged;
            _view.UsernameSubmitRequested += HandleUsernameSubmitRequested;
            _view.PrivacyPolicyClicked += HandlePrivacyPolicyClicked;
            _view.TermsOfServiceClicked += HandleTermsOfServiceClicked;
            _playerRepository.ProfileUpdated += HandleProfileUpdated;
        }

        public void OnScreenShown() => Refresh();

        public void Dispose()
        {
            _view.MusicVolumeChanged -= HandleMusicVolumeChanged;
            _view.SfxVolumeChanged -= HandleSfxVolumeChanged;
            _view.HapticsChanged -= HandleHapticsChanged;
            _view.UsernameSubmitRequested -= HandleUsernameSubmitRequested;
            _view.PrivacyPolicyClicked -= HandlePrivacyPolicyClicked;
            _view.TermsOfServiceClicked -= HandleTermsOfServiceClicked;
            _playerRepository.ProfileUpdated -= HandleProfileUpdated;
        }

        private void Refresh()
        {
            if (_playerRepository.IsLoaded)
                _view.SetUsername(_playerRepository.Profile.DisplayName);

            _view.SetMusicVolume(_settings.MusicVolume);
            _view.SetSfxVolume(_settings.SfxVolume);
            _view.SetHapticsEnabled(_settings.HapticsEnabled);
        }

        private void HandleProfileUpdated() => _view.SetUsername(_playerRepository.Profile.DisplayName);

        private void HandleMusicVolumeChanged(float volume) => SetVolume(volume, playSfx: false);

        private void HandleSfxVolumeChanged(float volume) => SetVolume(volume, playSfx: true);

        private void SetVolume(float volume, bool playSfx)
        {
            if (playSfx)
                _settings.SfxVolume = volume;
            else
                _settings.MusicVolume = volume;

            _settings.Save();

            if (playSfx)
                _audioService.PlaySfx(SfxId.ButtonClick);
        }

        private void HandleHapticsChanged(bool enabled)
        {
            _settings.HapticsEnabled = enabled;
            _settings.Save();

            if (enabled)
                _haptics.PlayLight();
        }

        private void HandleUsernameSubmitRequested(string displayName) =>
            _playerRepository.UpdateDisplayNameAsync(displayName).Forget(Debug.LogException);

        private void HandlePrivacyPolicyClicked() {}

        private void HandleTermsOfServiceClicked() {}
    }
}
