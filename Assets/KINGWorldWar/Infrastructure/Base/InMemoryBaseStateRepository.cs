using System;
using System.Collections.Generic;
using KingWorldWar.Core.Base;

namespace KingWorldWar.Infrastructure.Base
{
    /// <summary>
    /// In-memory player base state repository for local development, tests, and offline mocking.
    /// In production, player structures and coordinates are synchronized with Nakama server.
    /// Implements IBaseRepository with IsMock = true.
    /// </summary>
    public class InMemoryBaseStateRepository : IBaseRepository
    {
        public bool IsMock => true;
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
            // 1. Command Center (HQ)
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

            // 2. Training Center / Barracks
            var trainDef = dataSource?.GetDefinition(BuildingType.TrainingCenter);
            var trainWidth = trainDef?.FootprintWidth ?? 3;
            var trainHeight = trainDef?.FootprintHeight ?? 3;
            var trainingCenter = new BuildingRuntimeState(
                new BuildingId("bld_barracks_01"),
                BuildingType.TrainingCenter,
                currentLevel: 1,
                gridX: 2,
                gridY: 4);
            RegisterBuilding(trainingCenter, trainWidth, trainHeight);

            // 3. Resource Depot / Supply Depot
            var depDef = dataSource?.GetDefinition(BuildingType.ResourceDepot);
            var depWidth = depDef?.FootprintWidth ?? 3;
            var depHeight = depDef?.FootprintHeight ?? 3;
            var depot = new BuildingRuntimeState(
                new BuildingId("bld_depot_01"),
                BuildingType.ResourceDepot,
                currentLevel: 1,
                gridX: 14,
                gridY: 4);
            RegisterBuilding(depot, depWidth, depHeight);

            // 4. Airfield
            var airDef = dataSource?.GetDefinition(BuildingType.Airfield);
            var airWidth = airDef?.FootprintWidth ?? 4;
            var airHeight = airDef?.FootprintHeight ?? 4;
            var airfield = new BuildingRuntimeState(
                new BuildingId("bld_airfield_01"),
                BuildingType.Airfield,
                currentLevel: 1,
                gridX: 2,
                gridY: 8);
            RegisterBuilding(airfield, airWidth, airHeight);

            // 5. Arms Factory
            var armsDef = dataSource?.GetDefinition(BuildingType.ArmsFactory);
            var armsWidth = armsDef?.FootprintWidth ?? 3;
            var armsHeight = armsDef?.FootprintHeight ?? 3;
            var armsFactory = new BuildingRuntimeState(
                new BuildingId("bld_arms_factory_01"),
                BuildingType.ArmsFactory,
                currentLevel: 1,
                gridX: 2,
                gridY: 14);
            RegisterBuilding(armsFactory, armsWidth, armsHeight);

            // 6. Research Center
            var resDef = dataSource?.GetDefinition(BuildingType.ResearchCenter);
            var resWidth = resDef?.FootprintWidth ?? 3;
            var resHeight = resDef?.FootprintHeight ?? 3;
            var researchCenter = new BuildingRuntimeState(
                new BuildingId("bld_research_center_01"),
                BuildingType.ResearchCenter,
                currentLevel: 1,
                gridX: 14,
                gridY: 9);
            RegisterBuilding(researchCenter, resWidth, resHeight);

            // 7. Tank Factory / Vehicle Factory
            var facDef = dataSource?.GetDefinition(BuildingType.TankFactory);
            var facWidth = facDef?.FootprintWidth ?? 4;
            var facHeight = facDef?.FootprintHeight ?? 3;
            var tankFactory = new BuildingRuntimeState(
                new BuildingId("bld_factory_01"),
                BuildingType.TankFactory,
                currentLevel: 1,
                gridX: 13,
                gridY: 13);
            RegisterBuilding(tankFactory, facWidth, facHeight);

            // 8. Auxiliary Radar Station
            var radDef = dataSource?.GetDefinition(BuildingType.RadarStation);
            var radWidth = radDef?.FootprintWidth ?? 2;
            var radHeight = radDef?.FootprintHeight ?? 2;
            var radar = new BuildingRuntimeState(
                new BuildingId("bld_radar_01"),
                BuildingType.RadarStation,
                currentLevel: 1,
                gridX: 8,
                gridY: 14);
            RegisterBuilding(radar, radWidth, radHeight);
        }

        private void RegisterBuilding(BuildingRuntimeState building, int width, int height)
        {
            Grid.TryPlace(building, width, height);
            _buildings[building.Id] = building;
        }

        public IReadOnlyList<BuildingRuntimeState> GetAllBuildings()
        {
            var list = new List<BuildingRuntimeState>(_buildings.Count);
            foreach (var b in _buildings.Values) list.Add(b);
            return list;
        }

        public bool TryGetBuilding(BuildingId id, out BuildingRuntimeState building) =>
            _buildings.TryGetValue(id, out building);

        public BuildingRuntimeState GetCommandCenter() => _commandCenter;

        public void UpdateBuilding(BuildingRuntimeState building)
        {
            if (_buildings.ContainsKey(building.Id))
            {
                _buildings[building.Id] = building;
                if (building.Type == BuildingType.CommandCenter)
                {
                    _commandCenter = building;
                }
                OnBuildingStateChanged?.Invoke(building);
            }
        }
    }
}
