using DG.Tweening;
using TMPro;
using UnityEngine;
using Game.Scripts.Bootstrap.UI.Views;

namespace Game.Features.Battle.Scripts
{
    public sealed class BattleMatchIntroView : ScreenView
    {
        [SerializeField] private CanvasGroup _overlay;
        [SerializeField] private RectTransform _localPanel;
        [SerializeField] private RectTransform _opponentPanel;
        [SerializeField] private RectTransform _vsIcon;
        [SerializeField] private TMP_Text _localNameText;
        [SerializeField] private TMP_Text _opponentNameText;
        [SerializeField] private TMP_Text _countdownText;
        [SerializeField] private RectTransform _fightTitle;
        [SerializeField] private float _localRestX = -325f;
        [SerializeField] private float _opponentRestX = 325f;
        [SerializeField] private float _panelOffscreenX = 900f;

        private Vector3 _fightTitleRestScale = Vector3.one;
        private Sequence _activeSequence;

        private void Awake()
        {
            if (transform.localScale == Vector3.zero)
                transform.localScale = Vector3.one;

            if (_fightTitle != null)
                _fightTitleRestScale = _fightTitle.localScale;
        }

        private void OnDestroy() => KillActiveSequence();

        public void PrepareHidden()
        {
            KillActiveSequence();
            ResetVisualState();
            gameObject.SetActive(false);
        }

        private void ResetVisualState()
        {
            if (_overlay != null)
            {
                _overlay.alpha = 0f;
                _overlay.blocksRaycasts = false;
            }

            SetPanelX(_localPanel, -_panelOffscreenX);
            SetPanelX(_opponentPanel, _panelOffscreenX);
            SetScale(_vsIcon, 0f);
            SetText(_countdownText, string.Empty, visible: false);
            SetScale(_fightTitle, 0f);
        }

        public void BindPlayers(string localName, string opponentName)
        {
            if (_localNameText != null)
                _localNameText.text = localName;

            if (_opponentNameText != null)
                _opponentNameText.text = opponentName;
        }

        public Sequence PlayIntroSequence()
        {
            KillActiveSequence();
            gameObject.SetActive(true);
            ResetVisualState();

            if (_overlay != null)
                _overlay.blocksRaycasts = true;

            _activeSequence = DOTween.Sequence().SetUpdate(true);

            if (_overlay != null)
                _activeSequence.Append(_overlay.DOFade(1f, 0.25f));

            if (_localPanel != null)
                _activeSequence.Join(_localPanel.DOAnchorPosX(_localRestX, 0.55f).SetEase(Ease.OutBack));

            if (_opponentPanel != null)
                _activeSequence.Join(_opponentPanel.DOAnchorPosX(_opponentRestX, 0.55f).SetEase(Ease.OutBack));

            if (_vsIcon != null)
                _activeSequence.Join(_vsIcon.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.15f));

            _activeSequence.AppendInterval(0.85f);
            AppendCountdownSteps(_activeSequence);
            AppendFightStep(_activeSequence);

            if (_overlay != null)
                _activeSequence.Append(_overlay.DOFade(0f, 0.35f));

            _activeSequence.OnComplete(() =>
            {
                if (_overlay != null)
                    _overlay.blocksRaycasts = false;

                gameObject.SetActive(false);
            });

            return _activeSequence;
        }

        private void AppendCountdownSteps(Sequence sequence)
        {
            AppendCountdownStep(sequence, "3");
            AppendCountdownStep(sequence, "2");
            AppendCountdownStep(sequence, "1");
        }

        private void AppendCountdownStep(Sequence sequence, string value)
        {
            if (_countdownText == null)
            {
                sequence.AppendInterval(0.75f);
                return;
            }

            var countdownRect = _countdownText.rectTransform;
            _countdownText.gameObject.SetActive(true);
            _countdownText.text = value;
            _countdownText.alpha = 0f;
            countdownRect.localScale = Vector3.one * 1.6f;

            sequence.Append(_countdownText.DOFade(1f, 0.12f));
            sequence.Join(countdownRect.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            sequence.AppendInterval(0.28f);
            sequence.Append(_countdownText.DOFade(0f, 0.12f));
        }

        private void AppendFightStep(Sequence sequence)
        {
            if (_fightTitle == null)
            {
                sequence.AppendInterval(0.6f);
                return;
            }

            if (_countdownText != null)
                _countdownText.gameObject.SetActive(false);

            _fightTitle.gameObject.SetActive(true);
            _fightTitle.localScale = Vector3.zero;

            sequence.Append(_fightTitle.DOScale(_fightTitleRestScale, 0.35f).SetEase(Ease.OutBack));
            sequence.AppendInterval(0.45f);
        }

        private void KillActiveSequence()
        {
            if (_activeSequence == null)
                return;

            _activeSequence.Kill();
            _activeSequence = null;
        }

        private static void SetPanelX(RectTransform panel, float x)
        {
            if (panel == null)
                return;

            var anchored = panel.anchoredPosition;
            anchored.x = x;
            panel.anchoredPosition = anchored;
        }

        private static void SetScale(RectTransform target, float scale)
        {
            if (target == null)
                return;

            target.localScale = Vector3.one * scale;
        }

        private static void SetText(TMP_Text text, string value, bool visible)
        {
            if (text == null)
                return;

            text.text = value;
            text.gameObject.SetActive(visible);
            text.alpha = visible ? 1f : 0f;
        }
    }
}
