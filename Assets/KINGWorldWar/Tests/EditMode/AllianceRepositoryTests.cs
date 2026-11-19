using System;
using System.Linq;
using System.Threading.Tasks;
using KingWorldWar.Core.Alliance;
using KingWorldWar.Infrastructure.Alliance;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class AllianceRepositoryTests
    {
        private LocalMockAllianceRepository _repo;

        [SetUp]
        public void Setup()
        {
            _repo = new LocalMockAllianceRepository();
            _repo.SimulatedLatencyMs = 0; // Immediate for fast tests
            _repo.CurrentPlayerId = "usr_tester_01";
            _repo.CurrentPlayerName = "Test Commander";
            _repo.CurrentPlayerCombatPower = 500000;
        }

        [Test]
        public async Task GetAllianceAsync_ReturnsSeededProfiles()
        {
            var res1 = await _repo.GetAllianceAsync(new AllianceId("all_kww_001"));
            Assert.IsTrue(res1.IsSuccess);
            Assert.AreEqual("King Vanguard", res1.Value.Name);
            Assert.AreEqual("KWW", res1.Value.Tag);
            Assert.AreEqual(AllianceJoinPolicy.Open, res1.Value.JoinPolicy);

            var res2 = await _repo.GetAllianceAsync(new AllianceId("all_iron_002"));
            Assert.IsTrue(res2.IsSuccess);
            Assert.AreEqual("Iron Legion", res2.Value.Name);
            Assert.AreEqual(AllianceJoinPolicy.ApplyOnly, res2.Value.JoinPolicy);
        }

        [Test]
        public async Task SearchAlliancesAsync_FiltersByNameAndTag()
        {
            var searchName = await _repo.SearchAlliancesAsync("Iron");
            Assert.IsTrue(searchName.IsSuccess);
            Assert.AreEqual(1, searchName.Value.Count);
            Assert.AreEqual("Iron Legion", searchName.Value[0].Name);

            var searchTag = await _repo.SearchAlliancesAsync("DES");
            Assert.IsTrue(searchTag.IsSuccess);
            Assert.AreEqual(1, searchTag.Value.Count);
            Assert.AreEqual("Desert Falcons", searchTag.Value[0].Name);
        }

        [Test]
        public async Task SearchAlliancesAsync_FiltersByJoinPolicy()
        {
            var applyOnly = await _repo.SearchAlliancesAsync("", AllianceJoinPolicy.ApplyOnly);
            Assert.IsTrue(applyOnly.IsSuccess);
            Assert.AreEqual(1, applyOnly.Value.Count);
            Assert.AreEqual("Iron Legion", applyOnly.Value[0].Name);

            var openAlliances = await _repo.SearchAlliancesAsync("", AllianceJoinPolicy.Open);
            Assert.IsTrue(openAlliances.IsSuccess);
            Assert.AreEqual(2, openAlliances.Value.Count);
        }

        [Test]
        public async Task JoinAllianceAsync_OpenPolicy_IncrementsCountAndAddsMember()
        {
            var allianceId = new AllianceId("all_kww_001");
            var initialProfile = (await _repo.GetAllianceAsync(allianceId)).Value;
            int initialCount = initialProfile.MemberCount;

            var joinResult = await _repo.JoinAllianceAsync(Guid.NewGuid().ToString("N"), allianceId);
            Assert.IsTrue(joinResult.IsSuccess);

            var updatedProfile = (await _repo.GetAllianceAsync(allianceId)).Value;
            Assert.AreEqual(initialCount + 1, updatedProfile.MemberCount);

            var members = (await _repo.GetMembersAsync(allianceId)).Value;
            Assert.IsTrue(members.Any(m => m.PlayerId == "usr_tester_01" && m.Role == AllianceRole.Recruit));
        }

        [Test]
        public async Task JoinAllianceAsync_ApplyOnlyPolicy_Fails()
        {
            var allianceId = new AllianceId("all_iron_002");
            var joinResult = await _repo.JoinAllianceAsync(Guid.NewGuid().ToString("N"), allianceId);
            Assert.IsFalse(joinResult.IsSuccess);
            Assert.AreEqual("INSUFFICIENT_PERMISSIONS", joinResult.Error.Code);
        }

        [Test]
        public async Task ApplyToAllianceAsync_AddsToPendingApplications()
        {
            var allianceId = new AllianceId("all_iron_002");
            var applyResult = await _repo.ApplyToAllianceAsync(Guid.NewGuid().ToString("N"), allianceId);
            Assert.IsTrue(applyResult.IsSuccess);

            var apps = (await _repo.GetApplicationsAsync(allianceId)).Value;
            Assert.IsTrue(apps.Any(a => a.PlayerId == "usr_tester_01"));
        }

        [Test]
        public async Task AcceptApplicationAsync_AuthorizedOfficer_AddsMemberAndRemovesApp()
        {
            var allianceId = new AllianceId("all_iron_002");

            // Setup caller as Officer of all_iron_002
            _repo.SetPlayerAllianceForTest(allianceId, AllianceRole.Officer);

            var apps = (await _repo.GetApplicationsAsync(allianceId)).Value;
            Assert.Greater(apps.Count, 0);
            var targetApp = apps[0];

            var acceptResult = await _repo.AcceptApplicationAsync(Guid.NewGuid().ToString("N"), allianceId, targetApp.ApplicationId);
            Assert.IsTrue(acceptResult.IsSuccess);

            var remainingApps = (await _repo.GetApplicationsAsync(allianceId)).Value;
            Assert.IsFalse(remainingApps.Any(a => a.ApplicationId == targetApp.ApplicationId));

            var members = (await _repo.GetMembersAsync(allianceId)).Value;
            Assert.IsTrue(members.Any(m => m.PlayerId == targetApp.PlayerId));
        }

        [Test]
        public async Task RejectApplicationAsync_AuthorizedOfficer_RemovesAppWithoutAddingMember()
        {
            var allianceId = new AllianceId("all_iron_002");
            _repo.SetPlayerAllianceForTest(allianceId, AllianceRole.Officer);

            var apps = (await _repo.GetApplicationsAsync(allianceId)).Value;
            var targetApp = apps[0];

            var rejectResult = await _repo.RejectApplicationAsync(Guid.NewGuid().ToString("N"), allianceId, targetApp.ApplicationId);
            Assert.IsTrue(rejectResult.IsSuccess);

            var remainingApps = (await _repo.GetApplicationsAsync(allianceId)).Value;
            Assert.IsFalse(remainingApps.Any(a => a.ApplicationId == targetApp.ApplicationId));

            var members = (await _repo.GetMembersAsync(allianceId)).Value;
            Assert.IsFalse(members.Any(m => m.PlayerId == targetApp.PlayerId));
        }

        [Test]
        public async Task KickMemberAsync_OfficerKickingRecruit_Succeeds()
        {
            var allianceId = new AllianceId("all_kww_001");
            _repo.SetPlayerAllianceForTest(allianceId, AllianceRole.Officer);

            var members = (await _repo.GetMembersAsync(allianceId)).Value;
            var recruit = members.FirstOrDefault(m => m.Role == AllianceRole.Recruit);
            Assert.IsNotNull(recruit);

            var kickResult = await _repo.KickMemberAsync(Guid.NewGuid().ToString("N"), allianceId, recruit.PlayerId);
            Assert.IsTrue(kickResult.IsSuccess);

            var updatedMembers = (await _repo.GetMembersAsync(allianceId)).Value;
            Assert.IsFalse(updatedMembers.Any(m => m.PlayerId == recruit.PlayerId));
        }

        [Test]
        public async Task KickMemberAsync_OfficerKickingLeader_FailsWithHierarchyError()
        {
            var allianceId = new AllianceId("all_kww_001");
            _repo.SetPlayerAllianceForTest(allianceId, AllianceRole.Officer);

            var members = (await _repo.GetMembersAsync(allianceId)).Value;
            var leader = members.FirstOrDefault(m => m.Role == AllianceRole.Leader);
            Assert.IsNotNull(leader);

            var kickResult = await _repo.KickMemberAsync(Guid.NewGuid().ToString("N"), allianceId, leader.PlayerId);
            Assert.IsFalse(kickResult.IsSuccess);
            Assert.AreEqual("CANNOT_KICK_HIGHER_ROLE", kickResult.Error.Code);
        }

        [Test]
        public async Task CreateAllianceAsync_EstablishesNewAllianceWithPlayerAsLeader()
        {
            string reqId = Guid.NewGuid().ToString("N");
            var createResult = await _repo.CreateAllianceAsync(reqId, "Black Watch", "BLKW", "Recon group", AllianceJoinPolicy.Open);
            Assert.IsTrue(createResult.IsSuccess);
            Assert.AreEqual("Black Watch", createResult.Value.Name);
            Assert.AreEqual("BLKW", createResult.Value.Tag);
            Assert.AreEqual(1, createResult.Value.MemberCount);

            var members = (await _repo.GetMembersAsync(createResult.Value.Id)).Value;
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual("usr_tester_01", members[0].PlayerId);
            Assert.AreEqual(AllianceRole.Leader, members[0].Role);
        }

        [Test]
        public async Task CreateAllianceAsync_RejectsDuplicateTagOrName()
        {
            var dupTag = await _repo.CreateAllianceAsync(Guid.NewGuid().ToString("N"), "Unique Name", "KWW", "", AllianceJoinPolicy.Open);
            Assert.IsFalse(dupTag.IsSuccess);
            Assert.AreEqual("ALLIANCE_TAG_TAKEN", dupTag.Error.Code);

            var dupName = await _repo.CreateAllianceAsync(Guid.NewGuid().ToString("N"), "King Vanguard", "NEWT", "", AllianceJoinPolicy.Open);
            Assert.IsFalse(dupName.IsSuccess);
            Assert.AreEqual("ALLIANCE_NAME_TAKEN", dupName.Error.Code);
        }

        [Test]
        public async Task Idempotency_DuplicateRequestIdReturnsCachedResult()
        {
            string reqId = "req_fixed_id_123";
            var first = await _repo.CreateAllianceAsync(reqId, "Strike Force", "STRK", "Notice", AllianceJoinPolicy.Open);
            Assert.IsTrue(first.IsSuccess);

            var second = await _repo.CreateAllianceAsync(reqId, "Different Name", "DIFF", "Notice", AllianceJoinPolicy.Open);
            Assert.IsTrue(second.IsSuccess);
            Assert.AreEqual(first.Value.Id, second.Value.Id);
            Assert.AreEqual("Strike Force", second.Value.Name);
        }
    }
}
