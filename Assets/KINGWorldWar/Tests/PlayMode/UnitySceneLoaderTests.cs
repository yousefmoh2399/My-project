using System;
using System.Collections;
using System.Text.RegularExpressions;
using KingWorldWar.App;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Logging;
using KingWorldWar.Infrastructure.Scenes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class UnitySceneLoaderTests
    {
        private UnitySceneLoader _loader;
        private ILog _log;

        [SetUp]
        public void SetUp()
        {
            _log = new GameLog(new UnityLogSink(), LogLevel.Trace);
            _loader = new UnitySceneLoader(_log);
        }

        [Test]
        public void Constructor_NullLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new UnitySceneLoader(null));
        }

        [Test]
        public void CurrentSceneName_ReturnsActiveSceneName()
        {
            Assert.AreEqual(SceneManager.GetActiveScene().name, _loader.CurrentSceneName);
        }

        [Test]
        public void IsLoading_Initially_IsFalse()
        {
            Assert.IsFalse(_loader.IsLoading);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_UnregisteredScene_ReturnsSceneNotFoundFailure()
        {
            const string missingScene = "NonExistentScene_XYZ_999";
            LogAssert.Expect(LogType.Error, new Regex(@".*Scene 'NonExistentScene_XYZ_999' is not registered in Build Settings.*"));

            var task = _loader.LoadSceneAsync(missingScene);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            Result<GameError> result = task.Result;

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("SCENE_NOT_FOUND", result.Error.Code);
            Assert.AreEqual(missingScene, result.Error.Details);
            Assert.IsFalse(_loader.IsLoading);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_NullOrEmptySceneName_ReturnsSceneLoadFailed()
        {
            LogAssert.Expect(LogType.Error, new Regex(@".*Scene name cannot be empty.*"));

            var task = _loader.LoadSceneAsync(string.Empty);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            Result<GameError> result = task.Result;

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("SCENE_LOAD_FAILED", result.Error.Code);
            Assert.IsFalse(_loader.IsLoading);
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_WhitespaceSceneName_ReturnsSceneLoadFailed()
        {
            LogAssert.Expect(LogType.Error, new Regex(@".*Scene name cannot be empty.*"));

            var task = _loader.LoadSceneAsync("   ");
            while (!task.IsCompleted)
            {
                yield return null;
            }

            Result<GameError> result = task.Result;

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("SCENE_LOAD_FAILED", result.Error.Code);
            Assert.IsFalse(_loader.IsLoading);
        }
    }
}
