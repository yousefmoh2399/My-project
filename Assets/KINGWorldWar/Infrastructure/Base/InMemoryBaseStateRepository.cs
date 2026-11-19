using System;
using System.Collections.Generic;
using KingWorldWar.Core.Base;

namespace KingWorldWar.Infrastructure.Base
{
    /// <summary>
    /// In-memory player base state repository for local development, tests, and offline mocking.
    /// In production, player structures and coordinates are synchronized with Nakama server.
    /// </summary>
    public sealed class InMemoryBaseStateRepository : IBaseStateRepository
    {
        public BaseGrid Grid { get; }

        private readonly Dictionary<BuildingId, BuildingRuntimeState> _buildings =
            new Dictionary<BuildingId, BuildingRuntimeState>();

        private BuildingRuntimeState _commandCenter;

        public event Action<BuildingRuntimeState> OnBuildingStateChanged;

        public InMemoryBaseStateRepository(IBuildingDataSource dataSource, int gridWidth = 20, int gridHeight = 20)
        {
            Grid = new BaseGrid(gridWidth, gridHeight);
            InitializeDefaultBase(dataSource);
        }

        private void InitializeDefaultBase(IBuildingDataSource dataSource)
        {
            var hqDef = dataSource?.GetDefinition(BuildingType.CommandCenter);
            var hqWidth = hqDef?.FootprintWidth ?? 4;
            var hqHeight = hqDef?.FootprintHeight ?? 4;

            _commandCenter = new BuildingRuntimeState(
                new BuildingId("bld_hq"),
                BuildingType.CommandCenter,
                currentLevel: 1,
                gridX: 8,
                gridY: 8);
            RegisterBuilding(_commandCenter, hqWidth, hqHeight);

            var barDef = dataSource?.GetDefinition(BuildingType.Barracks);
            var barracks = new BuildingRuntimeState(
                new BuildingId("bld_barracks_01"),
                BuildingType.Barracks,
                currentLevel: 1,
                gridX: 2,
                gridY: 4);
            RegisterBuilding(barracks, barDef?.FootprintWidth ?? 3, barDef?.FootprintHeight ?? 3);

            var depDef = dataSource?.GetDefinition(BuildingType.SupplyDepot);
            var depot = new BuildingRuntimeState(
                new BuildingId("bld_depot_01"),
                BuildingType.SupplyDepot,
                currentLevel: 1,
                gridX: 14,
                gridY: 4);
            RegisterBuilding(depot, depDef?.FootprintWidth ?? 3, depDef?.FootprintHeight ?? 3);

            var radDef = dataSource?.GetDefinition(BuildingType.RadarStation);
            var radar = new BuildingRuntimeState(
                new BuildingId("bld_radar_01"),
                BuildingType.RadarStation,
                currentLevel: 1,
                gridX: 3,
                gridY: 14);
            RegisterBuilding(radar, radDef?.FootprintWidth ?? 2, radDef?.FootprintHeight ?? 2);

            var facDef = dataSource?.GetDefinition(BuildingType.VehicleFactory);
            var factory = new BuildingRuntimeState(
                new BuildingId("bld_factory_01"),
                BuildingType.VehicleFactory,
                currentLevel: 1,
                gridX: 13,
                gridY: 13);
            RegisterBuilding(factory, facDef?.FootprintWidth ?? 4, facDef?.FootprintHeight ?? 3);
        }

        private void RegisterBuilding(BuildingRuntimeState building, int width, int height)
        {
            Grid.TryPlace(building, width, height);
            _buildings[building.Id] = building;
        }

        public IReadOnlyList<BuildingRuntimeState> GetAllBuildings()
        {
            return new List<BuildingRuntimeState>(_buildings.Values);
        }

        public bool TryGetBuilding(BuildingId id, out BuildingRuntimeState building)
        {
            return _buildings.TryGetValue(id, out building);
        }

        public BuildingRuntimeState GetCommandCenter()
        {
            return _commandCenter;
        }

        public void UpdateBuilding(BuildingRuntimeState building)
        {
            if (building == null) return;
            _buildings[building.Id] = building;
            OnBuildingStateChanged?.Invoke(building);
        }
    }
}
