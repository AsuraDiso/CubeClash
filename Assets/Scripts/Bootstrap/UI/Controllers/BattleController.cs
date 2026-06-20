using System;
using Bootstrap.Common;
using Bootstrap.UI.Views;
using Core.Battle;
using Core.Networking;
using UnityEngine;
using VContainer.Unity;

namespace Bootstrap.UI.Controllers
{
    public sealed class BattleController : IStartable, IDisposable
    {
        private readonly IUiViewFactory _viewFactory;
        private readonly IBattleControllerRegistry _controllerRegistry;
        private readonly INetworkSession _networkSession;

        private BattleView _view;
        private IBattleAttackGateway _gateway;
        private bool _isGameOver;

        public BattleController(
            IUiViewFactory viewFactory,
            IBattleControllerRegistry controllerRegistry,
            INetworkSession networkSession)
        {
            _viewFactory = viewFactory;
            _controllerRegistry = controllerRegistry;
            _networkSession = networkSession;
        }

        public void Start()
        {
            _view = _viewFactory.CreateBattleView();
            _view.CardDiceAssignmentsChanged += OnCardDiceAssignmentsChanged;
            _controllerRegistry.GatewayAvailable += OnGatewayAvailable;

            if (_controllerRegistry.Current != null)
                BindGateway(_controllerRegistry.Current);

            _view.SetStatusText($"Connected! Players: {_networkSession.PlayerCount}");
            RefreshAttackButton();
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.CardDiceAssignmentsChanged -= OnCardDiceAssignmentsChanged;
                _viewFactory.Destroy(_view);
                _view = null;
            }

            UnbindGateway();
            _controllerRegistry.GatewayAvailable -= OnGatewayAvailable;
        }

        private void OnGatewayAvailable(IBattleAttackGateway gateway) => BindGateway(gateway);

        private void BindGateway(IBattleAttackGateway gateway)
        {
            ViewBinding.Switch(ref _gateway, gateway, SubscribeGateway, UnsubscribeGateway);
            if (_gateway == null)
                return;

            RefreshProfiles();
            RefreshDecks();
            OnTurnChanged();
        }

        private void UnbindGateway()
        {
            ViewBinding.Unbind(ref _gateway, UnsubscribeGateway);
            RefreshAttackButton();
        }

        private void SubscribeGateway(IBattleAttackGateway gateway)
        {
            gateway.TurnChanged += OnTurnChanged;
            gateway.ProfilesUpdated += OnProfilesUpdated;
            gateway.DecksUpdated += OnDecksUpdated;
            gateway.GameOver += OnGameOver;
        }

        private void UnsubscribeGateway(IBattleAttackGateway gateway)
        {
            gateway.TurnChanged -= OnTurnChanged;
            gateway.ProfilesUpdated -= OnProfilesUpdated;
            gateway.DecksUpdated -= OnDecksUpdated;
            gateway.GameOver -= OnGameOver;
        }

        private void OnTurnChanged()
        {
            RefreshTurnDice();
            RefreshAttackButton();
        }

        private void RefreshTurnDice()
        {
            if (_view == null || _gateway == null || _isGameOver)
            {
                _view?.ClearTurnDice();
                return;
            }

            if (!_gateway.IsMyTurn)
            {
                _view.ClearTurnDice();
                _view.SetTurnText("Opponent's Turn");
                return;
            }

            _view.SpawnTurnDice(_gateway.TurnDice1, _gateway.TurnDice2);
            _view.SetTurnText("Your Turn · Drag dice onto card slots");
        }

        private void OnProfilesUpdated() => RefreshProfiles();

        private void OnDecksUpdated() => RefreshDecks();

        private void OnCardDiceAssignmentsChanged(CardView card)
        {
            if (card == null || !card.AreAllSlotsFilled())
            {
                return;
            }

            _view.SetStatusText($"Card ready: {card.Definition.DisplayName}");
        }

        private void RefreshProfiles()
        {
            if (_gateway == null)
            {
                return;
            }

            _view.SetLocalHp(_gateway.LocalProfile.Hp);
            _view.SetOpponentHp(_gateway.OpponentProfile.Hp);
            _view.SetPlayerName(_gateway.LocalProfile.DisplayName);
            _view.SetOpponentName(_gateway.OpponentProfile.DisplayName);
        }

        private void RefreshDecks()
        {
            if (_gateway == null || _view == null)
            {
                return;
            }

            _view.SetLocalDeck(_gateway.LocalDeck);
            _view.SetOpponentDeck(_gateway.OpponentDeck);
            RefreshTurnDice();
        }

        private void OnGameOver(bool localPlayerWon)
        {
            _isGameOver = true;
            _view.ClearTurnDice();
            _view.SetTurnText("Game Over");
            _view.SetStatusText(localPlayerWon ? "You Win!" : "You Lose!");
            RefreshAttackButton();
        }

        private void RefreshAttackButton() => _view?.SetAttackEnabled(_gateway is { IsMyTurn: true } && !_isGameOver);
    }
}


