using Game.Scripts.Core.Settings;
using UnityEngine;

namespace Game.Scripts.Bootstrap.Settings
{
    public sealed class HapticsService : IHapticsService
    {
        private readonly IGameSettingsService _settings;

        public HapticsService(IGameSettingsService settings)
        {
            _settings = settings;
        }

        public bool IsEnabled => _settings.HapticsEnabled;

        public void PlayLight()
        {
            if (!IsEnabled)
                return;

            #if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Handheld.Vibrate();
            #endif
        }
    }

}
