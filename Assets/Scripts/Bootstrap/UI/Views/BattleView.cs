using System;
using System.Collections.Generic;
using Cards;
using Core.Battle;
using TMPro;
using UnityEngine;

namespace Bootstrap.UI.Views
{
    public sealed class BattleView : MonoBehaviour
    {
        private const float DiceSpacing = 84f;

        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _localHpText;
        [SerializeField] private TMP_Text _opponentHpText;
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _enemyName;
        [SerializeField] private CardGridView _localDeckGrid;
        [SerializeField] private CardGridView _opponentDeckGrid;
        [SerializeField] private RectTransform _diceTray;
        [SerializeField] private DiceView _dicePrefab;
        [SerializeField] private Canvas _dragCanvas;

        private readonly List<DiceView> _spawnedDice = new();

        public event Action<CardView> CardDiceAssignmentsChanged;

        private void Awake()
        {
            _localDeckGrid ??= transform.Find("CardDeck/CardGrid")?.GetComponent<CardGridView>();
            _dragCanvas ??= GetComponent<Canvas>();
            EnsureDiceTray();
            ConfigureBattleGrids();
        }

        public void SetStatusText(string message) => _statusText.text = message;
        public void SetLocalHp(int hp) => _localHpText.text = $"My HP: {hp}";
        public void SetOpponentHp(int hp) => _opponentHpText.text = $"Enemy HP: {hp}";
        public void SetTurnText(string text) => _turnText.text = text;
        public void SetAttackEnabled(bool enabled) { }
        public void SetOpponentName(string name) => _enemyName.text = name;
        public void SetPlayerName(string name) => _playerName.text = name;

        public void SetLocalDeck(IReadOnlyList<PlacedCard> placedCards)
        {
            _localDeckGrid?.SetCards(placedCards);
            WireLocalCardEvents();
        }

        public void SetOpponentDeck(IReadOnlyList<PlacedCard> placedCards) => _opponentDeckGrid?.SetCards(placedCards);

        public void ClearTurnDice()
        {
            for (var i = _spawnedDice.Count - 1; i >= 0; i--)
            {
                if (_spawnedDice[i] != null)
                {
                    Destroy(_spawnedDice[i].gameObject);
                }
            }

            _spawnedDice.Clear();
            _localDeckGrid?.ClearAllDiceAssignments();
        }

        public void SpawnTurnDice(int dice1, int dice2)
        {
            ClearTurnDice();

            if (_dicePrefab == null || _diceTray == null || dice1 <= 0 || dice2 <= 0)
            {
                return;
            }

            SpawnDie(dice1, 0);
            SpawnDie(dice2, 1);
        }

        private void ConfigureBattleGrids()
        {
            if (_localDeckGrid != null)
            {
                _localDeckGrid.SetDragCanvas(_dragCanvas);
                _localDeckGrid.SetCardDragEnabled(false);
            }

            if (_opponentDeckGrid != null)
            {
                _opponentDeckGrid.SetDragCanvas(_dragCanvas);
                _opponentDeckGrid.SetCardDragEnabled(false);
            }
        }

        private void EnsureDiceTray()
        {
            if (_diceTray != null)
            {
                return;
            }

            var trayObject = new GameObject("DiceTray", typeof(RectTransform));
            _diceTray = trayObject.GetComponent<RectTransform>();
            _diceTray.SetParent(transform, false);
            _diceTray.anchorMin = new Vector2(0.5f, 0f);
            _diceTray.anchorMax = new Vector2(0.5f, 0f);
            _diceTray.pivot = new Vector2(0.5f, 0f);
            _diceTray.anchoredPosition = new Vector2(0f, 360f);
            _diceTray.sizeDelta = new Vector2(220f, 90f);
        }

        private void SpawnDie(int value, int index)
        {
            var offset = (index - 0.5f) * DiceSpacing;
            var die = Instantiate(_dicePrefab, _diceTray);
            die.Initialize(_dragCanvas, index, new Vector2(offset, 0f));
            die.SetValue(value);
            _spawnedDice.Add(die);
        }

        private void WireLocalCardEvents()
        {
            _localDeckGrid?.ForEachCard(card =>
            {
                card.DiceAssignmentsChanged -= HandleCardDiceAssignmentsChanged;
                card.DiceAssignmentsChanged += HandleCardDiceAssignmentsChanged;
            });
        }

        private void HandleCardDiceAssignmentsChanged(CardView card) => CardDiceAssignmentsChanged?.Invoke(card);
    }
}
