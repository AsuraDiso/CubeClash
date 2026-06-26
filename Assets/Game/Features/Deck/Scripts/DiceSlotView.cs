using Game.Scripts.Core.Data.Cards;
using Game.Shared.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Features.Deck.Scripts
{
    public sealed class DiceSlotView : MonoBehaviour, IDropHandler
    {
        [SerializeField] private TMP_Text _titleLabel;

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
            _titleLabel.text = DiceRequirementEntry.FormatLabel(requirement);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            if (!eventData.pointerDrag.TryGetComponent(out DiceView dice))
                return;

            _card.TryAssignDice(dice, this);
        }

        public bool TryAssign(DiceView dice)
        {
            if (IsFilled)
                return false;

            dice.AssignToSlot(DiceAnchor, ReleaseDice);
            _assignedDice = dice;
            return true;
        }

        public void ReleaseDice() => _assignedDice = null;

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
    }
}
