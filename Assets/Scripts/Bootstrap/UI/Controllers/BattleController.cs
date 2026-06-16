using System;
using Bootstrap.UI;
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
            _view.AttackClicked -= OnAttack;
            _viewFactory.Destroy(_view);
            _view = null;

            UnbindGateway();

            _controllerRegistry.GatewayAvailable -= OnGatewayAvailable;
        }

        private void OnGatewayAvailable(IBattleAttackGateway gateway)
        {
            BindGateway(gateway);
        }

        private void BindGateway(IBattleAttackGateway gateway)
        {
            if (_gateway == gateway)
            {
                return;
            }

            UnbindGateway();
            _gateway = gateway;
            _gateway.TurnChanged += OnTurnChanged;
            _gateway.HealthUpdated += OnHealthUpdated;
            _gateway.GameOver += OnGameOver;

            _view.SetLocalHp(_gateway.LocalPlayerHp);
            _view.SetOpponentHp(_gateway.OpponentHp);
            OnTurnChanged();
        }

        private void UnbindGateway()
        {
            if (_gateway == null)
            {
                return;
            }

            _gateway.TurnChanged -= OnTurnChanged;
            _gateway.HealthUpdated -= OnHealthUpdated;
            _gateway.GameOver -= OnGameOver;
            _gateway = null;
            RefreshAttackButton();
        }

        private void OnTurnChanged()
        {
            if (_gateway != null && !_isGameOver)
            {
                _view.SetTurnText(_gateway.IsMyTurn ? "Your Turn" : "Opponent's Turn");
            }

            RefreshAttackButton();
        }

        private void OnHealthUpdated(int localHp, int opponentHp)
        {
            _view.SetLocalHp(localHp);
            _view.SetOpponentHp(opponentHp);
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
                Debug.LogWarning("[CubeClash] Battle gateway is not available yet.");
                return;
            }

            _gateway.SendAttack();
        }

        private void RefreshAttackButton()
        {
            _view.SetAttackEnabled(_gateway != null && _gateway.IsMyTurn && !_isGameOver);
        }
    }
}
