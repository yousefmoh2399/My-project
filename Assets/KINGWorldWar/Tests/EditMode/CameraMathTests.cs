using KingWorldWar.Core.Camera;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class CameraMathTests
    {
        [Test]
        public void ClampPosition_InsideBounds_ReturnsUnmodified()
        {
            var pos = new Vector3(10f, 25f, -5f);
            var min = new Vector2(-50f, -50f);
            var max = new Vector2(50f, 50f);

            var clamped = CameraMath.ClampPosition(pos, min, max);

            Assert.AreEqual(10f, clamped.x);
            Assert.AreEqual(25f, clamped.y, "Y axis should be preserved");
            Assert.AreEqual(-5f, clamped.z);
        }

        [Test]
        public void ClampPosition_OutsideBounds_ClampsToLimits()
        {
            var pos = new Vector3(120f, 15f, -90f);
            var min = new Vector2(-50f, -40f);
            var max = new Vector2(60f, 40f);

            var clamped = CameraMath.ClampPosition(pos, min, max);

            Assert.AreEqual(60f, clamped.x);
            Assert.AreEqual(15f, clamped.y);
            Assert.AreEqual(-40f, clamped.z);
        }

        [Test]
        public void ClampPosition_InvertedMinMax_HandlesGracefully()
        {
            var pos = new Vector3(100f, 0f, -100f);
            var min = new Vector2(50f, 50f);
            var max = new Vector2(-50f, -50f);

            var clamped = CameraMath.ClampPosition(pos, min, max);

            Assert.AreEqual(50f, clamped.x);
            Assert.AreEqual(-50f, clamped.z);
        }

        [Test]
        public void ClampZoom_BetweenLimits_ReturnsValue()
        {
            float zoom = CameraMath.ClampZoom(25f, 10f, 60f);
            Assert.AreEqual(25f, zoom);
        }

        [Test]
        public void ClampZoom_BelowMin_ClampsToMin()
        {
            float zoom = CameraMath.ClampZoom(5f, 15f, 50f);
            Assert.AreEqual(15f, zoom);
        }

        [Test]
        public void ClampZoom_AboveMax_ClampsToMax()
        {
            float zoom = CameraMath.ClampZoom(100f, 15f, 50f);
            Assert.AreEqual(50f, zoom);
        }

        [Test]
        public void CalculatePinchRatio_SpreadApart_ReturnsGreaterThanOne()
        {
            var p0 = new Vector2(0, 0);
            var p1 = new Vector2(100, 0);

            var c0 = new Vector2(-20, 0);
            var c1 = new Vector2(120, 0);

            float ratio = CameraMath.CalculatePinchRatio(p0, c0, p1, c1);

            Assert.Greater(ratio, 1.0f);
            Assert.AreEqual(1.4f, ratio, 0.001f);
        }

        [Test]
        public void CalculatePinchRatio_PinchTogether_ReturnsLessThanOne()
        {
            var p0 = new Vector2(0, 0);
            var p1 = new Vector2(100, 0);

            var c0 = new Vector2(20, 0);
            var c1 = new Vector2(80, 0);

            float ratio = CameraMath.CalculatePinchRatio(p0, c0, p1, c1);

            Assert.Less(ratio, 1.0f);
            Assert.AreEqual(0.6f, ratio, 0.001f);
        }

        [Test]
        public void CalculatePinchRatio_ZeroDistance_ReturnsOne()
        {
            var p0 = new Vector2(0, 0);
            var p1 = new Vector2(0, 0);
            var c0 = new Vector2(0, 0);
            var c1 = new Vector2(0, 0);

            float ratio = CameraMath.CalculatePinchRatio(p0, c0, p1, c1);

            Assert.AreEqual(1.0f, ratio);
        }
    }
}
