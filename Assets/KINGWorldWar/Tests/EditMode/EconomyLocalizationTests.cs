using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Infrastructure.Localization;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class EconomyLocalizationTests
    {
        private LocalizationService _loc;

        [SetUp]
        public void SetUp()
        {
            _loc = new LocalizationService(GameLog.Default);
        }

        [Test]
        public void EconomyKeys_StrictKeyParity_AndNonEmptyTranslations()
        {
            string[] economyKeys = {
                "res.gold",
                "res.oil",
                "res.iron",
                "res.rate_per_hour",
                "res.capacity",
                "res.time_to_full",
                "res.projection_estimate",
                "res.shortfall",
                "res.status.normal",
                "res.status.near_cap",
                "res.status.capped",
                "res.status.stale",
                "res.drawer.title",
                "res.drawer.rates_header",
                "res.drawer.caps_header",
                "res.drawer.btn_refresh",
                "res.insufficient.gold",
                "res.insufficient.oil",
                "res.insufficient.iron"
            };

            foreach (var key in economyKeys)
            {
                // English
                _loc.SetLanguage(Language.English, persist: false);
                string en = _loc.GetString(key);
                Assert.IsFalse(en.StartsWith("[key:"), $"Missing English localization for '{key}'");
                Assert.IsFalse(string.IsNullOrWhiteSpace(en), $"Empty English string for '{key}'");

                // Arabic
                _loc.SetLanguage(Language.Arabic, persist: false);
                string ar = _loc.GetString(key);
                Assert.IsFalse(ar.StartsWith("[key:"), $"Missing Arabic localization for '{key}'");
                Assert.IsFalse(string.IsNullOrWhiteSpace(ar), $"Empty Arabic string for '{key}'");
            }
        }
    }
}
