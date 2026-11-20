using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;
using KingWorldWar.Infrastructure.Army;
using KingWorldWar.Infrastructure.Base;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ArmyServiceTests
    {
        private ArmyConfigSO _armyConfig;
        private InMemoryArmyStateRepository _armyRepo;
        private BaseConfigSO _baseConfig;
        private InMemoryBaseStateRepository _baseRepo;
        private ArmyService _service;

        [SetUp]
        public void SetUp()
        {
            _armyConfig = ScriptableObject.CreateInstance<ArmyConfigSO>();
            _armyConfig.InitializeFromDefaults();

            _armyRepo = new InMemoryArmyStateRepository(maxConcurrentQueues: 2);

            _baseConfig = ScriptableObject.CreateInstance<BaseConfigSO>();
            _baseConfig.InitializeFromDefaults();

            _baseRepo = new InMemoryBaseStateRepository(_baseConfig);

            _service = new ArmyService(_armyConfig, _armyRepo, _baseRepo);
        }

        [TearDown]
        public void TearDown()
        {
            if (_armyConfig != null) UnityEngine.Object.DestroyImmediate(_armyConfig);
            if (_baseConfig != null) UnityEngine.Object.DestroyImmediate(_baseConfig);
        }

        [Test]
        public async Task RequestRecruitment_Valid_ReturnsOkAndEnqueues()
        {
            RecruitmentQueueItem startedItem = null;
            _service.OnRecruitmentStarted += item => startedItem = item;

            var result = await _service.RequestRecruitmentAsync(UnitType.Tank, 10);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Type, Is.EqualTo(UnitType.Tank));
            Assert.That(result.Value.Count, Is.EqualTo(10));
            Assert.That(_service.GetActiveQueues().Count, Is.EqualTo(1));
            Assert.That(startedItem, Is.Not.Null);
            Assert.That(startedItem.QueueId, Is.EqualTo(result.Value.QueueId));
        }

        [Test]
        public async Task RequestRecruitment_InvalidCount_Fails()
        {
            var resultZero = await _service.RequestRecruitmentAsync(UnitType.Tank, 0);
            Assert.That(resultZero.IsFailure, Is.True);
            Assert.That(resultZero.Error.Code, Is.EqualTo("INVALID_RECRUIT_COUNT"));

            var resultNegative = await _service.RequestRecruitmentAsync(UnitType.Tank, -5);
            Assert.That(resultNegative.IsFailure, Is.True);
            Assert.That(resultNegative.Error.Code, Is.EqualTo("INVALID_RECRUIT_COUNT"));
        }

        [Test]
        public async Task RequestRecruitment_ExceedsMaxQueues_Fails()
        {
            var q1 = await _service.RequestRecruitmentAsync(UnitType.Tank, 10);
            Assert.That(q1.IsSuccess, Is.True);

            var q2 = await _service.RequestRecruitmentAsync(UnitType.ArmoredVehicle, 1);
            Assert.That(q2.IsSuccess, Is.True);

            var q3 = await _service.RequestRecruitmentAsync(UnitType.Tank, 5);
            Assert.That(q3.IsFailure, Is.True);
            Assert.That(q3.Error.Code, Is.EqualTo("RECRUITMENT_QUEUE_FULL"));
        }

        [Test]
        public async Task RequestRecruitment_PrerequisiteNotMet_Fails()
        {
            // Heavy Artillery requires TankFactory Level 2.
            // In initial base repo, Tank Factory is Level 1.
            var result = await _service.RequestRecruitmentAsync(UnitType.Artillery, 1);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("UNIT_PREREQUISITE_NOT_MET"));
        }

        [Test]
        public async Task RequestRecruitment_PrerequisiteSatisfied_Succeeds()
        {
            // Upgrade Tank Factory to Level 2 in baseRepo
            BuildingRuntimeState factory = default;
            foreach (var b in _baseRepo.GetAllBuildings())
            {
                if (b.Type == BuildingType.TankFactory)
                {
                    factory = b;
                    break;
                }
            }

            Assert.That(factory.Id.IsValid, Is.True, "Tank Factory should exist in initial base");
            var upgradedFactory = new BuildingRuntimeState(
                factory.Id, factory.Type, currentLevel: 2, factory.GridX, factory.GridY);
            _baseRepo.UpdateBuilding(upgradedFactory);

            var result = await _service.RequestRecruitmentAsync(UnitType.Artillery, 1);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Type, Is.EqualTo(UnitType.Artillery));
        }

        [Test]
        public async Task CancelRecruitment_Existing_RemovesQueue()
        {
            var rec = await _service.RequestRecruitmentAsync(UnitType.Tank, 10);
            Assert.That(rec.IsSuccess, Is.True);
            string queueId = rec.Value.QueueId;

            string cancelledId = null;
            _service.OnRecruitmentCancelled += qId => cancelledId = qId;

            var cancelResult = await _service.CancelRecruitmentAsync(queueId);
            Assert.That(cancelResult.IsSuccess, Is.True);
            Assert.That(_service.GetActiveQueues().Count, Is.EqualTo(0));
            Assert.That(cancelledId, Is.EqualTo(queueId));
        }

        [Test]
        public async Task CancelRecruitment_NonExisting_Fails()
        {
            var cancelResult = await _service.CancelRecruitmentAsync("non_existing_queue_id");
            Assert.That(cancelResult.IsFailure, Is.True);
            Assert.That(cancelResult.Error.Code, Is.EqualTo("RECRUITMENT_ITEM_NOT_FOUND"));
        }

        [Test]
        public void CheckCompletedRecruitments_ElapsedQueue_AddsUnitsToRoster()
        {
            var startTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var endTime = startTime.AddSeconds(30);
            var item = new RecruitmentQueueItem(
                "q_test_1",
                UnitType.Tank,
                count: 3,
                startTimeUtc: startTime,
                endTimeUtc: endTime,
                totalCost: new UnitCost(100, 100, 100, 30));

            _armyRepo.EnqueueRecruitment(item);
            Assert.That(_service.GetActiveQueues().Count, Is.EqualTo(1));

            int initialTanks = _service.GetRoster().GetCount(UnitType.Tank);

            bool rosterEventFired = false;
            _service.OnRosterUpdated += () => rosterEventFired = true;

            UnitType completedType = default;
            int completedCount = 0;
            _service.OnRecruitmentCompleted += (t, c) =>
            {
                completedType = t;
                completedCount = c;
            };

            // Check before completion time
            var check1 = _service.CheckCompletedRecruitments(startTime.AddSeconds(20));
            Assert.That(check1.Value, Is.EqualTo(0));
            Assert.That(_service.GetActiveQueues().Count, Is.EqualTo(1));
            Assert.That(rosterEventFired, Is.False);

            // Check after completion time
            var check2 = _service.CheckCompletedRecruitments(endTime.AddSeconds(5));
            Assert.That(check2.Value, Is.EqualTo(1));
            Assert.That(_service.GetActiveQueues().Count, Is.EqualTo(0));
            Assert.That(rosterEventFired, Is.True);
            Assert.That(completedType, Is.EqualTo(UnitType.Tank));
            Assert.That(completedCount, Is.EqualTo(3));
            Assert.That(_service.GetRoster().GetCount(UnitType.Tank), Is.EqualTo(initialTanks + 3));
        }
    }
}
