using System;
using System.Threading;
using Core.Battle;
using Core.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Data.Mock
{
    public sealed class MockGameDatabase : IGameDatabase
    {
        private const int SimulatedLoadDelayMs = 750;

        private PlayerProfile _player;
        private GameBalanceData _balance;

        public bool IsLoaded { get; private set; }

        public PlayerProfile Player
        {
            get
            {
                EnsureLoaded();
                return _player;
            }
        }

        public GameBalanceData Balance
        {
            get
            {
                EnsureLoaded();
                return _balance;
            }
        }

        public async UniTask LoadAsync(CancellationToken cancellationToken = default)
        {
            if (IsLoaded)
            {
                return;
            }

            await UniTask.Delay(SimulatedLoadDelayMs, cancellationToken: cancellationToken);

            _player = new PlayerProfile(
                playerId: "mock-player-001",
                displayName: "Cube Fighter");

            _balance = new GameBalanceData(
                maxHp: BattleConstants.MaxHp,
                attackDamage: BattleConstants.AttackDamage);

            IsLoaded = true;
            Debug.Log($"[CubeClash] Mock database loaded for {_player.DisplayName}.");
        }

        private void EnsureLoaded()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException("Game database has not been loaded yet.");
            }
        }
    }
}
