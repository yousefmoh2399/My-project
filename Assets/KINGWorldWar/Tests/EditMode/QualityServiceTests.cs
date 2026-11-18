using System;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.Quality;
using KingWorldWar.Infrastructure.Quality;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class QualityServiceTests
    {
        private class DummyLog : ILog
        {
            public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;
            public void Trace(string category, string message) { }
            public void Debug(string category, string message) { }
            public void Info(string category, string message) { }
            public void Warning(string category, string message, Exception exception = null) { }
            public void Error(string category, string message, Exception exception = null) { }
        }

        private QualityService _service;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(QualityService.QualityPrefKey);
            _service = new QualityService(new DummyLog());
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(QualityService.QualityPrefKey);
        }

        [Test]
        public void Constructor_NullLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new QualityService(null));
        }

        [Test]
        public void EvaluateOptimalTier_LowRam_ReturnsLow()
        {
            // 2GB RAM device
            var tier = _service.EvaluateOptimalTier(systemMemoryMb: 2048, graphicsMemoryMb: 1500, processorCount: 8);
            Assert.AreEqual(QualityTier.Low, tier);
        }

        [Test]
        public void EvaluateOptimalTier_LowGpuVram_ReturnsLow()
        {
            // 4GB RAM device with low VRAM
            var tier = _service.EvaluateOptimalTier(systemMemoryMb: 4096, graphicsMemoryMb: 512, processorCount: 8);
            Assert.AreEqual(QualityTier.Low, tier);
        }

        [Test]
        public void EvaluateOptimalTier_QuadCoreCpu_ReturnsLow()
        {
            // 4-core CPU
            var tier = _service.EvaluateOptimalTier(systemMemoryMb: 4096, graphicsMemoryMb: 2048, processorCount: 4);
            Assert.AreEqual(QualityTier.Low, tier);
        }

        [Test]
        public void EvaluateOptimalTier_FlagshipSpec_ReturnsHigh()
        {
            // 8GB RAM, 3GB VRAM, 8-core CPU
            var tier = _service.EvaluateOptimalTier(systemMemoryMb: 8192, graphicsMemoryMb: 3072, processorCount: 8);
            Assert.AreEqual(QualityTier.High, tier);
        }

        [Test]
        public void EvaluateOptimalTier_MidrangeSpec_ReturnsMedium()
        {
            // 4GB RAM, 1.5GB VRAM, 8-core CPU
            var tier = _service.EvaluateOptimalTier(systemMemoryMb: 4096, graphicsMemoryMb: 1536, processorCount: 8);
            Assert.AreEqual(QualityTier.Medium, tier);
        }

        [Test]
        public void SetQualityTier_UpdatesCurrentTierAndDispatchesEvent()
        {
            QualityTier observedTier = QualityTier.Low;
            int eventCount = 0;

            _service.OnQualityTierChanged += (tier) =>
            {
                observedTier = tier;
                eventCount++;
            };

            _service.SetQualityTier(QualityTier.High, persist: true);

            Assert.AreEqual(QualityTier.High, _service.CurrentTier);
            Assert.AreEqual(QualityTier.High, observedTier);
            Assert.AreEqual(1, eventCount);
            Assert.AreEqual((int)QualityTier.High, PlayerPrefs.GetInt(QualityService.QualityPrefKey));
        }
    }
}
