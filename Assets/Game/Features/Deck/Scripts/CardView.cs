using System;
using System.Collections.Generic;
using Game.Scripts.Core.Battle.Simulation;
using Game.Scripts.Core.Data.Cards;
using Game.Shared.Scripts.UI;
using Game.Shared.Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Features.Deck.Scripts
{
    public sealed class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _descriptionLabel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private DiceSlotView _diceSlotPrefab;
        [SerializeField] private RectTransform _diceSlotsGrid;

        private readonly List<DiceSlotView> _diceSlots = new();
        private readonly List<int> _priorSlotValues = new();

        private Canvas _dragCanvas;
        private Vector2 _originalPosition;
        private Transform _originalParent;
        private bool _cardDragEnabled = true;

        public CardDefinition Definition { get; private set; }
        public int CatalogIndex { get; private set; }
        public int DeckIndex { get; private set; } = -1;
        public CardGridView OwnerGrid { get; private set; }

        public event Action<CardView, PointerEventData> CardDragEnded;
        public event Action<CardView> CardClicked;
        public event Action<CardView> DiceAssignmentsChanged;
        public Action<DiceView> DiceReleased;
        public Func<int[], bool> ValidateAssignment { get; set; }

        public void Initialize(CardGridView ownerGrid, Canvas dragCanvas, bool cardDragEnabled = true)
        {
            OwnerGrid = ownerGrid;
            _dragCanvas = dragCanvas;
            _cardDragEnabled = cardDragEnabled;
        }

        public void Bind(CardDefinition definition, int catalogIndex = -1, int deckIndex = -1)
        {
            Definition = definition;
            CatalogIndex = catalogIndex;
            DeckIndex = deckIndex;
            gameObject.name = definition.DisplayName;
            _titleLabel.text = definition.DisplayName;

            ClearDiceSlots();

            var slotIndex = 0;
            foreach (var requirement in definition.GetFlatRequirements())
            {
                var diceSlot = Instantiate(_diceSlotPrefab, _diceSlotsGrid);
                diceSlot.Initialize(this, slotIndex, requirement);
                _diceSlots.Add(diceSlot);
                slotIndex++;
            }

            ConfigureDiceGrid(definition);
            _diceSlotsGrid.SetAsLastSibling();

            if (_descriptionLabel != null)
            {
                _descriptionLabel.raycastTarget = false;
                _descriptionLabel.gameObject.SetActive(false);
            }
        }

        public bool TryAssignDice(DiceView dice, DiceSlotView slot)
        {
            if (slot.OwnerCard != this || dice.IsAssigned)
                return false;

            if (IsDieAlreadyAssigned(dice, slot))
                return false;

            if (!IsSlotRequirementMet(slot, dice))
                return false;

            if (ValidateAssignment != null
                && TryBuildHypotheticalIndices(slot.SlotIndex, dice, out var hypothetical)
                && !ValidateAssignment(hypothetical))
                return false;

            if (!slot.TryAssign(dice))
                return false;

            DiceAssignmentsChanged?.Invoke(this);
            return true;
        }

        public void ClearDiceAssignments()
        {
            foreach (var slot in _diceSlots)
            {
                if (slot.AssignedDice == null)
                    continue;

                var dice = slot.AssignedDice;
                slot.ReleaseDice();
                dice.ResetForPool();
                DiceReleased?.Invoke(dice);
            }

            DiceAssignmentsChanged?.Invoke(this);
        }

        public void CollectAssignedDieIndices(HashSet<int> indices)
        {
            foreach (var slot in _diceSlots)
            {
                if (slot.AssignedDice != null)
                    indices.Add(slot.AssignedDice.DiceIndex);
            }
        }

        public void ReleaseAssignedConsumedDice(Func<int, bool> isConsumed, Action<DiceView> releaseDie)
        {
            foreach (var slot in _diceSlots)
            {
                var dice = slot.AssignedDice;
                if (dice == null || !isConsumed(dice.DiceIndex))
                    continue;

                slot.ReleaseDice();
                dice.ResetForPool();
                releaseDie?.Invoke(dice);
            }
        }

        public bool AreAllSlotsFilled()
        {
            if (_diceSlots.Count == 0)
                return true;

            foreach (var slot in _diceSlots)
                if (!slot.IsFilled)
                    return false;

            return true;
        }

        public bool TryGetAssignedDieIndices(out int[] dieIndices)
        {
            dieIndices = new int[_diceSlots.Count];
            for (var i = 0; i < _diceSlots.Count; i++)
            {
                var dice = _diceSlots[i].AssignedDice;
                if (dice == null)
                {
                    dieIndices = null;
                    return false;
                }

                dieIndices[i] = dice.DiceIndex;
            }

            return true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_cardDragEnabled)
                return;

            _originalParent = transform.parent;
            _originalPosition = RectTransform.anchoredPosition;
            UiDragHelper.Begin(_canvasGroup, transform, _dragCanvas, 0.6f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_cardDragEnabled)
                return;

            UiDragHelper.Move(RectTransform, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_cardDragEnabled)
                return;

            UiDragHelper.End(_canvasGroup);

            transform.SetParent(_originalParent, true);
            RectTransform.anchoredPosition = _originalPosition;

            CardDragEnded?.Invoke(this, eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_cardDragEnabled)
                return;

            CardClicked?.Invoke(this);
        }

        private bool IsDieAlreadyAssigned(DiceView dice, DiceSlotView slot)
        {
            foreach (var diceSlot in _diceSlots)
            {
                if (diceSlot == slot || diceSlot.AssignedDice == null)
                    continue;

                if (diceSlot.AssignedDice.DiceIndex == dice.DiceIndex)
                    return true;
            }

            return false;
        }

        private bool IsSlotRequirementMet(DiceSlotView slot, DiceView dice)
        {
            _priorSlotValues.Clear();
            foreach (var diceSlot in _diceSlots)
            {
                if (diceSlot.SlotIndex >= slot.SlotIndex)
                    break;

                if (diceSlot.TryGetAssignedValue(out var value))
                    _priorSlotValues.Add(value);
            }

            return DiceAssignmentValidator.IsSlotValid(
                slot.SlotIndex,
                dice.Value,
                Definition.GetFlatRequirements(),
                _priorSlotValues);
        }

        private bool TryBuildHypotheticalIndices(int slotIndex, DiceView dice, out int[] dieIndices)
        {
            dieIndices = new int[_diceSlots.Count];
            for (var i = 0; i < _diceSlots.Count; i++)
            {
                var diceSlot = _diceSlots[i];
                if (diceSlot.SlotIndex == slotIndex)
                {
                    dieIndices[i] = dice.DiceIndex;
                    continue;
                }

                if (diceSlot.AssignedDice == null)
                {
                    dieIndices = null;
                    return false;
                }

                dieIndices[i] = diceSlot.AssignedDice.DiceIndex;
            }

            return true;
        }

        private void ClearDiceSlots()
        {
            _diceSlots.Clear();

            for (var i = _diceSlotsGrid.childCount - 1; i >= 0; i--)
                Destroy(_diceSlotsGrid.GetChild(i).gameObject);
        }

        private void ConfigureDiceGrid(CardDefinition definition)
        {
            if (_diceSlotsGrid == null)
                return;

            var grid = _diceSlotsGrid.GetComponent<GridLayoutGroup>();
            if (grid == null)
                return;

            var theme = UiThemeAccess.Current;
            var cellSize = theme != null ? theme.DiceSlotCellSize : 44f;
            var spacing = theme != null ? theme.DiceSlotSpacing : 6f;
            grid.cellSize = new Vector2(cellSize, cellSize);
            grid.spacing = new Vector2(spacing, spacing);

            var slotCount = _diceSlots.Count;
            if (slotCount <= 0)
                return;

            if (definition.Layout == CardLayout.Vertical)
            {
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 1;
            }
            else if (slotCount > 2)
            {
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2;
            }
            else
            {
                grid.constraint = GridLayoutGroup.Constraint.Flexible;
            }

            var columns = definition.Layout == CardLayout.Vertical ? 1 : Mathf.Min(slotCount, 2);
            var rows = definition.Layout == CardLayout.Vertical
                ? slotCount
                : (slotCount + columns - 1) / columns;

            _diceSlotsGrid.anchorMin = new Vector2(0.5f, 0.5f);
            _diceSlotsGrid.anchorMax = new Vector2(0.5f, 0.5f);
            _diceSlotsGrid.pivot = new Vector2(0.5f, 0.5f);
            _diceSlotsGrid.anchoredPosition = new Vector2(0f, -12f);
            _diceSlotsGrid.sizeDelta = new Vector2(
                columns * cellSize + (columns - 1) * spacing,
                rows * cellSize + (rows - 1) * spacing);
        }
    }
}
