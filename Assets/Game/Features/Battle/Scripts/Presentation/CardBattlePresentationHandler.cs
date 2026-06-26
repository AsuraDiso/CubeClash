using System;
using System.Collections.Generic;
using Game.Features.Deck.Scripts;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Actions;
using Game.Scripts.Core.Battle.Actions.Card;

namespace Game.Features.Battle.Scripts.Presentation
{
    public sealed class CardBattlePresentationHandler : IBattleActionPresentationHandler
    {
        private readonly Dictionary<CardView, Action<CardView>> _assignmentHandlers = new();

        public string TypeId => BattleActionTypes.CardResolve;

        public void Wire(BattlePresentationContext context) =>
            context.View.LocalDeckGrid.ForEachCard(card => WireCard(context, card));

        public void Unwire(BattlePresentationContext context)
        {
            context.View.LocalDeckGrid.ForEachCard(UnwireCard);
            _assignmentHandlers.Clear();
        }

        public void OnActionResolved(BattlePresentationContext context, BattleActionResolvedEventArgs args)
        {
            if (args.Action is not CardBattleAction || args.Result is not CardBattleActionResult cardResult)
                return;

            if (!args.IsLocalActor)
                return;
            context.View.ClearCardDiceAssignments(cardResult.DeckCardIndex);
            if (cardResult.TurnEnded)
                context.View.ClearTurnDice();
        }

        public void OnActionFailed(BattlePresentationContext context)
        {
            context.View.ClearTurnDice();
            context.View.RefreshTurnDiceFromGateway(context.Gateway);
        }

        private void WireCard(BattlePresentationContext context, CardView card)
        {
            Action<CardView> handler = c => TrySubmitCardPlay(context, c);
            _assignmentHandlers[card] = handler;
            card.DiceAssignmentsChanged += handler;
            card.CardClicked += handler;
            card.DiceReleased = context.View.ReleasePooledDie;
            card.ValidateAssignment = dieIndices =>
                context.Gateway.TryValidateAction(new CardBattleAction(card.DeckIndex, dieIndices));
        }

        private void UnwireCard(CardView card)
        {
            if (_assignmentHandlers.TryGetValue(card, out var handler))
            {
                card.DiceAssignmentsChanged -= handler;
                card.CardClicked -= handler;
                _assignmentHandlers.Remove(card);
            }

            card.DiceReleased = null;
            card.ValidateAssignment = null;
        }

        private static void TrySubmitCardPlay(BattlePresentationContext context, CardView card)
        {
            if (!context.CanSubmitAction || !card.AreAllSlotsFilled())
                return;

            if (!card.TryGetAssignedDieIndices(out var dieIndices))
                return;

            var action = new CardBattleAction(card.DeckIndex, dieIndices);
            if (!context.Gateway.TryValidateAction(action))
                return;

            context.ResolveInFlight = true;
            context.SubmitAction(action);
        }
    }
}
