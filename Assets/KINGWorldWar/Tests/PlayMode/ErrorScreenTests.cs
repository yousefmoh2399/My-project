using System.Collections;
using KingWorldWar.App;
using KingWorldWar.Infrastructure.Config;
using KingWorldWar.UI.ErrorScreen;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class ErrorScreenTests
    {
        private GameObject _holderGo;
        private GameBootstrap _bootstrap;
        private ErrorScreenView _errorScreen;

        [SetUp]
        public void SetUp()
        {
            _holderGo = new GameObject("Test_ErrorScreenHolder");
            _holderGo.SetActive(false);
            _bootstrap = _holderGo.AddComponent<GameBootstrap>();
            _bootstrap.AutoRunOnStart = false;

            var errorGo = new GameObject("ErrorScreenView");
            errorGo.transform.SetParent(_holderGo.transform);
            _errorScreen = errorGo.AddComponent<ErrorScreenView>();
            errorGo.SetActive(false);

            _bootstrap.ErrorScreen = _errorScreen;
            _holderGo.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_holderGo != null)
            {
                Object.DestroyImmediate(_holderGo);
            }
            ServiceContainer.Instance.Reset();
        }

        [UnityTest]
        public IEnumerator Bootstrap_MissingConfig_TriggersErrorScreen()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*Configuration asset is missing.*"));

            _bootstrap.Config = null;
            _bootstrap.SkipSceneTransitionForTest = true;

            var task = _bootstrap.RunBootstrapAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsFalse(task.Result);
            Assert.IsTrue(_bootstrap.HasFailed);
            Assert.AreEqual("CONFIG_MISSING", _bootstrap.LastError?.Code);
            Assert.IsTrue(_errorScreen.IsVisible);
        }

        [UnityTest]
        public IEnumerator Bootstrap_MaintenanceMode_TriggersMaintenanceScreen()
        {
            var config = GameConfig.CreateInMemory(
                EnvironmentType.Dev,
                version: "0.1.0",
                host: "127.0.0.1",
                port: 7350,
                maintenance: true);

            _bootstrap.Config = config;
            _bootstrap.SkipSceneTransitionForTest = true;

            var task = _bootstrap.RunBootstrapAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsFalse(task.Result);
            Assert.IsTrue(_bootstrap.HasFailed);
            Assert.AreEqual("MAINTENANCE_ACTIVE", _bootstrap.LastError?.Code);
            Assert.IsTrue(_errorScreen.IsVisible);
        }

        [Test]
        public void ErrorScreenView_ShowAndHide_TogglesVisibility()
        {
            _errorScreen.Show("TITLE", "Message", "ERR_123");
            Assert.IsTrue(_errorScreen.IsVisible);

            _errorScreen.Hide();
            Assert.IsFalse(_errorScreen.IsVisible);
        }
    }
}
