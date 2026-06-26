using Game.Scripts.Bootstrap.UI.Views;
using TMPro;
using UnityEngine;
namespace Game.Features.Matchmaking.Scripts
{
    public sealed class MatchmakingView : NavigableScreenView
    {
        [SerializeField] private TMP_Text _statusText;

        public void SetStatusText(string message) => _statusText.text = message;
    }
}
