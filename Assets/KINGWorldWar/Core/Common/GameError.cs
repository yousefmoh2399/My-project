using System;

namespace KingWorldWar.Core.Common
{
    /// <summary>
    /// Represents a structured, typed application error with a stable code.
    /// </summary>
    public readonly struct GameError : IEquatable<GameError>
    {
        public string Code { get; }
        public string Message { get; }
        public string Details { get; }

        public GameError(string code, string message, string details = null)
        {
            Code = code ?? "UNKNOWN_ERROR";
            Message = message ?? string.Empty;
            Details = details ?? string.Empty;
        }

        public static GameError ConfigMissing(string details) =>
            new GameError("CONFIG_MISSING", "Configuration asset is missing or not assigned.", details);

        public static GameError ConfigInvalid(string details) =>
            new GameError("CONFIG_INVALID", "Configuration asset contains invalid or conflicting settings.", details);

        public static GameError SceneNotFound(string sceneName) =>
            new GameError("SCENE_NOT_FOUND", $"Scene '{sceneName}' is not registered or cannot be found.", sceneName);

        public static GameError SceneLoadFailed(string sceneName, string reason) =>
            new GameError("SCENE_LOAD_FAILED", $"Failed to load scene '{sceneName}': {reason}", reason);

        public static GameError ServiceInitFailed(string serviceName, string reason) =>
            new GameError("SERVICE_INIT_FAILED", $"Failed to initialize service '{serviceName}': {reason}", reason);

        public static GameError NetworkUnavailable(string reason) =>
            new GameError("NETWORK_UNAVAILABLE", "Network is unreachable or offline.", reason);

        public bool Equals(GameError other) =>
            string.Equals(Code, other.Code, StringComparison.Ordinal) &&
            string.Equals(Message, other.Message, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is GameError other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Code, Message);

        public override string ToString() =>
            string.IsNullOrEmpty(Details) ? $"[{Code}] {Message}" : $"[{Code}] {Message} ({Details})";
    }
}
