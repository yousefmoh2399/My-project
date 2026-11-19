using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Contract for providing static building definitions, level stats, and costs.
    /// Implemented by ScriptableObject or server configuration cache.
    /// </summary>
    public interface IBuildingDataSource
    {
        BuildingDefinition GetDefinition(BuildingType type);
        bool TryGetDefinition(BuildingType type, out BuildingDefinition definition);
        IReadOnlyList<BuildingDefinition> GetAllDefinitions();
    }

    /// <summary>
    /// Contract for reading and updating the player's runtime base structures and layout.
    /// Non-authoritative client cache until synchronized with Nakama server.
    /// </summary>
    public interface IBaseStateRepository
    {
        BaseGrid Grid { get; }
        IReadOnlyList<BuildingRuntimeState> GetAllBuildings();
        bool TryGetBuilding(BuildingId id, out BuildingRuntimeState building);
        BuildingRuntimeState GetCommandCenter();
        void UpdateBuilding(BuildingRuntimeState building);
        event Action<BuildingRuntimeState> OnBuildingStateChanged;
    }

    /// <summary>
    /// High-level domain service orchestrating player base interactions, building selection,
    /// and asynchronous upgrade workflows.
    /// </summary>
    public interface IPlayerBaseService
    {
        IBuildingDataSource DataSource { get; }
        IBaseStateRepository StateRepository { get; }

        BuildingId SelectedBuildingId { get; }
        void SelectBuilding(BuildingId id);
        void Deselect();

        Task<Result<BuildingRuntimeState, GameError>> RequestUpgradeAsync(BuildingId id);
        Task<Result<BuildingRuntimeState, GameError>> CancelUpgradeAsync(BuildingId id);
        Task<Result<BuildingRuntimeState, GameError>> MoveBuildingAsync(BuildingId id, int newX, int newY);
        void CheckCompletedUpgrades();

        event Action<BuildingId> OnBuildingSelected;
        event Action<BuildingRuntimeState> OnUpgradeStarted;
        event Action<BuildingRuntimeState> OnBuildingUpgraded;
        event Action<BuildingRuntimeState> OnUpgradeCancelled;
    }
}
