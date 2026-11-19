using System;
using System.Reflection;
using KingWorldWar.Core.Economy;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class EconomySnapshotTests
    {
        private DateTime _baseTime;
        private StorageCapacity _caps;
        private ResourceYieldRate _rates;

        [SetUp]
        public void SetUp()
        {
            _baseTime = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
            _caps = new StorageCapacity(100000, 80000, 60000);
            _rates = new ResourceYieldRate(3600, 1800, 720); // 1/sec Gold, 0.5/sec Oil, 0.2/sec Iron
        }

        [Test]
        public void CalculateProjection_ElapsedTime_AccruesContinuously()
        {
            var snapshot = new EconomySnapshot(10000, 5000, 2000, _caps, _rates, _baseTime, 1);

            // 1 hour elapsed (3600 seconds)
            var targetTime = _baseTime.AddHours(1);
            var projected = snapshot.CalculateProjection(targetTime);

            Assert.AreEqual(10000 + 3600, projected.Gold);
            Assert.AreEqual(5000 + 1800, projected.Oil);
            Assert.AreEqual(2000 + 720, projected.Iron);
            Assert.AreEqual(1, projected.SnapshotVersion);
        }

        [Test]
        public void CalculateProjection_ExceedingStorageCap_ClampsToCap()
        {
            // Close to cap
            var snapshot = new EconomySnapshot(99000, 79000, 59500, _caps, _rates, _baseTime, 1);

            // 10 hours elapsed -> accrual would vastly exceed cap
            var targetTime = _baseTime.AddHours(10);
            var projected = snapshot.CalculateProjection(targetTime);

            Assert.AreEqual(_caps.GoldCap, projected.Gold);
            Assert.AreEqual(_caps.OilCap, projected.Oil);
            Assert.AreEqual(_caps.IronCap, projected.Iron);
        }

        [Test]
        public void CalculateProjection_PastOrSameTimestamp_ReturnsUnchanged()
        {
            var snapshot = new EconomySnapshot(10000, 5000, 2000, _caps, _rates, _baseTime, 1);

            var same = snapshot.CalculateProjection(_baseTime);
            Assert.AreEqual(10000, same.Gold);

            var past = snapshot.CalculateProjection(_baseTime.AddMinutes(-10));
            Assert.AreEqual(10000, past.Gold);
        }

        [Test]
        public void Validate_NegativeBalances_ReturnsInvalidPayloadFailure()
        {
            var res = EconomySnapshot.Validate(-10, 500, 500, _caps, _rates, _baseTime, 1);
            Assert.IsTrue(res.IsFailure);
            Assert.AreEqual("INVALID_PAYLOAD", res.Error.Code);
        }

        [Test]
        public void Validate_DefaultTimestamp_ReturnsInvalidPayloadFailure()
        {
            var res = EconomySnapshot.Validate(100, 100, 100, _caps, _rates, default, 1);
            Assert.IsTrue(res.IsFailure);
            Assert.AreEqual("INVALID_PAYLOAD", res.Error.Code);
        }

        [Test]
        public void CanAffordAndShortfall_CalculatesAccurately()
        {
            var snapshot = new EconomySnapshot(1000, 500, 200, _caps, _rates, _baseTime, 1);

            Assert.IsTrue(snapshot.CanAfford(500, 250, 100));
            Assert.IsFalse(snapshot.CanAfford(1500, 250, 100));

            var shortfall = snapshot.GetShortfall(1500, 800, 150);
            Assert.AreEqual(500, shortfall.GoldShortfall);
            Assert.AreEqual(300, shortfall.OilShortfall);
            Assert.AreEqual(0, shortfall.IronShortfall);
        }

        [Test]
        public void Immutability_SnapshotAndResourceAmount_HaveNoPublicSetters_ClientHasNoWritePath()
        {
            // Acceptance Criteria 2: The client cannot set arbitrary resource balances.
            // Verify via reflection that EconomySnapshot has ZERO public setters or mutator methods.
            var snapshotProps = typeof(EconomySnapshot).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in snapshotProps)
            {
                Assert.IsNull(prop.GetSetMethod(), $"EconomySnapshot property '{prop.Name}' must NOT have a public setter.");
            }

            var amountProps = typeof(ResourceAmount).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in amountProps)
            {
                Assert.IsNull(prop.GetSetMethod(), $"ResourceAmount property '{prop.Name}' must NOT have a public setter.");
            }
        }
    }
}
