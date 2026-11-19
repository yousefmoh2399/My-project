using KingWorldWar.Core.Base;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Infrastructure.Base;
using KingWorldWar.Infrastructure.Localization;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class BuildingLocalizationTests
    {
        private LocalizationService _loc;
        private IBuildingDataSource _dataSource;

        [SetUp]
        public void SetUp()
        {
            _loc = new LocalizationService(GameLog.Default);
            var config = UnityEngine.ScriptableObject.CreateInstance<BaseConfigSO>();
            config.InitializeFromDefaults();
            _dataSource = config;
        }

        [Test]
        public void StringTables_StrictKeyParityAcrossLanguages()
        {
            var en = DefaultStringTables.EnglishTable;
            var ar = DefaultStringTables.ArabicTable;

            Assert.AreEqual(en.Count, ar.Count, "English and Arabic tables must have exact same number of keys.");

            foreach (var key in en.Keys)
            {
                Assert.IsTrue(ar.ContainsKey(key), $"Arabic string table missing key: '{key}'");
                Assert.IsFalse(string.IsNullOrWhiteSpace(ar[key]), $"Arabic translation empty for key: '{key}'");
            }
        }

        [Test]
        public void AllBuildingDefinitions_HaveLocalizedNamesAndDescriptions()
        {
            var defs = _dataSource.GetAllDefinitions();
            foreach (var def in defs)
            {
                // English
                _loc.SetLanguage(Language.English, persist: false);
                string enName = _loc.GetString(def.NameLocalizationKey);
                string enDesc = _loc.GetString(def.DescriptionLocalizationKey);

                Assert.IsFalse(enName.StartsWith("[key:"), $"Missing English translation for: {def.NameLocalizationKey}");
                Assert.IsFalse(enDesc.StartsWith("[key:"), $"Missing English translation for: {def.DescriptionLocalizationKey}");

                // Arabic
                _loc.SetLanguage(Language.Arabic, persist: false);
                string arName = _loc.GetString(def.NameLocalizationKey);
                string arDesc = _loc.GetString(def.DescriptionLocalizationKey);

                Assert.IsFalse(arName.StartsWith("[key:"), $"Missing Arabic translation for: {def.NameLocalizationKey}");
                Assert.IsFalse(arDesc.StartsWith("[key:"), $"Missing Arabic translation for: {def.DescriptionLocalizationKey}");
                Assert.IsTrue(ArabicTextShaper.ContainsArabic(arName), $"Arabic name '{arName}' must contain Arabic characters.");
            }
        }

        [Test]
        public void BaseDrawerKeys_ResolveInBothLanguages()
        {
            string[] drawerKeys = new[]
            {
                "base.drawer.title",
                "base.drawer.level",
                "base.drawer.max_level",
                "base.drawer.upgrade_cost",
                "base.drawer.upgrade_time",
                "base.drawer.req_hq",
                "base.drawer.req_hq_met",
                "base.drawer.action_upgrade",
                "base.drawer.action_cancel",
                "base.drawer.upgrading",
                "base.drawer.time_remaining",
                "base.drawer.current_perk",
                "base.drawer.next_perk",
                "base.drawer.mock_badge",
                "base.cancel_dialog.title",
                "base.cancel_dialog.msg"
            };

            foreach (var key in drawerKeys)
            {
                _loc.SetLanguage(Language.English, persist: false);
                string en = _loc.GetString(key);
                Assert.IsFalse(en.StartsWith("[key:"), $"Missing English key: {key}");

                _loc.SetLanguage(Language.Arabic, persist: false);
                string ar = _loc.GetString(key);
                Assert.IsFalse(ar.StartsWith("[key:"), $"Missing Arabic key: {key}");
            }
        }
    }
}
