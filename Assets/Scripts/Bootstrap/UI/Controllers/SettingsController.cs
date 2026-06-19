using System;
using Bootstrap.Common;
using Bootstrap.UI.Views;
using Core.Audio;
using Core.Data;
using Core.Settings;
using UnityEngine;

namespace Bootstrap.UI.Controllers
{
    public sealed class SettingsController : IDisposable
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameSettingsService _settings;
        private readonly IAudioService _audioService;

        private SettingsView _view;

        public SettingsController(
            IPlayerRepository playerRepository,
            IGameSettingsService settings,
            IAudioService audioService)
        {
            _playerRepository = playerRepository;
            _settings = settings;
            _audioService = audioService;
        }

        public void Bind(SettingsView view)
        {
            UnbindView();
            _view = view;

            if (_view == null)
            {
                return;
            }

            _view.MusicVolumeChanged += OnMusicVolumeChanged;
            _view.SfxVolumeChanged += OnSfxVolumeChanged;
            _view.HapticsChanged += OnHapticsChanged;
            _view.UsernameSubmitRequested += OnUsernameSubmitRequested;
            _view.PrivacyPolicyClicked += OnPrivacyPolicyClicked;
            _view.TermsOfServiceClicked += OnTermsOfServiceClicked;
            _playerRepository.ProfileUpdated += RefreshUsername;

            Refresh();
        }

        public void Dispose() => UnbindView();

        private void UnbindView()
        {
            if (_view == null)
            {
                return;
            }

            _view.MusicVolumeChanged -= OnMusicVolumeChanged;
            _view.SfxVolumeChanged -= OnSfxVolumeChanged;
            _view.HapticsChanged -= OnHapticsChanged;
            _view.UsernameSubmitRequested -= OnUsernameSubmitRequested;
            _view.PrivacyPolicyClicked -= OnPrivacyPolicyClicked;
            _view.TermsOfServiceClicked -= OnTermsOfServiceClicked;
            _playerRepository.ProfileUpdated -= RefreshUsername;
            _view = null;
        }

        private void Refresh()
        {
            if (_view == null)
            {
                return;
            }

            if (_playerRepository.IsLoaded)
            {
                _view.SetUsername(_playerRepository.Profile.DisplayName);
            }

            _view.SetMusicVolume(_settings.MusicVolume);
            _view.SetSfxVolume(_settings.SfxVolume);
            _view.SetHapticsEnabled(_settings.HapticsEnabled);
        }

        private void RefreshUsername() => _view?.SetUsername(_playerRepository.Profile.DisplayName);

        private void OnMusicVolumeChanged(float volume)
        {
            _settings.MusicVolume = volume;
            _settings.Save();
        }

        private void OnSfxVolumeChanged(float volume)
        {
            _settings.SfxVolume = volume;
            _settings.Save();
            _audioService.PlaySfx(SfxId.ButtonClick);
        }

        private void OnHapticsChanged(bool enabled)
        {
            _settings.HapticsEnabled = enabled;
            _settings.Save();
        }

        private void OnUsernameSubmitRequested(string displayName) =>
            FireAndForget.Run(() => _playerRepository.UpdateDisplayNameAsync(displayName));

        private void OnPrivacyPolicyClicked()
        {
            Debug.Log("privacy");
        }
        private void OnTermsOfServiceClicked()
        {
            Debug.Log("terms");
        }
    }

}
