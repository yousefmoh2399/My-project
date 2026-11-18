using System.Collections;
using KingWorldWar.App;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.Networking;
using KingWorldWar.Infrastructure.Config;
using KingWorldWar.Infrastructure.Scenes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class BootstrapFlowTests
    {
        private GameObject _bootstrapGo;
        private GameBootstrap _bootstrap;

        [SetUp]
        public void SetUp()
        {
            _bootstrapGo = new GameObject("Test_Bootstrap");
            _bootstrapGo.SetActive(false);
            _bootstrap = _bootstrapGo.AddComponent<GameBootstrap>();
            _bootstrap.AutoRunOnStart = false;
        }

        [TearDown]
        public void TearDown()
        {
            if (_bootstrapGo != null)
            {
                Object.DestroyImmediate(_bootstrapGo);
            }
            ServiceContainer.Instance.Reset();
        }

        [UnityTest]
        public IEnumerator Bootstrap_WithValidConfig_InitializesSuccessfully()
        {
            var config = GameConfig.CreateInMemory(EnvironmentType.Dev, "0.1.0", "127.0.0.1", 7350);
            _bootstrap.Config = config;
            _bootstrap.SkipSceneTransitionForTest = true;
            _bootstrapGo.SetActive(true);

            var task = _bootstrap.RunBootstrapAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(task.Result);
            Assert.IsTrue(_bootstrap.IsInitialized);
            Assert.IsFalse(_bootstrap.HasFailed);

            // Verify services registered in ServiceContainer
            var container = ServiceContainer.Instance;
            Assert.IsTrue(container.TryResolve<GameConfig>(out var resolvedConfig));
            Assert.AreEqual(config, resolvedConfig);

            Assert.IsTrue(container.TryResolve<ILog>(out var resolvedLog));
            Assert.IsNotNull(resolvedLog);

            Assert.IsTrue(container.TryResolve<ISceneLoader>(out var resolvedLoader));
            Assert.IsNotNull(resolvedLoader);

            Assert.IsTrue(container.TryResolve<IAuthService>(out var resolvedAuth));
            Assert.IsNotNull(resolvedAuth);

            Assert.IsTrue(container.TryResolve<IPlayerRepository>(out var resolvedPlayer));
            Assert.IsNotNull(resolvedPlayer);
        }
    }
}
