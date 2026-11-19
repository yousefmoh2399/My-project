using System;
using System.Threading.Tasks;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Logging;

namespace KingWorldWar.Infrastructure.Base
{
    /// <summary>
    /// Coordinates player base interactions, building upgrades, and placement validation.
    /// Non-authoritative local simulation client service; maps to Nakama server RPCs in Phase 8.
    /// </summary>
    public sealed class PlayerBaseService : IPlayerBaseService
    {
        public IBuildingDataSource DataSource { get; }
        public IBaseStateRepository StateRepository { get; }

        private readonly ILog _log;

        public BuildingId SelectedBuildingId { get; private set; } = BuildingId.Empty;

        public event Action<BuildingId> OnBuildingSelected;
        public event Action<BuildingRuntimeState> OnUpgradeStarted;
        public event Action<BuildingRuntimeState> OnBuildingUpgraded;
        public event Action<BuildingRuntimeState> OnUpgradeCancelled;

        public PlayerBaseService(
            IBuildingDataSource dataSource,
            IBaseStateRepository stateRepository,
            ILog log = null)
        {
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            StateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
            _log = log ?? GameLog.Default;
        }

        private const string Tag = "PlayerBaseService";

        public void SelectBuilding(BuildingId id)
        {
            if (SelectedBuildingId == id) return;
            SelectedBuildingId = id;
            _log.Info(Tag, $"Selected building: {id}");
            OnBuildingSelected?.Invoke(id);
        }

        public void Deselect()
        {
            if (SelectedBuildingId == BuildingId.Empty) return;
            SelectedBuildingId = BuildingId.Empty;
            _log.Info(Tag, "Deselected building.");
            OnBuildingSelected?.Invoke(BuildingId.Empty);
        }

        public Task<Result<BuildingRuntimeState, GameError>> RequestUpgradeAsync(BuildingId id)
        {
            if (!StateRepository.TryGetBuilding(id, out var building))
            {
                var err = GameError.BuildingNotFound(id.Value);
                _log.Warning(Tag, $"Upgrade failed: {err}");
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            if (building.IsUpgrading)
            {
                var err = GameError.BuildingAlreadyUpgrading(id.Value);
                _log.Warning(Tag, $"Upgrade failed: {err}");
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            if (!DataSource.TryGetDefinition(building.Type, out var def))
            {
                var err = GameError.BuildingNotFound(building.Type.ToString());
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            if (building.CurrentLevel >= def.MaxLevel)
            {
                var err = GameError.BuildingMaxLevel(id.Value, def.MaxLevel);
                _log.Warning(Tag, $"Upgrade failed: {err}");
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            int targetLevel = building.CurrentLevel + 1;
            if (!def.TryGetLevelData(targetLevel, out var nextLevelData))
            {
                var err = GameError.ConfigInvalid($"Level data missing for {building.Type} level {targetLevel}");
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            // Prerequisite validation: check Command Center level requirement
            if (building.Type != BuildingType.CommandCenter)
            {
                var cc = StateRepository.GetCommandCenter();
                int currentHQLevel = cc?.CurrentLevel ?? 1;
                if (currentHQLevel < nextLevelData.RequiredHQLevel)
                {
                    var err = GameError.PrerequisiteNotMet(
                        $"Requires Command Center Level {nextLevelData.RequiredHQLevel} (Current: {currentHQLevel})");
                    _log.Warning(Tag, $"Upgrade failed: {err}");
                    return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
                }
            }

            // Start simulated local upgrade
            building.StartUpgrade(DateTime.UtcNow, nextLevelData.UpgradeDurationSeconds);
            StateRepository.UpdateBuilding(building);

            _log.Info(Tag, $"Upgrade started for {building.Id} to Lv.{targetLevel} (Duration: {nextLevelData.UpgradeDurationSeconds}s)");
            OnUpgradeStarted?.Invoke(building);

            return Task.FromResult(Result<BuildingRuntimeState, GameError>.Ok(building));
        }

        public Task<Result<BuildingRuntimeState, GameError>> CancelUpgradeAsync(BuildingId id)
        {
            if (!StateRepository.TryGetBuilding(id, out var building))
            {
                var err = GameError.BuildingNotFound(id.Value);
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            if (!building.IsUpgrading)
            {
                var err = GameError.BuildingNotUpgrading(id.Value);
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            building.CancelUpgrade();
            StateRepository.UpdateBuilding(building);

            _log.Info(Tag, $"Upgrade cancelled for {building.Id}");
            OnUpgradeCancelled?.Invoke(building);

            return Task.FromResult(Result<BuildingRuntimeState, GameError>.Ok(building));
        }

        public Task<Result<BuildingRuntimeState, GameError>> MoveBuildingAsync(BuildingId id, int newX, int newY)
        {
            if (!StateRepository.TryGetBuilding(id, out var building))
            {
                var err = GameError.BuildingNotFound(id.Value);
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            if (!StateRepository.Grid.TryMove(id, newX, newY))
            {
                var err = GameError.GridPlacementFailed($"Cannot move {id} to [{newX},{newY}] - space occupied or out of bounds.");
                _log.Warning(Tag, $"Move failed: {err}");
                return Task.FromResult(Result<BuildingRuntimeState, GameError>.Fail(err));
            }

            StateRepository.UpdateBuilding(building);
            _log.Info(Tag, $"Moved {id} to [{newX},{newY}]");

            return Task.FromResult(Result<BuildingRuntimeState, GameError>.Ok(building));
        }

        public void CheckCompletedUpgrades()
        {
            var now = DateTime.UtcNow;
            var buildings = StateRepository.GetAllBuildings();

            foreach (var b in buildings)
            {
                if (b.IsUpgrading && b.UpgradeEndTimeUtc.HasValue && now >= b.UpgradeEndTimeUtc.Value)
                {
                    b.CompleteUpgrade();
                    StateRepository.UpdateBuilding(b);
                    _log.Info(Tag, $"Upgrade complete! {b.Id} is now Level {b.CurrentLevel}");
                    OnBuildingUpgraded?.Invoke(b);
                }
            }
        }
    }
}
