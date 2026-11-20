using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Reusable military force selection model for battle deployment (Phase 9 preparation).
    /// Strictly validates selections against available troop counts from the authoritative roster snapshot.
    /// Contains zero battle logic; outputs a clean request DTO.
    /// </summary>
    public sealed class UnitSelection
    {
        public IReadOnlyDictionary<UnitId, int> Selections { get; }
        public int TotalCount => Selections.Values.Sum();
        public bool IsEmpty => Selections.Count == 0 || TotalCount == 0;

        public UnitSelection(IDictionary<UnitId, int> selections = null)
        {
            var dict = new Dictionary<UnitId, int>();
            if (selections != null)
            {
                foreach (var kvp in selections)
                {
                    if (kvp.Value > 0 && !kvp.Key.IsEmpty)
                    {
                        dict[kvp.Key] = kvp.Value;
                    }
                }
            }
            Selections = new ReadOnlyDictionary<UnitId, int>(dict);
        }

        public int GetQuantity(UnitId unitId) =>
            Selections.TryGetValue(unitId, out int q) ? q : 0;

        /// <summary>
        /// Validates that selections do not contain negative/zero counts, unknown units,
        /// and do not exceed available reserves in the snapshot.
        /// </summary>
        public Result<bool, GameError> Validate(ArmyRosterSnapshot snapshot, IArmyDataSource dataSource)
        {
            if (snapshot == null)
            {
                return Result<bool, GameError>.Fail(GameError.InvalidPayload("Roster snapshot is missing."));
            }

            if (IsEmpty)
            {
                return Result<bool, GameError>.Fail(GameError.InvalidPayload("Unit selection cannot be empty."));
            }

            foreach (var kvp in Selections)
            {
                var unitId = kvp.Key;
                int requestedQuantity = kvp.Value;

                if (requestedQuantity <= 0)
                {
                    return Result<bool, GameError>.Fail(
                        GameError.InvalidPayload($"Selected quantity for unit '{unitId}' must be greater than zero."));
                }

                // Verify unit exists in data source
                UnitDefinition def = null;
                if (dataSource != null)
                {
                    def = dataSource.GetAllUnits().FirstOrDefault(u => u.Id == unitId);
                }

                if (def == null)
                {
                    return Result<bool, GameError>.Fail(GameError.UnitNotFound(unitId.Value));
                }

                // Check against available units
                int available = snapshot.GetAvailableCount(def.Type);
                if (requestedQuantity > available)
                {
                    return Result<bool, GameError>.Fail(
                        GameError.InvalidPayload($"Requested {requestedQuantity} of '{unitId}', but only {available} are available for deployment."));
                }
            }

            return Result<bool, GameError>.Ok(true);
        }

        /// <summary>
        /// Compiles valid selections into a serializable deployment request DTO.
        /// </summary>
        public BattleDeploymentRequestDto ToDeploymentRequest(string targetTerritoryId, string requestId = null)
        {
            return new BattleDeploymentRequestDto(
                string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString("N") : requestId,
                targetTerritoryId,
                Selections.ToDictionary(k => k.Key.Value, v => v.Value)
            );
        }
    }

    /// <summary>
    /// Pure client request DTO sent to backend for battle deployment.
    /// </summary>
    [Serializable]
    public sealed class BattleDeploymentRequestDto
    {
        public string RequestId { get; }
        public string TargetTerritoryId { get; }
        public IReadOnlyDictionary<string, int> Units { get; }

        public BattleDeploymentRequestDto(string requestId, string targetTerritoryId, Dictionary<string, int> units)
        {
            RequestId = requestId ?? Guid.NewGuid().ToString("N");
            TargetTerritoryId = targetTerritoryId ?? string.Empty;
            Units = new ReadOnlyDictionary<string, int>(units ?? new Dictionary<string, int>());
        }
    }
}
