using System;
using System.Collections.Generic;
using Game.Features.Deck.Scripts;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Data.Cards;
using Game.Scripts.Bootstrap.UI.Views;
using Game.Shared.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Features.Battle.Scripts
{
    public sealed class BattleView : ScreenView
    {
        private const int DicePoolDefaultCapacity = 8;
        private const int DicePoolMaxSize = 16;

        [SerializeField] private HealthBarView _localHp;
        [SerializeField] private HealthBarView _opponentHp;
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _opponentName;
        [SerializeField] private CardGridView _localDeckGrid;
        [SerializeField] private CardGridView _opponentDeckGrid;
        [SerializeField] private RectTransform _diceTray;
        [SerializeField] private DiceView _dicePrefab;
        [SerializeField] private Canvas _dragCanvas;

        private readonly List<DiceView> _spawnedDice = new();
        private ObjectPool<DiceView> _dicePool;

        public CardGridView LocalDeckGrid => _localDeckGrid;

        private void Awake()
        {
            _dicePool = new ObjectPool<DiceView>(
                createFunc: () => Instantiate(_dicePrefab, _diceTray),
                actionOnGet: die => die.gameObject.SetActive(true),
                actionOnRelease: ReleaseDieToPool,
                actionOnDestroy: die => Destroy(die.gameObject),
                collectionCheck: true,
                defaultCapacity: DicePoolDefaultCapacity,
                maxSize: DicePoolMaxSize);
        }

        private void ReleaseDieToPool(DiceView die)
        {
            die.ResetForPool();
            die.gameObject.SetActive(false);
        }

        private void OnDestroy() => _dicePool.Dispose();

        public void SetLocalHp(int hp, int maxHp) =>
            _localHp.SetFillAmount(maxHp > 0 ? (float)hp / maxHp : 0f);

        public void SetOpponentHp(int hp, int maxHp) =>
            _opponentHp.SetFillAmount(maxHp > 0 ? (float)hp / maxHp : 0f);

        public void SetTurnText(string text) => _turnText.text = text;

        public void SetOpponentName(string name) => _opponentName.text = name;

        public void SetPlayerName(string name) => _playerName.text = name;

        public void SetLocalDeck(IReadOnlyList<PlacedCard> placedCards)
        {
            _localDeckGrid.SetCardDragEnabled(false);
            _localDeckGrid.SetCards(placedCards);
        }

        public void SetOpponentDeck(IReadOnlyList<PlacedCard> placedCards)
        {
            _opponentDeckGrid.SetCardDragEnabled(false);
            _opponentDeckGrid.SetCards(placedCards);
        }

        public void ClearDiceTray()
        {
            for (var i = _spawnedDice.Count - 1; i >= 0; i--)
                _dicePool.Release(_spawnedDice[i]);

            _spawnedDice.Clear();
        }

        public void ClearTurnDice()
        {
            _localDeckGrid.ClearAllDiceAssignments();
            ClearDiceTray();
        }

        public void ClearCardDiceAssignments(int deckCardIndex) =>
            _localDeckGrid.ClearCardDiceAssignments(deckCardIndex);

        public void ReleasePooledDie(DiceView die)
        {
            _spawnedDice.Remove(die);
            _dicePool.Release(die);
        }

        public void RefreshTurnDiceFromGateway(IBattleGateway gateway)
        {
            if (gateway == null)
                return;

            RefreshDiceTray(
                gateway.DiceCount,
                gateway.GetTurnDiceValue,
                gateway.IsTurnDiceConsumed,
                gateway.DiceMin,
                gateway.DiceMax,
                interactable: gateway.IsMyTurn);
        }

        public void RefreshDiceTray(int diceCount, Func<int, int> getValue, Func<int, bool> isConsumed,
            int diceMin = 1, int diceMax = 6, bool interactable = true)
        {
            if (interactable)
                _localDeckGrid.ReleaseAssignedConsumedDice(isConsumed, ReleasePooledDie);

            for (var i = _spawnedDice.Count - 1; i >= 0; i--)
            {
                var die = _spawnedDice[i];
                if (die.IsAssigned)
                    continue;

                if (isConsumed(die.DiceIndex) || getValue(die.DiceIndex) <= 0)
                {
                    _spawnedDice.RemoveAt(i);
                    _dicePool.Release(die);
                }
            }

            var assignedIndices = interactable ? _localDeckGrid.CollectAssignedDieIndices() : null;

            for (var index = 0; index < diceCount; index++)
            {
                if (isConsumed(index) || getValue(index) <= 0 || assignedIndices?.Contains(index) == true)
                    continue;

                if (TryFindUnassignedTrayDie(index, out var existing))
                {
                    existing.SetValue(getValue(index));
                    existing.SetInteractable(interactable);
                    continue;
                }

                SpawnDie(getValue(index), index, diceMin, diceMax, interactable);
            }
        }

        private bool TryFindUnassignedTrayDie(int diceIndex, out DiceView die)
        {
            foreach (var spawned in _spawnedDice)
            {
                if (!spawned.IsAssigned && spawned.DiceIndex == diceIndex)
                {
                    die = spawned;
                    return true;
                }
            }

            die = null;
            return false;
        }

        private void SpawnDie(int value, int index, int diceMin, int diceMax, bool interactable)
        {
            var die = _dicePool.Get();
            die.Bind(_dragCanvas, index, _diceTray, diceMin, diceMax);
            die.SetValue(value);
            die.SetInteractable(interactable);
            _spawnedDice.Add(die);
        }
    }
}
