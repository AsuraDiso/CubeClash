using System;
using System.Collections.Generic;
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

        private readonly List<DiceSlotView> _diceSlots = new();

        private Canvas _dragCanvas;
        private Vector2 _originalPosition;
        private Transform _originalParent;
        private bool _cardDragEnabled = true;

        public CardDefinition Definition { get; private set; }
        public int CatalogIndex { get; private set; }
        public CardGridView OwnerGrid { get; private set; }

        public event Action<CardView, PointerEventData> DragEnded;
        public event Action<CardView> DiceAssignmentsChanged;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Initialize(CardGridView ownerGrid, Canvas dragCanvas, bool cardDragEnabled = true)
        {
            OwnerGrid = ownerGrid;
            _dragCanvas = dragCanvas;
            _cardDragEnabled = cardDragEnabled;
        }

        public void Bind(CardDefinition definition, int catalogIndex = -1)
        {
            Definition = definition;
            CatalogIndex = catalogIndex;
            gameObject.name = definition.DisplayName;
            _titleLabel.text = definition.DisplayName;

            ClearDiceSlots();

            var slotIndex = 0;
            foreach (var diceSlotDefinition in definition.DiceSlots)
            {
                foreach (var requirement in diceSlotDefinition.Requirements)
                {
                    var diceSlot = Instantiate(_diceSlotPrefab, _diceSlotsGrid);
                    diceSlot.Initialize(this, slotIndex, requirement);
                    _diceSlots.Add(diceSlot);
                    slotIndex++;
                }
            }
        }

        public bool TryAssignDice(DiceView dice, DiceSlotView slot)
        {
            if (dice == null || slot == null || slot.OwnerCard != this || dice.IsAssigned)
            {
                return false;
            }

            if (!DiceAssignmentRules.CanAssign(BuildPriorSlotValues(slot.SlotIndex), slot.SlotIndex, slot.Requirement, dice.Value))
            {
                return false;
            }

            if (!slot.TryAssign(dice))
            {
                return false;
            }

            DiceAssignmentsChanged?.Invoke(this);
            return true;
        }

        public void ClearDiceAssignments()
        {
            foreach (var slot in _diceSlots)
            {
                if (slot.AssignedDice == null)
                {
                    continue;
                }

                slot.ReleaseDice();
            }

            DiceAssignmentsChanged?.Invoke(this);
        }

        public bool AreAllSlotsFilled()
        {
            if (_diceSlots.Count == 0)
            {
                return false;
            }

            foreach (var slot in _diceSlots)
            {
                if (!slot.IsFilled)
                {
                    return false;
                }
            }

            return true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_cardDragEnabled)
            {
                return;
            }

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
            if (!_cardDragEnabled)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(RectTransform, eventData.position,
                    eventData.pressEventCamera, out var worldPoint))
            {
                RectTransform.position = worldPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_cardDragEnabled)
            {
                return;
            }

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            if (_originalParent != null)
            {
                transform.SetParent(_originalParent, true);
                RectTransform.anchoredPosition = _originalPosition;
            }

            DragEnded?.Invoke(this, eventData);
        }

        private List<int> BuildPriorSlotValues(int slotIndex)
        {
            var values = new List<int>(slotIndex);
            for (var i = 0; i < slotIndex && i < _diceSlots.Count; i++)
            {
                if (_diceSlots[i].TryGetAssignedValue(out var value))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        private void ClearDiceSlots()
        {
            _diceSlots.Clear();

            for (var i = _diceSlotsGrid.childCount - 1; i >= 0; i--)
            {
                Destroy(_diceSlotsGrid.GetChild(i).gameObject);
            }
        }
    }
}
