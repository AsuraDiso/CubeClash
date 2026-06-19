using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public sealed class DiceView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _value;
        [SerializeField] private List<Sprite> _sprites;
        [SerializeField] private CanvasGroup _canvasGroup;

        private RectTransform _rectTransform;
        private Canvas _dragCanvas;
        private Transform _homeParent;
        private Vector2 _homeAnchoredPosition;
        private DiceSlotView _assignedSlot;

        public int DiceIndex { get; private set; }
        public int Value { get; private set; }
        public bool IsAssigned => _assignedSlot != null;

        public event Action<DiceView> DragEnded;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            StripNestedCanvas();
        }

        public void Initialize(Canvas dragCanvas, int diceIndex, Vector2 homeAnchoredPosition)
        {
            _dragCanvas = dragCanvas;
            DiceIndex = diceIndex;
            _homeAnchoredPosition = homeAnchoredPosition;
            _homeParent = transform.parent;
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = homeAnchoredPosition;
            _rectTransform.sizeDelta = new Vector2(72f, 72f);
            _rectTransform.localScale = Vector3.one;
        }

        public void SetValue(int value)
        {
            Value = Mathf.Clamp(value, 1, 6);
            if (_value == null || _sprites == null || _sprites.Count == 0)
            {
                return;
            }

            var spriteIndex = Mathf.Clamp(Value - 1, 0, _sprites.Count - 1);
            _value.sprite = _sprites[spriteIndex];
        }

        public void AssignToSlot(DiceSlotView slot)
        {
            _assignedSlot = slot;
            transform.SetParent(slot.DiceAnchor, false);
            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _rectTransform.localScale = Vector3.one;
        }

        public void ReturnHome()
        {
            _assignedSlot = null;
            transform.SetParent(_homeParent, false);
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = _homeAnchoredPosition;
            _rectTransform.sizeDelta = new Vector2(72f, 72f);
            _rectTransform.localScale = Vector3.one;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.85f;

            if (_assignedSlot != null)
            {
                _assignedSlot.ReleaseDice();
                _assignedSlot = null;
            }

            if (_dragCanvas != null)
            {
                transform.SetParent(_dragCanvas.transform, true);
                transform.SetAsLastSibling();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var worldPoint))
            {
                _rectTransform.position = worldPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            if (!IsAssigned)
            {
                ReturnHome();
            }

            DragEnded?.Invoke(this);
        }

        private void StripNestedCanvas()
        {
            var nestedCanvas = GetComponent<Canvas>();
            if (nestedCanvas == null)
            {
                return;
            }

            Destroy(nestedCanvas);
            var scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null)
            {
                Destroy(scaler);
            }

            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                Destroy(raycaster);
            }
        }
    }
}
