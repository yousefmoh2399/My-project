using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Localization;
using KingWorldWar.Infrastructure.Army;
using KingWorldWar.Infrastructure.Localization;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ArmyLocalizationTests
    {
        [Test]
        public void StringTables_StrictKeyParity_ArmyKeys()
        {
            var enTable = DefaultStringTables.EnglishTable;
            var arTable = DefaultStringTables.ArabicTable;

            var missingInAr = new List<string>();
            var missingInEn = new List<string>();

            foreach (var key in enTable.Keys)
            {
                if (key.StartsWith("unit.") || key.StartsWith("army."))
                {
                    if (!arTable.ContainsKey(key)) missingInAr.Add(key);
                }
            }

            foreach (var key in arTable.Keys)
            {
                if (key.StartsWith("unit.") || key.StartsWith("army."))
                {
                    if (!enTable.ContainsKey(key)) missingInEn.Add(key);
                }
            }

            Assert.That(missingInAr, Is.Empty, $"Army keys in English missing in Arabic: {string.Join(", ", missingInAr)}");
            Assert.That(missingInEn, Is.Empty, $"Army keys in Arabic missing in English: {string.Join(", ", missingInEn)}");
        }

        [Test]
        public void AllUnitDefinitions_HaveValidLocalization()
        {
            var defs = DefaultArmyConfig.CreateDefaultDefinitions();
            var enTable = DefaultStringTables.EnglishTable;
            var arTable = DefaultStringTables.ArabicTable;

            foreach (var def in defs)
            {
                Assert.That(enTable.ContainsKey(def.NameKey), Is.True, $"Missing EN name for {def.Id}");
                Assert.That(arTable.ContainsKey(def.NameKey), Is.True, $"Missing AR name for {def.Id}");

                Assert.That(enTable.ContainsKey(def.DescKey), Is.True, $"Missing EN desc for {def.Id}");
                Assert.That(arTable.ContainsKey(def.DescKey), Is.True, $"Missing AR desc for {def.Id}");

                Assert.That(enTable.ContainsKey(def.RoleKey), Is.True, $"Missing EN role for {def.Id}");
                Assert.That(arTable.ContainsKey(def.RoleKey), Is.True, $"Missing AR role for {def.Id}");

                // Confirm Arabic text contains actual Arabic characters
                Assert.That(ArabicTextShaper.ContainsArabic(arTable[def.NameKey]), Is.True, $"Arabic name for {def.Id} has no Arabic chars");
                Assert.That(ArabicTextShaper.ContainsArabic(arTable[def.DescKey]), Is.True, $"Arabic desc for {def.Id} has no Arabic chars");
            }
        }

        [Test]
        public void ArmyDrawerAndScreenKeys_ResolveInBothLanguages()
        {
            var enTable = DefaultStringTables.EnglishTable;
            var arTable = DefaultStringTables.ArabicTable;

            string[] requiredKeys =
            {
                "army.screen.title",
                "army.screen.total_power",
                "army.screen.total_troops",
                "army.screen.active_queues",
                "army.screen.roster_count",
                "army.screen.btn_recruit",
                "army.drawer.title",
                "army.drawer.quantity",
                "army.drawer.total_cost",
                "army.drawer.training_time",
                "army.drawer.req_met",
                "army.drawer.req_not_met",
                "army.drawer.action_recruit",
                "army.drawer.action_cancel",
                "army.drawer.time_remaining",
                "army.cancel_dialog.title",
                "army.cancel_dialog.msg",
                // Phase 7 Categories & Statuses
                "army.category.ground",
                "army.category.air",
                "army.category.all",
                "army.status.available",
                "army.status.in_training",
                "army.status.deployed",
                // Phase 7 Queue states
                "army.queue.loading",
                "army.queue.pending",
                "army.queue.in_progress",
                "army.queue.completed",
                "army.queue.failed",
                "army.queue.cancelled",
                "army.queue.refund_notice",
                // Phase 7 Detail View
                "army.detail.title",
                "army.detail.tier",
                "army.detail.level",
                "army.detail.category",
                "army.detail.action_train",
                "army.detail.action_upgrade",
                "army.detail.upgrade_cost",
                "army.detail.upgrade_duration",
                "army.detail.next_level_benefits",
                "army.detail.empty",
                "army.selection.error_exceeds",
                "army.selection.error_unknown"
            };

            foreach (var k in requiredKeys)
            {
                Assert.That(enTable.ContainsKey(k), Is.True, $"Missing EN key: {k}");
                Assert.That(arTable.ContainsKey(k), Is.True, $"Missing AR key: {k}");
                Assert.That(string.IsNullOrWhiteSpace(enTable[k]), Is.False, $"Empty EN key: {k}");
                Assert.That(string.IsNullOrWhiteSpace(arTable[k]), Is.False, $"Empty AR key: {k}");
            }
        }
    }
}
