using System;
using System.Threading.Tasks;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Logging;
using KingWorldWar.Infrastructure.Base;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class BuildingUpgradeTests
    {
        private IBuildingDataSource _dataSource;
        private IBaseStateRepository _stateRepository;
        private PlayerBaseService _service;

        [SetUp]
        public void SetUp()
        {
            var configSO = ScriptableObject.CreateInstance<BaseConfigSO>();
            configSO.InitializeFromDefaults();
            _dataSource = configSO;
            _stateRepository = new InMemoryBaseStateRepository(_dataSource);
            _service = new PlayerBaseService(_dataSource, _stateRepository, GameLog.Default);
        }

        [Test]
        public void Definitions_AllSevenMvpArchetypesConfigured()
        {
            var defs = _dataSource.GetAllDefinitions();
            Assert.That(defs.Count, Is.GreaterThanOrEqualTo(7), "Must configure at least 7 MVP building archetypes");

            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.CommandCenter, out _));
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.TankFactory, out _));
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.Airfield, out _));
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.ArmsFactory, out _));
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.ResearchCenter, out _));
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.ResourceDepot, out _));
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.TrainingCenter, out _));

            // Backward compatibility aliases
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.Barracks, out _));
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.SupplyDepot, out _));
            Assert.IsTrue(_dataSource.TryGetDefinition(BuildingType.VehicleFactory, out _));
        }

        [Test]
        public void Definitions_EachArchetype_HasTenLevelsWithValidProgression()
        {
            var defs = _dataSource.GetAllDefinitions();
            foreach (var def in defs)
            {
                Assert.AreEqual(10, def.MaxLevel, $"{def.Type} must have MaxLevel 10");
                Assert.AreEqual(10, def.Levels.Count, $"{def.Type} must have 10 level definitions");

                for (int lvl = 1; lvl <= 10; lvl++)
                {
                    Assert.IsTrue(def.TryGetLevelData(lvl, out var data), $"{def.Type} missing level {lvl}");
                    Assert.AreEqual(lvl, data.Level);

                    if (lvl > 1)
                    {
                        Assert.IsTrue(data.Cost.Gold > 0 || data.Cost.Oil > 0 || data.Cost.Iron > 0,
                            $"{def.Type} Lv.{lvl} cost must be positive");
                        Assert.IsTrue(data.UpgradeDurationSeconds > 0,
                            $"{def.Type} Lv.{lvl} duration must be positive");
                    }
                }
            }
        }

        [Test]
        public void Prerequisites_NoCycles_EnsuresDirectedAcyclicGraph()
        {
            var defs = _dataSource.GetAllDefinitions();
            foreach (var def in defs)
            {
                foreach (var lvl in def.Levels.Values)
                {
                    if (def.Type == BuildingType.CommandCenter)
                    {
                        Assert.AreEqual(0, lvl.RequiredHQLevel, "Command Center must never require external prerequisites (anchor)");
                    }
                    else
                    {
                        Assert.IsTrue(lvl.RequiredHQLevel >= 0 && lvl.RequiredHQLevel <= 10,
                            $"{def.Type} Lv.{lvl.Level} must have valid non-negative HQ prerequisite <= 10");
                    }
                }
            }
        }

        [Test]
        public void MockRepository_IsIsolatedAndMarkedMock()
        {
            var mockRepo = new LocalMockBaseRepository(_dataSource);
            Assert.IsTrue(mockRepo.IsMock);
            Assert.That(mockRepo.GetAllBuildings().Count, Is.GreaterThanOrEqualTo(7));
        }

        [Test]
        public async Task Upgrade_CommandCenter_StartsUpgradeTimer()
        {
            var hqId = new BuildingId("bld_hq");
            var result = await _service.RequestUpgradeAsync(hqId);

            Assert.IsTrue(result.IsSuccess);
            var hq = result.Value;
            Assert.IsTrue(hq.IsUpgrading);
            Assert.IsTrue(hq.UpgradeStartTimeUtc.HasValue);
            Assert.IsTrue(hq.UpgradeEndTimeUtc.HasValue);
            Assert.IsTrue(hq.UpgradeEndTimeUtc.Value > hq.UpgradeStartTimeUtc.Value);
        }

        [Test]
        public async Task Upgrade_AlreadyUpgrading_FailsWithBuildingAlreadyUpgrading()
        {
            var hqId = new BuildingId("bld_hq");
            await _service.RequestUpgradeAsync(hqId);

            // Second request while already upgrading
            var secondResult = await _service.RequestUpgradeAsync(hqId);

            Assert.IsTrue(secondResult.IsFailure);
            Assert.AreEqual("BUILDING_ALREADY_UPGRADING", secondResult.Error.Code);
        }

        [Test]
        public async Task Upgrade_PrerequisiteNotMet_Fails()
        {
            // By default, HQ is Level 1. TrainingCenter/Barracks Lv 2 requires HQ Level 2.
            var barId = new BuildingId("bld_barracks_01");
            var result = await _service.RequestUpgradeAsync(barId);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("PREREQUISITE_NOT_MET", result.Error.Code);
        }

        [Test]
        public async Task Upgrade_PrerequisiteMet_Succeeds()
        {
            // Simulate HQ upgrade completion to Level 2
            var hq = _stateRepository.GetCommandCenter();
            hq.CompleteUpgrade(); // Now Level 2
            _stateRepository.UpdateBuilding(hq);

            var barId = new BuildingId("bld_barracks_01");
            var result = await _service.RequestUpgradeAsync(barId);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.IsUpgrading);
        }

        [Test]
        public async Task Upgrade_Cancel_ResetsUpgradeState()
        {
            var hqId = new BuildingId("bld_hq");
            await _service.RequestUpgradeAsync(hqId);

            var cancelResult = await _service.CancelUpgradeAsync(hqId);

            Assert.IsTrue(cancelResult.IsSuccess);
            var hq = cancelResult.Value;
            Assert.IsFalse(hq.IsUpgrading);
            Assert.IsNull(hq.UpgradeStartTimeUtc);
            Assert.IsNull(hq.UpgradeEndTimeUtc);
        }

        [Test]
        public void Upgrade_CheckCompleted_IncrementsLevelWhenTimeElapsed()
        {
            var hq = _stateRepository.GetCommandCenter();
            // Start upgrade in the past (e.g. 10 seconds ago with 5 second duration)
            hq.StartUpgrade(DateTime.UtcNow.AddSeconds(-10), 5);
            _stateRepository.UpdateBuilding(hq);

            Assert.IsTrue(hq.IsUpgrading);
            Assert.AreEqual(1, hq.CurrentLevel);

            _service.CheckCompletedUpgrades();

            Assert.IsFalse(hq.IsUpgrading);
            Assert.AreEqual(2, hq.CurrentLevel);
        }

        [Test]
        public async Task Upgrade_MaxLevel_FailsWithBuildingMaxLevel()
        {
            var hq = _stateRepository.GetCommandCenter();
            // Fast forward HQ to Level 10
            for (int i = 1; i < 10; i++)
            {
                hq.CompleteUpgrade();
            }
            _stateRepository.UpdateBuilding(hq);
            Assert.AreEqual(10, hq.CurrentLevel);

            var result = await _service.RequestUpgradeAsync(hq.Id);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("BUILDING_MAX_LEVEL", result.Error.Code);
        }

        [Test]
        public async Task Upgrade_NonExistentBuilding_FailsWithBuildingNotFound()
        {
            var ghostId = new BuildingId("bld_ghost_non_existent");
            var result = await _service.RequestUpgradeAsync(ghostId);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("BUILDING_NOT_FOUND", result.Error.Code);
        }
    }
}
