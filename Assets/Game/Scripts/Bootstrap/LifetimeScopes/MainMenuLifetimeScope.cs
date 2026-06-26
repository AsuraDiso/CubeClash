using Game.Features.Deck.Scripts;
using Game.Features.Home.Scripts;
using Game.Features.Matchmaking.Scripts;
using Game.Features.Settings.Scripts;
using Game.Scripts.Bootstrap.EntryPoints;
using Game.Scripts.Bootstrap.Navigation;
using Game.Scripts.Bootstrap.UI.Views;
using Game.Scripts.Core.Audio;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.LifetimeScopes
{
    public sealed class MainMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] private HomeView _homeViewPrefab;
        [SerializeField] private SettingsView _settingsViewPrefab;
        [SerializeField] private DeckView _deckViewPrefab;
        [SerializeField] private MatchmakingView _matchmakingViewPrefab;

        private HomeView _homeView;
        private SettingsView _settingsView;
        private DeckView _deckView;
        private MatchmakingView _matchmakingView;

        protected override void Awake()
        {
            InstantiateViews();
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterViews(builder);
            RegisterControllers(builder);

            var screens = new ScreenRegistry();
            screens.Register<HomeController>(_homeView);
            screens.Register<DeckController>(_deckView);
            screens.Register<SettingsController>(_settingsView);
            screens.Register<MatchmakingController>(_matchmakingView);
            builder.RegisterInstance(screens);

            builder.Register<ScreenNavigator>(Lifetime.Scoped);
            builder.RegisterInstance(MusicId.MainMenu);
            builder.RegisterEntryPoint<SceneMusicEntryPoint>();
            builder.RegisterEntryPoint<MainMenuSceneEntryPoint>(Lifetime.Scoped);
        }

        private void InstantiateViews()
        {
            if (_homeView != null)
                return;

            RequirePrefab(_homeViewPrefab, nameof(_homeViewPrefab));
            RequirePrefab(_settingsViewPrefab, nameof(_settingsViewPrefab));
            RequirePrefab(_deckViewPrefab, nameof(_deckViewPrefab));
            RequirePrefab(_matchmakingViewPrefab, nameof(_matchmakingViewPrefab));

            var uiRoot = transform;
            _homeView = Instantiate(_homeViewPrefab, uiRoot);
            _settingsView = Instantiate(_settingsViewPrefab, uiRoot);
            _deckView = Instantiate(_deckViewPrefab, uiRoot);
            _matchmakingView = Instantiate(_matchmakingViewPrefab, uiRoot);

            foreach (var view in new ScreenView[] { _homeView, _settingsView, _deckView, _matchmakingView })
                view.gameObject.SetActive(false);
        }

        private void RegisterViews(IContainerBuilder builder)
        {
            builder.RegisterComponent(_homeView);
            builder.RegisterComponent(_settingsView);
            builder.RegisterComponent(_deckView);
            builder.RegisterComponent(_matchmakingView);
            builder.RegisterBuildCallback(InjectViews);
        }

        private void InjectViews(IObjectResolver resolver)
        {
            resolver.InjectGameObject(_homeView.gameObject);
            resolver.InjectGameObject(_settingsView.gameObject);
            resolver.InjectGameObject(_deckView.gameObject);
            resolver.InjectGameObject(_matchmakingView.gameObject);
        }

        private static void RegisterControllers(IContainerBuilder builder)
        {
            builder.Register<HomeController>(Lifetime.Scoped);
            builder.Register<DeckController>(Lifetime.Scoped);
            builder.Register<SettingsController>(Lifetime.Scoped);
            builder.Register<MatchmakingController>(Lifetime.Scoped);
        }

        private static void RequirePrefab(Object prefab, string fieldName)
        {
            if (prefab == null)
                throw new System.InvalidOperationException($"MainMenuLifetimeScope.{fieldName} is not assigned.");
        }

    }
}
