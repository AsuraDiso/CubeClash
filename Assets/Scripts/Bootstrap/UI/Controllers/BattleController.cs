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
            _view.AttackClicked += OnAttack;
            _controllerRegistry.GatewayAvailable += OnGatewayAvailable;

            if (_controllerRegistry.Current != null)
            {
                BindGateway(_controllerRegistry.Current);
            }

            _view.SetStatusText($"Connected! Players: {_networkSession.PlayerCount}");
            RefreshAttackButton();
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.AttackClicked -= OnAttack;
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
            {
                return;
            }

            RefreshProfiles();
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
            gateway.GameOver += OnGameOver;
        }

        private void UnsubscribeGateway(IBattleAttackGateway gateway)
        {
            gateway.TurnChanged -= OnTurnChanged;
            gateway.ProfilesUpdated -= OnProfilesUpdated;
            gateway.GameOver -= OnGameOver;
        }

        private void OnTurnChanged()
        {
            if (_gateway != null && !_isGameOver)
            {
                _view.SetTurnText(_gateway.IsMyTurn ? "Your Turn" : "Opponent's Turn");
            }

            RefreshAttackButton();
        }

        private void OnProfilesUpdated() => RefreshProfiles();

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

        private void OnGameOver(bool localPlayerWon)
        {
            _isGameOver = true;
            _view.SetTurnText("Game Over");
            _view.SetStatusText(localPlayerWon ? "You Win!" : "You Lose!");
            RefreshAttackButton();
        }

        private void OnAttack()
        {
            if (_gateway == null)
            {
                Debug.LogWarning("Battle gateway not ready.");
                return;
            }

            _gateway.SendAttack();
        }

        private void RefreshAttackButton() =>
            _view?.SetAttackEnabled(_gateway != null && _gateway.IsMyTurn && !_isGameOver);
    }
}
