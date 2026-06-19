using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public sealed class BattleView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _localHpText;
        [SerializeField] private TMP_Text _opponentHpText;
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _enemyName;

        public void SetStatusText(string message) => _statusText.text = message;
        public void SetLocalHp(int hp) => _localHpText.text = $"My HP: {hp}";
        public void SetOpponentHp(int hp) => _opponentHpText.text = $"Enemy HP: {hp}";
        public void SetTurnText(string text) => _turnText.text = text;
        public void SetAttackEnabled(bool enabled) {}
        public void SetOpponentName(string name) => _enemyName.text = name;
        public void SetPlayerName(string name) => _playerName.text = name;
    }
}
