using System;
using KingWorldWar.Core.Alliance;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class AllianceDomainTests
    {
        [Test]
        public void AllianceRole_LeaderHasFullAdministrativePermissions()
        {
            var leader = AllianceRole.Leader;

            Assert.IsTrue(leader.CanInvite());
            Assert.IsTrue(leader.CanManageApplications());
            Assert.IsTrue(leader.CanEditNotice());
            Assert.IsTrue(leader.CanDisband());
            Assert.IsTrue(leader.CanTransferLeadership());

            // Kicking checks
            Assert.IsTrue(leader.CanKick(AllianceRole.Officer));
            Assert.IsTrue(leader.CanKick(AllianceRole.Veteran));
            Assert.IsTrue(leader.CanKick(AllianceRole.Member));
            Assert.IsTrue(leader.CanKick(AllianceRole.Recruit));
            Assert.IsFalse(leader.CanKick(AllianceRole.Leader), "Leader cannot kick self via kick method");
        }

        [Test]
        public void AllianceRole_OfficerPermissionsRespectHierarchyLimits()
        {
            var officer = AllianceRole.Officer;

            Assert.IsTrue(officer.CanInvite());
            Assert.IsTrue(officer.CanManageApplications());
            Assert.IsTrue(officer.CanEditNotice());
            Assert.IsFalse(officer.CanDisband(), "Officer cannot disband alliance");
            Assert.IsFalse(officer.CanTransferLeadership(), "Officer cannot transfer leadership");

            // Kicking checks
            Assert.IsTrue(officer.CanKick(AllianceRole.Veteran));
            Assert.IsTrue(officer.CanKick(AllianceRole.Member));
            Assert.IsTrue(officer.CanKick(AllianceRole.Recruit));
            Assert.IsFalse(officer.CanKick(AllianceRole.Officer), "Officer cannot kick equal rank");
            Assert.IsFalse(officer.CanKick(AllianceRole.Leader), "Officer cannot kick superior Leader");
        }

        [Test]
        public void AllianceRole_SubordinateRanksHaveZeroAdministrativePrivileges()
        {
            AllianceRole[] subordinates = { AllianceRole.Veteran, AllianceRole.Member, AllianceRole.Recruit };

            foreach (var role in subordinates)
            {
                Assert.IsFalse(role.CanInvite(), $"{role} cannot invite");
                Assert.IsFalse(role.CanManageApplications(), $"{role} cannot manage applications");
                Assert.IsFalse(role.CanEditNotice(), $"{role} cannot edit notice");
                Assert.IsFalse(role.CanDisband(), $"{role} cannot disband");
                Assert.IsFalse(role.CanTransferLeadership(), $"{role} cannot transfer leadership");
                Assert.IsFalse(role.CanKick(AllianceRole.Recruit), $"{role} cannot kick subordinates");
            }
        }

        [Test]
        public void AllianceRole_PromotionAndDemotionEnforceRoleLimits()
        {
            var leader = AllianceRole.Leader;
            var officer = AllianceRole.Officer;

            // Only Leader can promote to Officer
            Assert.IsTrue(leader.CanPromote(AllianceRole.Veteran, AllianceRole.Officer));
            Assert.IsFalse(officer.CanPromote(AllianceRole.Veteran, AllianceRole.Officer));

            // Officer can promote Member to Veteran
            Assert.IsTrue(officer.CanPromote(AllianceRole.Member, AllianceRole.Veteran));

            // Only Leader can demote Officer
            Assert.IsTrue(leader.CanDemote(AllianceRole.Officer, AllianceRole.Veteran));
            Assert.IsFalse(officer.CanDemote(AllianceRole.Officer, AllianceRole.Veteran));

            // Officer can demote Veteran to Member
            Assert.IsTrue(officer.CanDemote(AllianceRole.Veteran, AllianceRole.Member));
        }

        [Test]
        public void AllianceProfile_ValidateCreationParameters_AcceptsValidInput()
        {
            var res1 = AllianceProfile.ValidateCreationParameters("King Vanguard", "KWW", "Valid notice description.");
            Assert.IsTrue(res1.IsSuccess);

            var res2 = AllianceProfile.ValidateCreationParameters("Desert Hawks", "HAWK", "");
            Assert.IsTrue(res2.IsSuccess);
        }

        [Test]
        public void AllianceProfile_ValidateCreationParameters_RejectsInvalidNames()
        {
            var tooShort = AllianceProfile.ValidateCreationParameters("KW", "KWW", "Notice");
            Assert.IsFalse(tooShort.IsSuccess);
            Assert.AreEqual("INVALID_PAYLOAD", tooShort.Error.Code);

            var tooLong = AllianceProfile.ValidateCreationParameters(new string('A', 25), "KWW", "Notice");
            Assert.IsFalse(tooLong.IsSuccess);
        }

        [Test]
        public void AllianceProfile_ValidateCreationParameters_RejectsInvalidTags()
        {
            var tooShort = AllianceProfile.ValidateCreationParameters("King Vanguard", "KW", "Notice");
            Assert.IsFalse(tooShort.IsSuccess);

            var tooLong = AllianceProfile.ValidateCreationParameters("King Vanguard", "KWWAA", "Notice");
            Assert.IsFalse(tooLong.IsSuccess);

            var nonAlpha = AllianceProfile.ValidateCreationParameters("King Vanguard", "KW#1", "Notice");
            Assert.IsFalse(nonAlpha.IsSuccess);

            var spaces = AllianceProfile.ValidateCreationParameters("King Vanguard", "K W", "Notice");
            Assert.IsFalse(spaces.IsSuccess);
        }

        [Test]
        public void AllianceProfile_ValidateCreationParameters_RejectsLongNotice()
        {
            var longNotice = AllianceProfile.ValidateCreationParameters("King Vanguard", "KWW", new string('N', 257));
            Assert.IsFalse(longNotice.IsSuccess);
        }

        [Test]
        public void AllianceId_CaseInsensitiveEquality()
        {
            var id1 = new AllianceId("all_kww_001");
            var id2 = new AllianceId("ALL_KWW_001");
            var id3 = new AllianceId("all_iron_002");

            Assert.AreEqual(id1, id2);
            Assert.IsTrue(id1 == id2);
            Assert.AreNotEqual(id1, id3);
            Assert.IsTrue(id1.IsValid);
            Assert.IsFalse(AllianceId.Empty.IsValid);
        }

        [Test]
        public void AllianceProfile_ImmutabilityAndStatusChecks()
        {
            var profile = new AllianceProfile(
                new AllianceId("all_01"),
                "Falcon Corps",
                "FALC",
                "Original Notice",
                "usr_01",
                "Leader Alpha",
                10,
                50,
                5000000,
                AllianceJoinPolicy.Open,
                DateTime.UtcNow
            );

            Assert.IsFalse(profile.IsFull);
            Assert.IsTrue(profile.CanDirectJoin);
            Assert.IsFalse(profile.CanApply);

            var updatedNotice = profile.WithNotice("Updated Notice");
            Assert.AreEqual("Updated Notice", updatedNotice.Notice);
            Assert.AreEqual("Original Notice", profile.Notice, "Original profile must remain immutable");

            var updatedPolicy = profile.WithPolicy(AllianceJoinPolicy.ApplyOnly);
            Assert.AreEqual(AllianceJoinPolicy.ApplyOnly, updatedPolicy.JoinPolicy);
            Assert.IsTrue(updatedPolicy.CanApply);
            Assert.IsFalse(updatedPolicy.CanDirectJoin);

            var fullProfile = profile.WithMemberCount(50, 25000000);
            Assert.IsTrue(fullProfile.IsFull);
            Assert.IsFalse(fullProfile.CanDirectJoin);
            Assert.IsFalse(fullProfile.CanApply);
        }
    }
}
