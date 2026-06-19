using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Core.Battle;
using Core.Data;
using Core.Networking;
using Fusion;
using Infrastructure.Photon;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Infrastructure.Photon.Battle
{
    public sealed class BattleNetworkController : NetworkBehaviour, IBattleAttackGateway
    {
        [Networked] public int CurrentTurnPlayerId { get; private set; }
        [Networked] public int TurnDice1 { get; private set; }
        [Networked] public int TurnDice2 { get; private set; }
        [Networked] public int Player1NetworkId { get; private set; }
        [Networked] public int Player2NetworkId { get; private set; }
        [Networked] public NetworkPlayerProfile Player1Profile { get; private set; }
        [Networked] public NetworkPlayerProfile Player2Profile { get; private set; }

        public event Action TurnChanged;
        public event Action ProfilesUpdated;
        public event Action DecksUpdated;
        public event Action<bool> GameOver;
        public event Action<int, string> AttackReceived;

        private readonly Dictionary<int, PlayerProfile> _pendingProfiles = new();
        private readonly Dictionary<int, int[]> _pendingDeckPacks = new();

        private IBattleControllerRegistry _controllerRegistry;
        private FusionSessionBridge _bridge;
        private ChangeDetector _changeDetector;
        private List<PlacedCard> _localDeck = new();
        private List<PlacedCard> _opponentDeck = new();

        public bool IsMyTurn => CurrentTurnPlayerId == Runner.LocalPlayer.PlayerId;
        public PlayerProfile LocalProfile => GetProfileForNetworkId(Runner.LocalPlayer.PlayerId);
        public PlayerProfile OpponentProfile => GetProfileForNetworkId(GetOpponentNetworkId());
        public IReadOnlyList<PlacedCard> LocalDeck => _localDeck;
        public IReadOnlyList<PlacedCard> OpponentDeck => _opponentDeck;

        public override void Spawned()
        {
            _bridge = Runner.GetComponent<FusionSessionBridge>();
            _controllerRegistry = _bridge?.BattleControllerRegistry;
            _controllerRegistry?.Register(this);
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            var payload = _bridge?.Payload;
            var profile = payload?.LocalProfile ?? PlayerProfile.CreateBattleDefault();
            RPC_SubmitProfile(Runner.LocalPlayer, profile.PlayerId, profile.DisplayName);

            var localDeck = payload?.LocalDeck ?? Array.Empty<PlacedCard>();
            _localDeck = new List<PlacedCard>(localDeck);
            DecksUpdated?.Invoke();
            RPC_SubmitDeck(Runner.LocalPlayer, DeckRecordConverter.PackNetwork(DeckRecordConverter.FromPlacedCards(localDeck)));

            if (HasStateAuthority)
            {
                TryInitializeMatch();
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
                    case nameof(TurnDice1):
                    case nameof(TurnDice2):
                        TurnChanged?.Invoke();
                        break;
                    case nameof(Player1Profile):
                    case nameof(Player2Profile):
                        ProfilesUpdated?.Invoke();
                        CheckGameOver();
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
        private void RPC_SubmitProfile(PlayerRef player, string playerId, string displayName)
        {
            var profile = PlayerProfile.CreateBattleDefault(playerId, displayName);
            _pendingProfiles[player.PlayerId] = profile;
            ApplyProfile(player.PlayerId, profile);
            TryInitializeMatch();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_SubmitDeck(PlayerRef player, int[] packedDeck)
        {
            _pendingDeckPacks[player.PlayerId] = packedDeck ?? Array.Empty<int>();
            TryInitializeMatch();
            BroadcastDecks();
        }

        private void BroadcastDecks()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            foreach (var player in Runner.ActivePlayers)
            {
                if (_pendingDeckPacks.TryGetValue(player.PlayerId, out var packedDeck))
                {
                    RPC_ApplyPlayerDeck(player.PlayerId, packedDeck);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ApplyPlayerDeck(int networkPlayerId, int[] packedDeck)
        {
            var catalog = _bridge?.Payload?.CardCatalog;
            if (catalog == null)
            {
                return;
            }

            var deck = DeckRecordConverter.ToPlacedCards(DeckRecordConverter.UnpackNetwork(packedDeck), catalog);
            if (networkPlayerId == Runner.LocalPlayer.PlayerId)
            {
                _localDeck = deck;
            }
            else
            {
                _opponentDeck = deck;
            }

            DecksUpdated?.Invoke();
        }

        private void TryInitializeMatch()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            var players = Runner.ActivePlayers.ToArray();
            if (players.Length < 2 || Player1NetworkId != 0)
            {
                return;
            }

            Player1NetworkId = players[0].PlayerId;
            Player2NetworkId = players[1].PlayerId;
            CurrentTurnPlayerId = Player1NetworkId;
            RollTurnDice();
            Player1Profile = NetworkPlayerProfile.From(ResolveProfile(players[0]));
            Player2Profile = NetworkPlayerProfile.From(ResolveProfile(players[1]));
            BroadcastDecks();
        }

        private PlayerProfile ResolveProfile(PlayerRef player)
        {
            if (_pendingProfiles.TryGetValue(player.PlayerId, out var pendingProfile))
            {
                return pendingProfile;
            }

            var tokenProfile = ProfileConnectionToken.TryDecode(Runner.GetPlayerConnectionToken(player));
            return tokenProfile ?? PlayerProfile.CreateBattleDefault();
        }

        private void ApplyProfile(int networkPlayerId, PlayerProfile profile)
        {
            if (networkPlayerId == Player1NetworkId)
            {
                Player1Profile = NetworkPlayerProfile.From(profile);
            }
            else if (networkPlayerId == Player2NetworkId)
            {
                Player2Profile = NetworkPlayerProfile.From(profile);
            }
        }

        private int GetOpponentNetworkId() =>
            Runner.LocalPlayer.PlayerId == Player1NetworkId ? Player2NetworkId : Player1NetworkId;

        private PlayerProfile GetProfileForNetworkId(int networkPlayerId)
        {
            if (networkPlayerId == Player1NetworkId)
            {
                return Player1Profile.ToProfile();
            }

            if (networkPlayerId == Player2NetworkId)
            {
                return Player2Profile.ToProfile();
            }

            return PlayerProfile.Unknown;
        }

        private void CheckGameOver()
        {
            var player1 = Player1Profile.ToProfile();
            var player2 = Player2Profile.ToProfile();

            if (player1.Hp > 0 && player2.Hp > 0)
            {
                return;
            }

            var winnerId = player1.Hp <= 0 ? Player2NetworkId : Player1NetworkId;
            GameOver?.Invoke(Runner.LocalPlayer.PlayerId == winnerId);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestAttack(PlayerRef attacker)
        {
            if (attacker.PlayerId != CurrentTurnPlayerId)
            {
                return;
            }

            ApplyDamageToOpponent(attacker, BattleConstants.AttackDamage);

            var attackerProfile = GetProfileForNetworkId(attacker.PlayerId);
            RPC_BroadcastAttack(attacker.PlayerId, attackerProfile.DisplayName);

            var player1 = Player1Profile.ToProfile();
            var player2 = Player2Profile.ToProfile();
            if (player1.Hp > 0 && player2.Hp > 0)
            {
                CurrentTurnPlayerId = attacker.PlayerId == Player1NetworkId ? Player2NetworkId : Player1NetworkId;
                RollTurnDice();
            }
        }

        private void RollTurnDice()
        {
            TurnDice1 = Random.Range(BattleConstants.DiceMin, BattleConstants.DiceMax + 1);
            TurnDice2 = Random.Range(BattleConstants.DiceMin, BattleConstants.DiceMax + 1);
        }

        private void ApplyDamageToOpponent(PlayerRef attacker, int damage)
        {
            if (attacker.PlayerId == Player1NetworkId)
            {
                var profile = Player2Profile.ToProfile();
                Player2Profile = NetworkPlayerProfile.From(profile.WithHp(profile.Hp - damage));
            }
            else
            {
                var profile = Player1Profile.ToProfile();
                Player1Profile = NetworkPlayerProfile.From(profile.WithHp(profile.Hp - damage));
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastAttack(int attackerId, string attackerLabel)
        {
            AttackReceived?.Invoke(attackerId, attackerLabel);
        }
    }
}
