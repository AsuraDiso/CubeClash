using System;
using System.Collections.Generic;
using Game.Scripts.Core.Battle.Actions;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Data.Cards;

namespace Game.Scripts.Core.Battle
{
    public interface IBattleGateway
    {
        int BattleModeId { get; }
        int MaxHp { get; }
        int DiceCount { get; }
        int DiceMin { get; }
        int DiceMax { get; }
        int WinnerNetworkId { get; }
        bool IsMyTurn { get; }
        bool IsMatchReady { get; }
        PlayerProfile LocalProfile { get; }
        PlayerProfile OpponentProfile { get; }
        IReadOnlyList<PlacedCard> LocalDeck { get; }
        IReadOnlyList<PlacedCard> OpponentDeck { get; }

        int GetTurnDiceValue(int dieIndex);
        bool IsTurnDiceConsumed(int dieIndex);

        bool TryValidateAction(IBattleAction action);
        void SubmitAction(IBattleAction action);

        event Action TurnChanged;
        event Action ProfilesUpdated;
        event Action DecksUpdated;
        event Action<bool> GameOver;
        event Action<BattleActionResolvedEventArgs> ActionResolved;
        event Action ActionFailed;
    }
}
