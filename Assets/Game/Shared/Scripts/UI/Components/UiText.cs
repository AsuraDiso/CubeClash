using Game.Shared.Scripts.UI.Theme;
using TMPro;
using UnityEngine;

namespace Game.Shared.Scripts.UI.Components
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class UiText : MonoBehaviour
    {
        [SerializeField] private UiTypographyRole _role = UiTypographyRole.Body;
        [SerializeField] private bool _applyOnAwake = true;

        private TMP_Text _text;

        public TMP_Text Text => _text != null ? _text : _text = GetComponent<TMP_Text>();

        private void Awake()
        {
            if (_applyOnAwake)
                Apply();
        }

        public void Apply()
        {
            var theme = UiThemeAccess.Current;
            if (theme == null)
                return;

            theme.ApplyTypography(Text, _role);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && UiThemeAccess.Current != null)
                Apply();
        }
#endif
    }
}
