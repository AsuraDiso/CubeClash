using Cards;
using TMPro;
using UnityEngine;

namespace Bootstrap.UI.Views
{
    public class DiceSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        public void Bind(DiceRequirementEntry entry)
        {
            _titleLabel.text = entry.Kind switch
            {
                DiceRequirementKind.LessThan => $"Less than\n{entry.Value}",
                DiceRequirementKind.Even => "Even",
                _ => _titleLabel.text
            };
        }
    }
}
