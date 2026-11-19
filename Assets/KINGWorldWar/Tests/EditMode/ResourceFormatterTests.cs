using KingWorldWar.Core.Economy;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ResourceFormatterTests
    {
        [Test]
        public void Format_Zero_ReturnsZeroString()
        {
            Assert.AreEqual("0", ResourceFormatter.Format(0));
        }

        [Test]
        public void Format_NegativeValue_ReturnsZeroString()
        {
            Assert.AreEqual("0", ResourceFormatter.Format(-500));
            Assert.AreEqual("0", ResourceFormatter.Format(-1));
        }

        [Test]
        public void Format_UnderThousand_ReturnsExactInteger()
        {
            Assert.AreEqual("1", ResourceFormatter.Format(1));
            Assert.AreEqual("450", ResourceFormatter.Format(450));
            Assert.AreEqual("999", ResourceFormatter.Format(999));
        }

        [Test]
        public void Format_Thousands_FormatsWithKSuffix()
        {
            Assert.AreEqual("1K", ResourceFormatter.Format(1000));
            Assert.AreEqual("1.25K", ResourceFormatter.Format(1250));
            Assert.AreEqual("10K", ResourceFormatter.Format(10000));
            Assert.AreEqual("48.5K", ResourceFormatter.Format(48500));
            Assert.AreEqual("999.99K", ResourceFormatter.Format(999990));
        }

        [Test]
        public void Format_Millions_FormatsWithMSuffix()
        {
            Assert.AreEqual("1M", ResourceFormatter.Format(1_000_000));
            Assert.AreEqual("1.25M", ResourceFormatter.Format(1_250_000));
            Assert.AreEqual("3.4M", ResourceFormatter.Format(3_400_000));
            Assert.AreEqual("999.99M", ResourceFormatter.Format(999_990_000));
        }

        [Test]
        public void Format_MaxLong_DoesNotOverflow_FormatsSafely()
        {
            string result = ResourceFormatter.Format(long.MaxValue);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.EndsWith("B"));
        }

        [Test]
        public void FormatExact_FormatsWithThousandsSeparators()
        {
            Assert.AreEqual("0", ResourceFormatter.FormatExact(0));
            Assert.AreEqual("1,250", ResourceFormatter.FormatExact(1250));
            Assert.AreEqual("3,400,000", ResourceFormatter.FormatExact(3400000));
        }

        [Test]
        public void EvaluateVisualState_Zero_ReturnsZero()
        {
            var state = ResourceFormatter.EvaluateVisualState(0, 10000);
            Assert.AreEqual(ResourceVisualState.Zero, state);
        }

        [Test]
        public void EvaluateVisualState_UnderNinetyPercent_ReturnsNormal()
        {
            var state = ResourceFormatter.EvaluateVisualState(8000, 10000);
            Assert.AreEqual(ResourceVisualState.Normal, state);
        }

        [Test]
        public void EvaluateVisualState_NinetyPercentOrAbove_ReturnsNearCap()
        {
            var state = ResourceFormatter.EvaluateVisualState(9000, 10000);
            Assert.AreEqual(ResourceVisualState.NearCap, state);

            var state2 = ResourceFormatter.EvaluateVisualState(9500, 10000);
            Assert.AreEqual(ResourceVisualState.NearCap, state2);
        }

        [Test]
        public void EvaluateVisualState_AtOrAboveCapacity_ReturnsCapped()
        {
            var state = ResourceFormatter.EvaluateVisualState(10000, 10000);
            Assert.AreEqual(ResourceVisualState.Capped, state);

            var stateOverflow = ResourceFormatter.EvaluateVisualState(12000, 10000);
            Assert.AreEqual(ResourceVisualState.Capped, stateOverflow);
        }

        [Test]
        public void EvaluateVisualState_IsStale_ReturnsStale()
        {
            var state = ResourceFormatter.EvaluateVisualState(5000, 10000, isStale: true);
            Assert.AreEqual(ResourceVisualState.Stale, state);
        }

        [Test]
        public void FormatTimeToFull_Capped_ReturnsFull()
        {
            Assert.AreEqual("Full", ResourceFormatter.FormatTimeToFull(10000, 10000, 1000));
        }

        [Test]
        public void FormatTimeToFull_CalculatesAccurateSpan()
        {
            // 2000 needed at 1000/hr = 2 hours
            string time = ResourceFormatter.FormatTimeToFull(8000, 10000, 1000);
            Assert.AreEqual("2h 0m", time);
        }
    }
}
