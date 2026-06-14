using System;
using System.Linq;
using Core.Battle;
using Fusion;
using UnityEngine;

namespace Infrastructure.Photon.Battle
{
    public sealed class BattleNetworkController : NetworkBehaviour, IBattleAttackGateway
    {
        [Networked] public int CurrentTurnPlayerId { get; private set; }
        [Networked] public int Player1Id { get; private set; }
        [Networked] public int Player1Hp { get; private set; }
        [Networked] public int Player2Id { get; private set; }
        [Networked] public int Player2Hp { get; private set; }

        public event Action TurnChanged;
        public event Action<int, int> HealthUpdated;
        public event Action<bool> GameOver;
        public event Action<int, string> AttackReceived;

        private IBattleControllerRegistry _controllerRegistry;
        private ChangeDetector _changeDetector;

        public bool IsMyTurn => CurrentTurnPlayerId == Runner.LocalPlayer.PlayerId;
        public int LocalPlayerHp => Runner.LocalPlayer.PlayerId == Player1Id ? Player1Hp : Player2Hp;
        public int OpponentHp => Runner.LocalPlayer.PlayerId == Player1Id ? Player2Hp : Player1Hp;

        public override void Spawned()
        {
            var bridge = Runner.GetComponent<FusionSessionBridge>();
            _controllerRegistry = bridge?.BattleControllerRegistry;
            _controllerRegistry?.Register(this);

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            if (HasStateAuthority)
            {
                var players = Runner.ActivePlayers.ToArray();
                if (players.Length >= 2)
                {
                    Player1Id = players[0].PlayerId;
                    Player2Id = players[1].PlayerId;
                    Player1Hp = BattleConstants.MaxHp;
                    Player2Hp = BattleConstants.MaxHp;
                    CurrentTurnPlayerId = Player1Id;
                }
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _controllerRegistry?.Unregister(this);
            _controllerRegistry = null;
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(CurrentTurnPlayerId):
                        TurnChanged?.Invoke();
                        break;
                    case nameof(Player1Hp):
                    case nameof(Player2Hp):
                        HealthUpdated?.Invoke(LocalPlayerHp, OpponentHp);
                        if (Player1Hp <= 0 || Player2Hp <= 0)
                        {
                            var winnerId = Player1Hp <= 0 ? Player2Id : Player1Id;
                            GameOver?.Invoke(Runner.LocalPlayer.PlayerId == winnerId);
                        }
                        break;
                }
            }
        }

        public void SendAttack()
        {
            if (!Object || !Object.IsValid || !IsMyTurn)
            {
                return;
            }

            RPC_RequestAttack(Runner.LocalPlayer);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestAttack(PlayerRef attacker)
        {
            if (attacker.PlayerId != CurrentTurnPlayerId)
            {
                return;
            }

            if (attacker.PlayerId == Player1Id)
            {
                Player2Hp -= BattleConstants.AttackDamage;
            }
            else
            {
                Player1Hp -= BattleConstants.AttackDamage;
            }

            var attackerLabel = $"Player {attacker.PlayerId}";
            RPC_BroadcastAttack(attacker.PlayerId, attackerLabel);

            if (Player1Hp > 0 && Player2Hp > 0)
            {
                CurrentTurnPlayerId = attacker.PlayerId == Player1Id ? Player2Id : Player1Id;
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastAttack(int attackerId, string attackerLabel)
        {
            AttackReceived?.Invoke(attackerId, attackerLabel);
        }
    }
}
