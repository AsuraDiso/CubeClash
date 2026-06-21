using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public sealed class SettingsView : MonoBehaviour, INavigableView
    {
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _hapticsToggle;

        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        public event Action<bool> HapticsChanged;
        public event Action<string> UsernameSubmitRequested;
        public event Action PrivacyPolicyClicked;
        public event Action TermsOfServiceClicked;
        public event Action BackClicked;

        public void SetUsername(string displayName) => _usernameInput.SetTextWithoutNotify(displayName ?? string.Empty);

        public void SetMusicVolume(float volume) => _musicVolumeSlider.SetValueWithoutNotify(Mathf.Clamp01(volume));

        public void SetSfxVolume(float volume) => _sfxVolumeSlider.SetValueWithoutNotify(Mathf.Clamp01(volume));

        public void SetHapticsEnabled(bool enabled) => _hapticsToggle.SetIsOnWithoutNotify(enabled);

        public void OnMusicVolumeChanged(float volume) => MusicVolumeChanged?.Invoke(volume);

        public void OnSfxVolumeChanged(float volume) => SfxVolumeChanged?.Invoke(volume);

        public void OnHapticsChanged(bool enabled) => HapticsChanged?.Invoke(enabled);

        public void OnUsernameSubmitRequested(string displayName) => UsernameSubmitRequested?.Invoke(displayName);

        public void OnPrivacyPolicyClicked() => PrivacyPolicyClicked?.Invoke();

        public void OnTermsOfServiceClicked() => TermsOfServiceClicked?.Invoke();

        public void OnBackButtonClicked() => BackClicked?.Invoke();
    }
}
