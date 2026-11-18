using System.Collections;
using KingWorldWar.Features.Camera;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class StrategyCameraRigTests
    {
        private GameObject _rigGo;
        private StrategyCameraRig _rig;
        private CameraConfigSO _config;

        [SetUp]
        public void SetUp()
        {
            _rigGo = new GameObject("Test_StrategyCameraRig");
            _config = CameraConfigSO.CreateDefaultInstance();
            _rig = _rigGo.AddComponent<StrategyCameraRig>();
            _rig.Config = _config;
        }

        [TearDown]
        public void TearDown()
        {
            if (_rigGo != null)
            {
                Object.DestroyImmediate(_rigGo);
            }
        }

        [Test]
        public void Initialization_AppliesDefaultZoomAndPositionWithinBounds()
        {
            Assert.AreEqual(_config.DefaultZoom, _rig.CurrentZoom);
            Assert.LessOrEqual(_rig.FocusPosition.x, _config.MaxBounds.x);
            Assert.GreaterOrEqual(_rig.FocusPosition.x, _config.MinBounds.x);
            Assert.LessOrEqual(_rig.FocusPosition.z, _config.MaxBounds.y);
            Assert.GreaterOrEqual(_rig.FocusPosition.z, _config.MinBounds.y);
        }

        [Test]
        public void SetFocusPosition_ClampedToBounds()
        {
            _rig.SetFocusPosition(new Vector3(9999f, 0f, 9999f), immediate: true);

            Assert.AreEqual(_config.MaxBounds.x, _rig.FocusPosition.x);
            Assert.AreEqual(_config.MaxBounds.y, _rig.FocusPosition.z);

            _rig.SetFocusPosition(new Vector3(-9999f, 0f, -9999f), immediate: true);

            Assert.AreEqual(_config.MinBounds.x, _rig.FocusPosition.x);
            Assert.AreEqual(_config.MinBounds.y, _rig.FocusPosition.z);
        }

        [Test]
        public void SetZoom_ClampedToMinMax()
        {
            _rig.SetZoom(_config.MinZoom - 100f, immediate: true);
            Assert.AreEqual(_config.MinZoom, _rig.CurrentZoom);

            _rig.SetZoom(_config.MaxZoom + 100f, immediate: true);
            Assert.AreEqual(_config.MaxZoom, _rig.CurrentZoom);
        }

        [UnityTest]
        public IEnumerator UpdateMovement_SmoothlyMovesTowardTarget()
        {
            _rig.SetFocusPosition(Vector3.zero, immediate: true);
            Assert.AreEqual(0f, _rig.FocusPosition.x);

            _rig.SetFocusPosition(new Vector3(20f, 0f, 20f), immediate: false);

            // Step a few frames
            for (int i = 0; i < 5; i++)
            {
                yield return null;
            }

            Assert.Greater(_rig.FocusPosition.x, 0f);
            Assert.LessOrEqual(_rig.FocusPosition.x, 20f);
        }
    }
}
