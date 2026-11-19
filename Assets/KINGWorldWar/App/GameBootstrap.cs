using System;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.Networking;
using KingWorldWar.Core.Quality;
using KingWorldWar.Core.Scenes;
using KingWorldWar.Core.UI;
using KingWorldWar.Core.WorldMap;
using KingWorldWar.Infrastructure.Config;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.Quality;
using KingWorldWar.Infrastructure.Scenes;
using KingWorldWar.Infrastructure.UI;
using KingWorldWar.Infrastructure.WorldMap;
using KingWorldWar.UI.ErrorScreen;
using UnityEngine;

namespace KingWorldWar.App
{
    /// <summary>
    /// Deterministic composition root and startup orchestrator for KING WORLD WAR.
    /// Runs in Bootstrap scene (Index 0).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameConfig _config;

        [Header("UI Fallback")]
        [SerializeField] private ErrorScreenView _errorScreen;

        [Header("Testing Overrides")]
        [SerializeField] private bool _autoRunOnStart = true;
        [SerializeField] private bool _skipSceneTransitionForTest = false;

        public bool IsInitialized { get; private set; }
        public bool HasFailed { get; private set; }
        public GameError? LastError { get; private set; }

        public GameConfig Config
        {
            get => _config;
            set => _config = value;
        }

        public ErrorScreenView ErrorScreen
        {
            get => _errorScreen;
            set => _errorScreen = value;
        }

        public bool AutoRunOnStart
        {
            get => _autoRunOnStart;
            set => _autoRunOnStart = value;
        }

        public bool SkipSceneTransitionForTest
        {
            get => _skipSceneTransitionForTest;
            set => _skipSceneTransitionForTest = value;
        }

        private async void Start()
        {
            if (_autoRunOnStart)
            {
                await RunBootstrapAsync();
            }
        }

        /// <summary>
        /// Executes the deterministic startup sequence.
        /// </summary>
        public async Task<bool> RunBootstrapAsync()
        {
            IsInitialized = false;
            HasFailed = false;
            LastError = null;

            // 1. Logging setup
            LogLevel minLevel = _config != null ? _config.MinimumLogLevel : LogLevel.Debug;
            GameLog.Default = new GameLog(new UnityLogSink(), minLevel);
            var log = GameLog.Default;
            log.Info("Bootstrap", "=== KING WORLD WAR Initializing ===");

            // 2. Mobile frame rate setup
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;

            // 3. Configuration validation
            if (_config == null)
            {
                var err = GameError.ConfigMissing("GameConfig ScriptableObject reference is null.");
                log.Error("Bootstrap", err.Message);
                HandleFailure(err, "CRITICAL ERROR", "Game configuration could not be loaded. Please restart.");
                return false;
            }

            var configValidation = _config.Validate();
            if (configValidation.IsFailure)
            {
                var err = configValidation.Error;
                log.Error("Bootstrap", $"Config validation failed: {err.Message}");
                HandleFailure(err, "CONFIGURATION ERROR", err.Message);
                return false;
            }

            // 4. Maintenance mode check
            if (_config.MaintenanceMode)
            {
                var err = new GameError("MAINTENANCE_ACTIVE", _config.MaintenanceMessage);
                log.Info("Bootstrap", $"Server in maintenance mode: {_config.MaintenanceMessage}");
                HandleFailure(err, "MAINTENANCE", _config.MaintenanceMessage, canRetry: true);
                return false;
            }

            // 5. Register Core services into composition root
            var container = ServiceContainer.Instance;
            container.Reset();
            container.Register<GameConfig>(_config);
            container.Register<ILog>(log);
            container.Register<ISceneLoader>(new UnitySceneLoader(log));
            container.Register<IQualityService>(new QualityService(log));
            container.Register<ILocalizationService>(new LocalizationService(log));
            container.Register<IScreenNavigationService>(new ScreenNavigationService(log));
            container.Register<IModalService>(new ModalService(log));

            var worldMapConfig = ScriptableObject.CreateInstance<WorldMapConfigSO>();
            worldMapConfig.InitializeFromDefaults();
            var territoryRepo = new InMemoryTerritoryStateRepository(worldMapConfig);
            var worldMapService = new WorldMapService(worldMapConfig, territoryRepo, log);

            container.Register<ITerritoryDataSource>(worldMapConfig);
            container.Register<ITerritoryStateRepository>(territoryRepo);
            container.Register<IWorldMapService>(worldMapService);

            container.Register<IAuthService>(new NullAuthService());
            container.Register<IPlayerRepository>(new NullPlayerRepository());

            log.Info("Bootstrap", $"Registered core services in ServiceContainer. Env={_config.Environment}, Ver={_config.AppVersion}");

            // 6. Optional test hook to skip scene transition
            if (_skipSceneTransitionForTest)
            {
                IsInitialized = true;
                log.Info("Bootstrap", "Bootstrap completed successfully (Test mode: skipped scene transition).");
                return true;
            }

            // 7. Transition to MainMenu scene
            var sceneLoader = container.Resolve<ISceneLoader>();
            var loadResult = await sceneLoader.LoadSceneAsync(SceneNameCatalog.MainMenu);
            if (loadResult.IsFailure)
            {
                var err = loadResult.Error;
                log.Error("Bootstrap", $"Failed to load initial menu: {err.Message}");
                HandleFailure(err, "STARTUP ERROR", $"Failed to launch Main Menu: {err.Message}", canRetry: true);
                return false;
            }

            IsInitialized = true;
            log.Info("Bootstrap", "Bootstrap sequence finished. MainMenu loaded.");
            return true;
        }

        private void HandleFailure(GameError error, string displayTitle, string displayMessage, bool canRetry = false)
        {
            HasFailed = true;
            LastError = error;

            if (_errorScreen != null)
            {
                _errorScreen.Show(
                    title: displayTitle,
                    message: displayMessage,
                    errorCode: error.Code,
                    onRetry: canRetry ? () => _ = RunBootstrapAsync() : (Action)null,
                    onQuit: () => Application.Quit()
                );
            }
            else
            {
                GameLog.Default.Error("Bootstrap", $"ErrorScreenView is null! Unhandled error display: [{error.Code}] {displayMessage}");
            }
        }
    }
}
