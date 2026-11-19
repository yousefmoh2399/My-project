using System;
using System.Threading.Tasks;
using KingWorldWar.Core.Economy;
using KingWorldWar.Infrastructure.Economy;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class EconomyRepositoryTests
    {
        private LocalMockEconomyRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _repo = new LocalMockEconomyRepository(
                initialGold: 10000,
                initialOil: 5000,
                initialIron: 2000,
                storageCaps: new StorageCapacity(50000, 50000, 50000),
                productionRates: new ResourceYieldRate(1200, 800, 600))
            {
                SimulatedLatencyMs = 0
            };
        }

        [Test]
        public async Task GetSnapshotAsync_ReturnsValidInitialSnapshot_MarkedAsMock()
        {
            Assert.IsTrue(_repo.IsMock);

            var res = await _repo.GetSnapshotAsync();
            Assert.IsTrue(res.IsSuccess);

            var snapshot = res.Value;
            Assert.GreaterOrEqual(snapshot.Gold, 10000);
            Assert.GreaterOrEqual(snapshot.Oil, 5000);
            Assert.GreaterOrEqual(snapshot.Iron, 2000);
            Assert.AreEqual(1, snapshot.SnapshotVersion);
        }

        [Test]
        public async Task RequestSpend_ValidAmount_DebitsBalancesAndIncrementsVersion()
        {
            string reqId = Guid.NewGuid().ToString("N");
            var spendRes = await _repo.RequestSpendAsync(reqId, 2000, 1000, 500, "TEST_SPEND", 1);

            Assert.IsTrue(spendRes.IsSuccess);
            var tx = spendRes.Value;
            Assert.IsTrue(tx.Success);
            Assert.AreEqual(2, tx.AuthoritativeSnapshot.SnapshotVersion);
            Assert.AreEqual(reqId, tx.RequestId);

            // Re-fetch snapshot to verify persistence
            var snapshotRes = await _repo.GetSnapshotAsync();
            Assert.AreEqual(tx.AuthoritativeSnapshot.SnapshotVersion, snapshotRes.Value.SnapshotVersion);
        }

        [Test]
        public async Task RequestSpend_DuplicateRequestId_ReturnsCachedResult_DoesNotSpendTwice()
        {
            string reqId = "idemp_key_12345";

            // First spend
            var res1 = await _repo.RequestSpendAsync(reqId, 1000, 500, 200, "FIRST_ATTEMPT", 1);
            Assert.IsTrue(res1.IsSuccess);
            long goldAfterFirst = res1.Value.AuthoritativeSnapshot.Gold;
            long verAfterFirst = res1.Value.AuthoritativeSnapshot.SnapshotVersion;

            // Replay identical requestId with older expectedVersion (simulating retry)
            var res2 = await _repo.RequestSpendAsync(reqId, 1000, 500, 200, "REPLAY_ATTEMPT", 1);
            Assert.IsTrue(res2.IsSuccess);

            // Verified: Exactly identical transaction returned without double debit
            Assert.AreEqual(res1.Value.TransactionId, res2.Value.TransactionId);
            Assert.AreEqual(goldAfterFirst, res2.Value.AuthoritativeSnapshot.Gold);
            Assert.AreEqual(verAfterFirst, res2.Value.AuthoritativeSnapshot.SnapshotVersion);
        }

        [Test]
        public async Task RequestSpend_InsufficientFunds_FailsWithInsufficientResources()
        {
            string reqId = Guid.NewGuid().ToString("N");
            // Request 999,999 Gold when balance is 10,000
            var res = await _repo.RequestSpendAsync(reqId, 999999, 100, 100, "EXCESSIVE_SPEND", 1);

            Assert.IsTrue(res.IsFailure);
            Assert.AreEqual("INSUFFICIENT_RESOURCES", res.Error.Code);
            Assert.IsTrue(res.Error.Details.Contains("Shortfall"));
        }

        [Test]
        public async Task RequestSpend_StaleVersion_FailsWithStaleVersion()
        {
            // Advance version once
            await _repo.RequestSpendAsync(Guid.NewGuid().ToString("N"), 500, 200, 100, "ADVANCE", 1);

            // Now attempt spend with stale version 1 (current is 2)
            var res = await _repo.RequestSpendAsync(Guid.NewGuid().ToString("N"), 500, 200, 100, "STALE_SPEND", 1);
            Assert.IsTrue(res.IsFailure);
            Assert.AreEqual("STALE_VERSION", res.Error.Code);
        }

        [Test]
        public async Task EconomyService_RefreshesSnapshot_CalculatesProjectionAndCanAfford()
        {
            var service = new EconomyService(_repo);
            var refRes = await service.RefreshSnapshotAsync();

            Assert.IsTrue(refRes.IsSuccess);
            Assert.IsFalse(service.IsStale);
            Assert.GreaterOrEqual(service.Gold, 10000);

            Assert.IsTrue(service.CanAfford(5000, 2000, 1000));
            Assert.IsFalse(service.CanAfford(50000, 2000, 1000));

            var shortfall = service.GetShortfall(50000, 2000, 1000);
            Assert.Greater(shortfall.GoldShortfall, 0);
        }
    }
}
