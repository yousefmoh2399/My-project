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

        public static GameError BuildingNotFound(string id) =>
            new GameError("BUILDING_NOT_FOUND", $"Building with ID '{id}' was not found.", id);

        public static GameError BuildingMaxLevel(string id, int level) =>
            new GameError("BUILDING_MAX_LEVEL", $"Building '{id}' is already at maximum level {level}.", id);

        public static GameError BuildingAlreadyUpgrading(string id) =>
            new GameError("BUILDING_ALREADY_UPGRADING", $"Building '{id}' is already currently upgrading.", id);

        public static GameError BuildingNotUpgrading(string id) =>
            new GameError("BUILDING_NOT_UPGRADING", $"Building '{id}' is not currently undergoing an upgrade.", id);

        public static GameError InsufficientResources(string details) =>
            new GameError("INSUFFICIENT_RESOURCES", "Player does not possess sufficient resources for this operation.", details);

        public static GameError PrerequisiteNotMet(string details) =>
            new GameError("PREREQUISITE_NOT_MET", "Building upgrade prerequisite has not been satisfied.", details);

        public static GameError GridPlacementFailed(string details) =>
            new GameError("GRID_PLACEMENT_FAILED", "Building cannot be placed at the specified grid coordinates.", details);

        public static GameError UnitNotFound(string id) =>
            new GameError("UNIT_NOT_FOUND", $"Unit definition with ID '{id}' was not found.", id);

        public static GameError UnitPrerequisiteNotMet(string details) =>
            new GameError("UNIT_PREREQUISITE_NOT_MET", "Building prerequisite for unit recruitment is not satisfied.", details);

        public static GameError RecruitmentQueueFull(string details) =>
            new GameError("RECRUITMENT_QUEUE_FULL", "Maximum concurrent recruitment queues reached.", details);

        public static GameError RecruitmentItemNotFound(string queueId) =>
            new GameError("RECRUITMENT_ITEM_NOT_FOUND", $"Recruitment queue item '{queueId}' was not found.", queueId);

        public static GameError InvalidRecruitCount(int count) =>
            new GameError("INVALID_RECRUIT_COUNT", $"Recruitment quantity must be greater than zero (received {count}).", count.ToString());

        public static GameError InvalidPayload(string details) =>
            new GameError("INVALID_PAYLOAD", "Payload is malformed, missing fields, or contains invalid values.", details);

        public static GameError DuplicateRequest(string requestId) =>
            new GameError("DUPLICATE_REQUEST", "Request has already been processed (idempotency key matched).", requestId);

        public static GameError StaleVersion(long expected, long actual) =>
            new GameError("STALE_VERSION", $"Optimistic concurrency check failed (expected revision {expected}, current is {actual}).", $"{expected}:{actual}");

        public static GameError RateLimited(string details) =>
            new GameError("RATE_LIMITED", "Operation rate limit exceeded. Please wait before retrying.", details);

        public static GameError AllianceNotFound(string id) =>
            new GameError("ALLIANCE_NOT_FOUND", $"Alliance with ID '{id}' was not found.", id);

        public static GameError AllianceFull(string id) =>
            new GameError("ALLIANCE_FULL", $"Alliance '{id}' has reached maximum member capacity.", id);

        public static GameError AlreadyInAlliance() =>
            new GameError("ALREADY_IN_ALLIANCE", "Player is already a member of an alliance.", string.Empty);

        public static GameError NotInAlliance() =>
            new GameError("NOT_IN_ALLIANCE", "Player is not a member of any alliance.", string.Empty);

        public static GameError InsufficientPermissions(string reason) =>
            new GameError("INSUFFICIENT_PERMISSIONS", "Actor does not have sufficient alliance permissions for this operation.", reason);

        public static GameError CannotKickHigherOrEqualRole() =>
            new GameError("CANNOT_KICK_HIGHER_ROLE", "Cannot kick a member with equal or higher rank in the alliance hierarchy.", string.Empty);

        public static GameError ApplicationNotFound(string appId) =>
            new GameError("APPLICATION_NOT_FOUND", $"Alliance application with ID '{appId}' was not found.", appId);

        public static GameError AllianceTagTaken(string tag) =>
            new GameError("ALLIANCE_TAG_TAKEN", $"Alliance tag '[{tag}]' is already in use by another alliance.", tag);

        public static GameError AllianceNameTaken(string name) =>
            new GameError("ALLIANCE_NAME_TAKEN", $"Alliance name '{name}' is already in use by another alliance.", name);

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
