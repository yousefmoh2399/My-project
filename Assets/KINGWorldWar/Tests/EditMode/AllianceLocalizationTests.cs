using System.Collections.Generic;
using System.Linq;
using KingWorldWar.Core.Localization;
using KingWorldWar.Infrastructure.Localization;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class AllianceLocalizationTests
    {
        [Test]
        public void AllianceKeys_HaveStrictBilingualParity()
        {
            var enTable = DefaultStringTables.GetTable(Language.English);
            var arTable = DefaultStringTables.GetTable(Language.Arabic);

            var allianceEnKeys = enTable.Keys.Where(k => k.StartsWith("alliance.")).ToHashSet();
            var allianceArKeys = arTable.Keys.Where(k => k.StartsWith("alliance.")).ToHashSet();

            Assert.Greater(allianceEnKeys.Count, 0, "English alliance keys must be populated");
            Assert.AreEqual(allianceEnKeys.Count, allianceArKeys.Count, "Key counts must match exactly");

            foreach (var key in allianceEnKeys)
            {
                Assert.IsTrue(arTable.ContainsKey(key), $"Missing Arabic translation for key: {key}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(enTable[key]), $"English value for {key} cannot be empty");
                Assert.IsFalse(string.IsNullOrWhiteSpace(arTable[key]), $"Arabic value for {key} cannot be empty");
            }
        }

        [Test]
        public void AllianceRoleAndPolicyKeys_ExistForAllWindows()
        {
            var enTable = DefaultStringTables.GetTable(Language.English);
            var arTable = DefaultStringTables.GetTable(Language.Arabic);

            string[] expectedKeys =
            {
                "alliance.screen.title",
                "alliance.screen.no_alliance_title",
                "alliance.role.leader",
                "alliance.role.officer",
                "alliance.role.veteran",
                "alliance.role.member",
                "alliance.role.recruit",
                "alliance.policy.open",
                "alliance.policy.apply_only",
                "alliance.policy.closed",
                "alliance.create.cost_label",
                "alliance.drawer.title",
                "alliance.member.action_kick",
                "alliance.leave.confirm_title"
            };

            foreach (var key in expectedKeys)
            {
                Assert.IsTrue(enTable.ContainsKey(key), $"English table missing required key '{key}'");
                Assert.IsTrue(arTable.ContainsKey(key), $"Arabic table missing required key '{key}'");
            }
        }
    }
}
