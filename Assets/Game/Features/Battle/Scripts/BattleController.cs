using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Features.Battle.Scripts.Presentation;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Simulation;
using Game.Scripts.Core.Matchmaking;
using Game.Scripts.Core.Scenes;
using UnityEngine;

namespace Game.Features.Battle.Scripts
{
    public sealed class BattleController : IDisposable
    {
        private readonly BattleView _view;
        private readonly IBattleGateway _gateway;
        private readonly BattlePresentationRouter _presentation;
        private readonly BattlePresentationContext _context;
        private readonly BattleMatchIntroPresenter _matchIntroPresenter;
        private readonly IMatchmakingService _matchmakingService;
        private readonly ISceneLoaderService _sceneLoaderService;
        private bool _wasMyTurn;
        private bool _isReturningToMenu;
        private bool _introStarted;

        public BattleController(BattleView view, IBattleControllerRegistry registry,
            BattlePresentationRouter presentation, BattleMatchIntroPresenter matchIntroPresenter,
            IMatchmakingService matchmakingService, ISceneLoaderService sceneLoaderService)
        {
            _view = view;
            _gateway = registry.Current;
            _presentation = presentation;
            _matchIntroPresenter = matchIntroPresenter;
            _matchmakingService = matchmakingService;
            _sceneLoaderService = sceneLoaderService;
            _context = new BattlePresentationContext(_view, _gateway);

            _gateway.TurnChanged += HandleTurnChanged;
            _gateway.ProfilesUpdated += HandleProfilesUpdated;
            _gateway.DecksUpdated += HandleDecksUpdated;
            _gateway.GameOver += HandleGameOver;
            _gateway.ActionResolved += HandleActionResolved;
            _gateway.ActionFailed += HandleActionFailed;

            RefreshProfiles();
            RefreshDecks();
            HandleTurnChanged();
            TryStartMatchIntro();
        }

        public void Dispose()
        {
            _presentation.UnwireAll(_context);
            _gateway.TurnChanged -= HandleTurnChanged;
            _gateway.ProfilesUpdated -= HandleProfilesUpdated;
            _gateway.DecksUpdated -= HandleDecksUpdated;
            _gateway.GameOver -= HandleGameOver;
            _gateway.ActionResolved -= HandleActionResolved;
            _gateway.ActionFailed -= HandleActionFailed;
        }

        private void HandleTurnChanged()
        {
            _context.ResolveInFlight = false;

            var isMyTurn = !_context.IsGameOver && _gateway.IsMatchReady && _gateway.IsMyTurn;
            if (_wasMyTurn != isMyTurn)
            {
                _view.ClearTurnDice();
                _wasMyTurn = isMyTurn;
            }

            RefreshTurnPresentation();
        }

        private void HandleActionResolved(BattleActionResolvedEventArgs args)
        {
            _context.ResolveInFlight = false;
            _presentation.OnActionResolved(_context, args);
            RefreshTurnPresentation();
        }

        private void HandleActionFailed()
        {
            _context.ResolveInFlight = false;
            _presentation.OnActionFailed(_context);
        }

        private void HandleProfilesUpdated()
        {
            RefreshProfiles();

            if (_gateway.IsMatchReady)
                TryStartMatchIntro();
        }

        private void HandleDecksUpdated() => RefreshDecks();

        private void RefreshTurnPresentation()
        {
            RefreshProfiles();
            RefreshTurnDice();
            RefreshTurnText();
        }

        private void RefreshTurnDice()
        {
            if (_context.IsGameOver || !_gateway.IsMatchReady)
            {
                _view.ClearTurnDice();
                return;
            }

            _view.RefreshTurnDiceFromGateway(_gateway, _context.CanSubmitAction);
        }

        private void RefreshTurnText()
        {
            if (_context.IsGameOver)
                return;

            if (!_gateway.IsMatchReady)
            {
                _view.SetTurnText("Waiting for opponent...");
                return;
            }

            if (!_context.IntroComplete)
            {
                _view.SetTurnText(string.Empty);
                return;
            }

            _view.SetTurnText(_gateway.IsMyTurn ? "Your Turn" : "Opponent's Turn");
        }

        private void TryStartMatchIntro()
        {
            if (_introStarted || _context.IntroComplete || _view.MatchIntro == null || !_gateway.IsMatchReady)
            {
                if (_view.MatchIntro == null)
                    _context.IntroComplete = true;

                return;
            }

            _introStarted = true;
            PlayMatchIntroAsync().Forget(Debug.LogException);
        }

        private async UniTask PlayMatchIntroAsync()
        {
            RefreshTurnText();

            try
            {
                await _matchIntroPresenter.PlayAsync(_view.MatchIntro, _gateway, CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _context.IntroComplete = true;
            RefreshTurnPresentation();
        }

        private void RefreshProfiles()
        {
            _view.SetLocalHp(_gateway.LocalProfile.Hp, _gateway.MaxHp);
            _view.SetOpponentHp(_gateway.OpponentProfile.Hp, _gateway.MaxHp);
            _view.SetPlayerName(_gateway.LocalProfile.DisplayName);
            _view.SetOpponentName(_gateway.OpponentProfile.DisplayName);
        }

        private void RefreshDecks()
        {
            _view.ClearTurnDice();
            _presentation.UnwireAll(_context);
            _view.SetLocalDeck(_gateway.LocalDeck);
            _view.SetOpponentDeck(_gateway.OpponentDeck);
            _presentation.WireAll(_context);
            RefreshTurnDice();
        }

        private void HandleGameOver(bool localPlayerWon)
        {
            _context.IsGameOver = true;
            _context.ResolveInFlight = false;
            _view.ClearTurnDice();

            var resultText = _gateway.WinnerNetworkId == BattleState.UnassignedNetworkId ? "Draw!" :
                localPlayerWon ? "You Win!" : "You Lose!";

            _view.SetTurnText($"Game Over — {resultText}");
            ReturnToMainMenuAsync().Forget(Debug.LogException);
        }

        private async UniTask ReturnToMainMenuAsync()
        {
            if (_isReturningToMenu)
                return;

            _isReturningToMenu = true;

            await _matchmakingService.ExitMatchAsync();
            await _sceneLoaderService.LoadSceneAsync(GameSceneId.MainMenu);
        }
    }
}
