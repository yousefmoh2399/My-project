using System;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Infrastructure.Localization;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class LocalizationServiceTests
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

        private LocalizationService _service;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(LocalizationService.LanguagePrefKey);
            _service = new LocalizationService(new DummyLog());
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(LocalizationService.LanguagePrefKey);
        }

        [Test]
        public void StringTables_StrictKeyParity_ArabicAndEnglishMustMatchExactly()
        {
            var enKeys = DefaultStringTables.EnglishTable.Keys;
            var arKeys = DefaultStringTables.ArabicTable.Keys;

            foreach (var key in enKeys)
            {
                Assert.IsTrue(DefaultStringTables.ArabicTable.ContainsKey(key), $"Missing Arabic translation for key: '{key}'");
                Assert.IsFalse(string.IsNullOrWhiteSpace(DefaultStringTables.ArabicTable[key]), $"Arabic translation is empty for key: '{key}'");
            }

            foreach (var key in arKeys)
            {
                Assert.IsTrue(DefaultStringTables.EnglishTable.ContainsKey(key), $"Missing English translation for key: '{key}'");
                Assert.IsFalse(string.IsNullOrWhiteSpace(DefaultStringTables.EnglishTable[key]), $"English translation is empty for key: '{key}'");
            }

            Assert.AreEqual(enKeys.Count, arKeys.Count, "Both string tables must have identical key count.");
        }

        [Test]
        public void GetString_English_ResolvesCorrectly()
        {
            _service.SetLanguage(Language.English, persist: false);
            string val = _service.GetString("nav.world");
            Assert.AreEqual("World", val);
        }

        [Test]
        public void GetString_Arabic_ShapesAndResolves()
        {
            _service.SetLanguage(Language.Arabic, persist: false);
            string val = _service.GetString("nav.world");
            Assert.IsNotNull(val);
            Assert.IsTrue(ArabicTextShaper.ContainsArabic(val));
        }

        [Test]
        public void GetString_MissingKey_ReturnsMissingKeyTag()
        {
            _service.SetLanguage(Language.English, persist: false);
            string val = _service.GetString("non.existent.key.123");
            Assert.AreEqual("[missing:non.existent.key.123]", val);
        }

        [Test]
        public void GetString_WithArguments_FormatsProperly()
        {
            _service.SetLanguage(Language.English, persist: false);
            string formatted = _service.GetString("common.level_format", 15);
            Assert.AreEqual("Level 15", formatted);
        }

        [Test]
        public void SetLanguage_PersistsAndDispatchesEvent()
        {
            Language observed = Language.English;
            int eventCount = 0;

            _service.OnLanguageChanged += (lang) =>
            {
                observed = lang;
                eventCount++;
            };

            _service.SetLanguage(Language.English, persist: true);
            Assert.AreEqual(Language.English, _service.CurrentLanguage);
            Assert.AreEqual(Language.English, observed);
            Assert.AreEqual(1, eventCount);
            Assert.AreEqual((int)Language.English, PlayerPrefs.GetInt(LocalizationService.LanguagePrefKey));
        }
    }
}
