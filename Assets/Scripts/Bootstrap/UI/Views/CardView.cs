using System;
using Cards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bootstrap.UI.Views
{
    public sealed class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private DiceSlotView _diceSlotPrefab;
        [SerializeField] private RectTransform _diceSlotsGrid;

        public CardDefinition Definition { get; private set; }
        public int CatalogIndex { get; private set; }
        public CardGridView OwnerGrid { get; private set; }

        private Canvas _dragCanvas;
        private Vector2 _originalPosition;
        private Transform _originalParent;

        public event Action<CardView, PointerEventData> DragEnded;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Initialize(CardGridView ownerGrid, Canvas dragCanvas)
        {
            OwnerGrid = ownerGrid;
            _dragCanvas = dragCanvas;
        }

        public void Bind(CardDefinition definition, int catalogIndex = -1)
        {
            Definition = definition;
            CatalogIndex = catalogIndex;
            gameObject.name = definition.DisplayName;
            _titleLabel.text = definition.DisplayName;

            foreach (var def in definition.DiceSlots)
            foreach (var requirement in def.Requirements)
            {
                var diceSlot = Instantiate(_diceSlotPrefab, _diceSlotsGrid);
                diceSlot.Bind(requirement);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.6f;

            _originalParent = transform.parent;
            _originalPosition = RectTransform.anchoredPosition;

            if (_dragCanvas != null)
            {
                transform.SetParent(_dragCanvas.transform, true);
                transform.SetAsLastSibling();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(RectTransform, eventData.position,
                    eventData.pressEventCamera, out var worldPoint))
            {
                RectTransform.position = worldPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            if (_originalParent != null)
            {
                transform.SetParent(_originalParent, true);
                RectTransform.anchoredPosition = _originalPosition;
            }

            DragEnded?.Invoke(this, eventData);
        }
    }
}
