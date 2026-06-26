using System;
using TMPro;
using UnityEngine;

namespace Game.Shared.Scripts.UI.Theme
{
    [CreateAssetMenu(fileName = "UiTheme", menuName = "Game/UI/Ui Theme")]
    public sealed class UiTheme : ScriptableObject
    {
        [Serializable]
        public sealed class TypographyStyle
        {
            public TMP_FontAsset Font;
            public float FontSize = 24f;
            public FontStyles FontStyle = FontStyles.Normal;
            public Color Color = Color.white;
        }

        [Header("Typography")]
        [SerializeField] private TypographyStyle _display = new() { FontSize = 48f, FontStyle = FontStyles.Bold };
        [SerializeField] private TypographyStyle _title = new() { FontSize = 32f, FontStyle = FontStyles.Bold };
        [SerializeField] private TypographyStyle _body = new() { FontSize = 22f };
        [SerializeField] private TypographyStyle _caption = new() { FontSize = 16f };
        [SerializeField] private TypographyStyle _button = new() { FontSize = 24f, FontStyle = FontStyles.Bold };

        [Header("Colors")]
        [SerializeField] private Color _textPrimary = Color.white;
        [SerializeField] private Color _textSecondary = new(1f, 1f, 1f, 0.7f);
        [SerializeField] private Color _textDisabled = new(1f, 1f, 1f, 0.4f);
        [SerializeField] private Color _tabSelected = Color.white;
        [SerializeField] private Color _tabUnselected = new(1f, 1f, 1f, 0.55f);

        [Header("Sprites")]
        [SerializeField] private Sprite _buttonBackground;
        [SerializeField] private Sprite _panelBackground;
        [SerializeField] private Sprite _sliderTrack;
        [SerializeField] private Sprite _sliderFill;
        [SerializeField] private Sprite _sliderHandle;
        [SerializeField] private Sprite _toggleBackground;
        [SerializeField] private Sprite _toggleCheckmark;
        [SerializeField] private Sprite _inputFieldBackground;

        [Header("Layout")]
        [SerializeField] private float _spacingUnit = 4f;
        [SerializeField] private float _diceSlotCellSize = 44f;
        [SerializeField] private float _diceSlotSpacing = 6f;

        public Color TextPrimary => _textPrimary;
        public Color TextSecondary => _textSecondary;
        public Color TextDisabled => _textDisabled;
        public Color TabSelected => _tabSelected;
        public Color TabUnselected => _tabUnselected;

        public Sprite ButtonBackground => _buttonBackground;
        public Sprite PanelBackground => _panelBackground;
        public Sprite SliderTrack => _sliderTrack;
        public Sprite SliderFill => _sliderFill;
        public Sprite SliderHandle => _sliderHandle;
        public Sprite ToggleBackground => _toggleBackground;
        public Sprite ToggleCheckmark => _toggleCheckmark;
        public Sprite InputFieldBackground => _inputFieldBackground;

        public float SpacingUnit => _spacingUnit;
        public float DiceSlotCellSize => _diceSlotCellSize;
        public float DiceSlotSpacing => _diceSlotSpacing;

        public TypographyStyle GetTypography(UiTypographyRole role) =>
            role switch
            {
                UiTypographyRole.Display => _display,
                UiTypographyRole.Title => _title,
                UiTypographyRole.Body => _body,
                UiTypographyRole.Caption => _caption,
                UiTypographyRole.Button => _button,
                _ => _body
            };

        public void ApplyTypography(TMP_Text text, UiTypographyRole role)
        {
            if (text == null)
                return;

            var style = GetTypography(role);
            if (style.Font != null)
                text.font = style.Font;

            text.fontSize = style.FontSize;
            text.fontStyle = style.FontStyle;
            text.color = style.Color;
        }
    }
}
