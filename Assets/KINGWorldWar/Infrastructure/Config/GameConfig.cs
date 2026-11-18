using System;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Logging;
using UnityEngine;

namespace KingWorldWar.Infrastructure.Config
{
    /// <summary>
    /// Non-secret client configuration asset for environment, feature flags, and endpoints.
    /// 
    /// SECURITY NOTICE:
    /// NEVER store private keys, database credentials, server secrets, or authentication tokens
    /// in this asset. All client builds are public and untrusted.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "KING WORLD WAR/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Environment")]
        [SerializeField] private EnvironmentType _environment = EnvironmentType.Dev;
        [SerializeField] private string _appVersion = "0.1.0";
        [SerializeField] private int _buildNumber = 1;
        [SerializeField] private LogLevel _minimumLogLevel = LogLevel.Debug;
        [SerializeField] private string _defaultLanguage = "ar";

        [Header("Maintenance / Operational Flags")]
        [SerializeField] private bool _maintenanceMode = false;
        [SerializeField] private string _maintenanceMessage = "Server maintenance in progress. Please check back later.";

        [Header("Backend Endpoints (Placeholders)")]
        [SerializeField] private string _nakamaHost = "127.0.0.1";
        [SerializeField] private int _nakamaPort = 7350;
        [SerializeField] private bool _nakamaUseSsl = false;
        [SerializeField] private bool _autoLoginOnStart = true;

        public EnvironmentType Environment => _environment;
        public string AppVersion => _appVersion;
        public int BuildNumber => _buildNumber;
        public LogLevel MinimumLogLevel => _minimumLogLevel;
        public string DefaultLanguage => _defaultLanguage;
        public bool MaintenanceMode => _maintenanceMode;
        public string MaintenanceMessage => _maintenanceMessage;
        public string NakamaHost => _nakamaHost;
        public int NakamaPort => _nakamaPort;
        public bool NakamaUseSsl => _nakamaUseSsl;
        public bool AutoLoginOnStart => _autoLoginOnStart;

        /// <summary>
        /// Validates that configuration properties are coherent and safe.
        /// </summary>
        public Result<GameError> Validate()
        {
            if (string.IsNullOrWhiteSpace(_appVersion))
            {
                return Result<GameError>.Fail(GameError.ConfigInvalid("App version must be specified."));
            }

            if (_nakamaPort <= 0 || _nakamaPort > 65535)
            {
                return Result<GameError>.Fail(GameError.ConfigInvalid($"Invalid Nakama port: {_nakamaPort}"));
            }

            if (string.IsNullOrWhiteSpace(_nakamaHost))
            {
                return Result<GameError>.Fail(GameError.ConfigInvalid("Nakama host cannot be empty."));
            }

            if (_environment == EnvironmentType.Prod && (_nakamaHost == "127.0.0.1" || _nakamaHost == "localhost"))
            {
                return Result<GameError>.Fail(GameError.ConfigInvalid("Production environment cannot target localhost."));
            }

            return Result<GameError>.Ok();
        }

        /// <summary>
        /// Helper factory for unit testing without asset database.
        /// </summary>
        public static GameConfig CreateInMemory(
            EnvironmentType env,
            string version = "0.1.0",
            string host = "127.0.0.1",
            int port = 7350,
            bool maintenance = false)
        {
            var config = CreateInstance<GameConfig>();
            config._environment = env;
            config._appVersion = version;
            config._nakamaHost = host;
            config._nakamaPort = port;
            config._maintenanceMode = maintenance;
            return config;
        }
    }
}
