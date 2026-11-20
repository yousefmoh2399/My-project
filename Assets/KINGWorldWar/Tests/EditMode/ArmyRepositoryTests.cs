using System;
using System.Threading.Tasks;
using KingWorldWar.Core.Army;
using KingWorldWar.Infrastructure.Army;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ArmyRepositoryTests
    {
        private ArmyConfigSO _dataSource;
        private LocalMockArmyRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _dataSource = ScriptableObject.CreateInstance<ArmyConfigSO>();
            _dataSource.InitializeFromDefaults();

            _repository = new LocalMockArmyRepository(_dataSource)
            {
                SimulatedLatencyMs = 0
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (_dataSource != null)
            {
                UnityEngine.Object.DestroyImmediate(_dataSource);
            }
        }

        [Test]
        public async Task GetRosterAsync_ReturnsAuthoritativeInitialSnapshot()
        {
            var res = await _repository.GetRosterAsync();

            Assert.That(res.IsSuccess, Is.True);
            var snapshot = res.Value;
            Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot.PlayerId, Is.EqualTo("usr_commander_01"));
            Assert.That(snapshot.BalanceVersion, Is.EqualTo("1.0.0"));
            Assert.That(snapshot.GetTotalOwnedCount(UnitType.Tank), Is.EqualTo(10));
            Assert.That(snapshot.GetTotalOwnedCount(UnitType.ArmoredVehicle), Is.EqualTo(15));
            Assert.That(snapshot.GetTotalOwnedCount(UnitType.Artillery), Is.EqualTo(6));
            Assert.That(snapshot.GetTotalOwnedCount(UnitType.FighterAircraft), Is.EqualTo(4));
            Assert.That(snapshot.GetTotalOwnedCount(UnitType.ReconAircraft), Is.EqualTo(8));
        }

        [Test]
        public async Task RequestTrainingAsync_ValidRequest_EnqueuesSuccessfully()
        {
            string reqId = Guid.NewGuid().ToString("D");
            var res = await _repository.RequestTrainingAsync(reqId, UnitType.Tank, 5);

            Assert.That(res.IsSuccess, Is.True);
            var item = res.Value;
            Assert.That(item.Type, Is.EqualTo(UnitType.Tank));
            Assert.That(item.Count, Is.EqualTo(5));
            Assert.That(item.State, Is.EqualTo(TrainingQueueState.InProgress));

            var queueRes = await _repository.GetTrainingQueueAsync();
            Assert.That(queueRes.Value.Count, Is.EqualTo(1));
            Assert.That(queueRes.Value[0].QueueId, Is.EqualTo(item.QueueId));

            var rosterRes = await _repository.GetRosterAsync();
            Assert.That(rosterRes.Value.GetInTrainingCount(UnitType.Tank), Is.EqualTo(5));
        }

        [Test]
        public async Task RequestTrainingAsync_IdempotentReplay_ReturnsCachedItem()
        {
            string reqId = Guid.NewGuid().ToString("D");
            var first = await _repository.RequestTrainingAsync(reqId, UnitType.Tank, 3);
            Assert.That(first.IsSuccess, Is.True);

            var second = await _repository.RequestTrainingAsync(reqId, UnitType.Tank, 3);
            Assert.That(second.IsSuccess, Is.True);
            Assert.That(second.Value.QueueId, Is.EqualTo(first.Value.QueueId));

            var queueRes = await _repository.GetTrainingQueueAsync();
            Assert.That(queueRes.Value.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task RequestTrainingAsync_ExceedingMaxSlots_RejectsWithQueueFull()
        {
            var q1 = await _repository.RequestTrainingAsync(Guid.NewGuid().ToString("D"), UnitType.Tank, 1);
            Assert.That(q1.IsSuccess, Is.True);

            var q2 = await _repository.RequestTrainingAsync(Guid.NewGuid().ToString("D"), UnitType.ArmoredVehicle, 1);
            Assert.That(q2.IsSuccess, Is.True);

            // Third request exceeds MaxQueueSlots = 2
            var q3 = await _repository.RequestTrainingAsync(Guid.NewGuid().ToString("D"), UnitType.Artillery, 1);
            Assert.That(q3.IsFailure, Is.True);
            Assert.That(q3.Error.Code, Is.EqualTo("RECRUITMENT_QUEUE_FULL"));
        }

        [Test]
        public async Task RequestCancelTrainingAsync_ExistingItem_RemovesAndFreesSlot()
        {
            var trainRes = await _repository.RequestTrainingAsync(Guid.NewGuid().ToString("D"), UnitType.FighterAircraft, 2);
            Assert.That(trainRes.IsSuccess, Is.True);
            string queueId = trainRes.Value.QueueId;

            var cancelRes = await _repository.RequestCancelTrainingAsync(Guid.NewGuid().ToString("D"), queueId);
            Assert.That(cancelRes.IsSuccess, Is.True);

            var queueRes = await _repository.GetTrainingQueueAsync();
            Assert.That(queueRes.Value.Count, Is.EqualTo(0));

            var rosterRes = await _repository.GetRosterAsync();
            Assert.That(rosterRes.Value.GetInTrainingCount(UnitType.FighterAircraft), Is.EqualTo(0));
        }

        [Test]
        public async Task RequestTrainingAsync_SimulateFailure_RejectsRequest()
        {
            _repository.SimulateFailure = true;

            var res = await _repository.RequestTrainingAsync(Guid.NewGuid().ToString("D"), UnitType.Tank, 1);
            Assert.That(res.IsFailure, Is.True);
            Assert.That(res.Error.Message.Contains("simulated failure"), Is.True);
        }
    }
}
