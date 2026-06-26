using System;
using System.Collections.Generic;
using Fusion;
using Game.Scripts.Core.Data.Cards;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Simulation;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Networking;

namespace Game.Scripts.Infrastructure.Battle.Network
{
    internal sealed class BattleMatchInitializer
    {
        private readonly Dictionary<int, int[]> _pendingDeckPacks = new();

        public void SubmitDeck(int playerId, int[] packedDeck) =>
            _pendingDeckPacks[playerId] = packedDeck ?? Array.Empty<int>();

        public bool TryInitialize(NetworkRunner runner, BattleModeConfig modeConfig, CardCatalog catalog,
            BattleEngine engine, bool isAlreadyInitialized, Action<BattleState> applyState, Action broadcastDecks)
        {
            if (isAlreadyInitialized)
                return false;

            var players = new PlayerRef[BattleState.MaxPlayers];
            var playerCount = 0;
            foreach (var p in runner.ActivePlayers)
            {
                players[playerCount++] = p;
                if (playerCount == BattleState.MaxPlayers)
                    break;
            }

            if (playerCount < BattleState.MaxPlayers)
                return false;

            for (var i = 0; i < BattleState.MaxPlayers; i++)
            {
                if (!_pendingDeckPacks.ContainsKey(players[i].PlayerId))
                    return false;
            }

            var networkIds = new int[BattleState.MaxPlayers];
            var profiles = new PlayerProfile[BattleState.MaxPlayers];
            var playerDecks = new IReadOnlyList<PlacedCard>[BattleState.MaxPlayers];

            for (var i = 0; i < BattleState.MaxPlayers; i++)
            {
                networkIds[i] = players[i].PlayerId;
                profiles[i] = ResolveProfile(runner, players[i], modeConfig.MaxHp);
                playerDecks[i] = ResolveDeck(players[i], catalog);
            }

            engine.Initialize(networkIds, profiles, playerDecks);
            applyState(engine.State);
            broadcastDecks();
            _pendingDeckPacks.Clear();
            return true;
        }

        private IReadOnlyList<PlacedCard> ResolveDeck(PlayerRef player, CardCatalog catalog)
        {
            var packedDeck = _pendingDeckPacks[player.PlayerId];
            return DeckRecordConverter.ToPlacedCardsFromNetworkPack(packedDeck, catalog);
        }

        private static PlayerProfile ResolveProfile(NetworkRunner runner, PlayerRef player, int maxHp)
        {
            var tokenProfile = ProfileConnectionToken.TryDecode(runner.GetPlayerConnectionToken(player));
            return tokenProfile ?? PlayerProfile.CreateBattleDefault(maxHp: maxHp);
        }
    }
}
