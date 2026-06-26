using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Shared.Scripts.UI
{
    public sealed class DiceView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _value;
        [SerializeField] private List<Sprite> _sprites;
        [SerializeField] private CanvasGroup _canvasGroup;

        private RectTransform _rectTransform;
        private Canvas _dragCanvas;
        private Transform _homeParent;
        private Action _releaseSlot;
        private int _diceMin = 1;
        private int _diceMax = 6;

        public int DiceIndex { get; private set; }
        public int Value { get; private set; }
        public bool IsAssigned => _releaseSlot != null;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        public void Bind(Canvas dragCanvas, int diceIndex, RectTransform homeParent, int diceMin = 1, int diceMax = 6)
        {
            ResetForPool();
            _dragCanvas = dragCanvas;
            _homeParent = homeParent;
            DiceIndex = diceIndex;
            _diceMin = diceMin;
            _diceMax = diceMax;
        }

        public void ResetForPool()
        {
            _releaseSlot = null;

            if (_homeParent != null)
                transform.SetParent(_homeParent, false);
        }

        public void SetValue(int value)
        {
            Value = Mathf.Clamp(value, _diceMin, _diceMax);

            var spriteIndex = Mathf.Clamp(Value - _diceMin, 0, _sprites.Count - 1);
            _value.sprite = _sprites[spriteIndex];
        }

        public void SetInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
        }

        public void AssignToSlot(RectTransform slotAnchor, Action releaseSlot)
        {
            _releaseSlot = releaseSlot;
            transform.SetParent(slotAnchor, false);
            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _rectTransform.localScale = Vector3.one;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_releaseSlot != null)
            {
                _releaseSlot.Invoke();
                _releaseSlot = null;
            }

            UiDragHelper.Begin(_canvasGroup, transform, _dragCanvas, 0.85f);
        }

        public void OnDrag(PointerEventData eventData) => UiDragHelper.Move(_rectTransform, eventData);

        public void OnEndDrag(PointerEventData eventData)
        {
            UiDragHelper.End(_canvasGroup);

            if (!IsAssigned)
                transform.SetParent(_homeParent, false);
        }
    }
}
