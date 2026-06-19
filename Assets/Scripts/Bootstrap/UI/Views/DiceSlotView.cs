using Cards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public sealed class DiceSlotView : MonoBehaviour, IDropHandler
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private Image _background;
        [SerializeField] private Color _emptyColor = new(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color _filledColor = new(0.4f, 0.9f, 0.5f, 0.85f);

        private CardView _card;
        private DiceRequirementEntry _requirement;
        private DiceView _assignedDice;

        public int SlotIndex { get; private set; }
        public CardView OwnerCard => _card;
        public DiceRequirementEntry Requirement => _requirement;
        public DiceView AssignedDice => _assignedDice;
        public bool IsFilled => _assignedDice != null;
        public RectTransform DiceAnchor => transform as RectTransform;

        public void Initialize(CardView card, int slotIndex, DiceRequirementEntry requirement)
        {
            _card = card;
            SlotIndex = slotIndex;
            _requirement = requirement;
            Bind(requirement);
        }

        public void Bind(DiceRequirementEntry entry)
        {
            _titleLabel.text = entry.Kind switch
            {
                DiceRequirementKind.LessThan => $"< {entry.Value}",
                DiceRequirementKind.Even => "Even",
                DiceRequirementKind.DistinctFromOtherSlots => "Unique",
                _ => "?"
            };

            RefreshVisual();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null || _card == null)
            {
                return;
            }

            var dice = eventData.pointerDrag.GetComponent<DiceView>();
            if (dice == null)
            {
                return;
            }

            _card.TryAssignDice(dice, this);
        }

        public bool TryAssign(DiceView dice)
        {
            if (IsFilled || dice == null)
            {
                return false;
            }

            dice.AssignToSlot(this);
            _assignedDice = dice;
            RefreshVisual();
            return true;
        }

        public void ReleaseDice()
        {
            _assignedDice = null;
            RefreshVisual();
        }

        public bool TryGetAssignedValue(out int value)
        {
            if (_assignedDice == null)
            {
                value = 0;
                return false;
            }

            value = _assignedDice.Value;
            return true;
        }

        private void RefreshVisual()
        {
            if (_background != null)
            {
                _background.color = IsFilled ? _filledColor : _emptyColor;
            }
        }
    }
}
