#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Alliance;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Infrastructure.Alliance
{
    /// <summary>
    /// Editor/Development-only mock implementation of IAllianceRepository.
    /// Simulates server-authoritative permission checks, atomic operations, role hierarchy enforcement,
    /// idempotency cache, capacity locks, and seeded military coalitions.
    /// Strictly excluded from release builds.
    /// </summary>
    public sealed class LocalMockAllianceRepository : IAllianceRepository
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, object> _processedRequests = new();

        private readonly Dictionary<AllianceId, AllianceProfile> _alliances = new();
        private readonly Dictionary<AllianceId, List<AllianceMember>> _members = new();
        private readonly Dictionary<AllianceId, List<AllianceApplication>> _applications = new();

        public bool IsMock => true;
        public int SimulatedLatencyMs { get; set; } = 20;

        public string CurrentPlayerId { get; set; } = "usr_player_001";
        public string CurrentPlayerName { get; set; } = "Commander Alpha";
        public long CurrentPlayerCombatPower { get; set; } = 750000;

        public LocalMockAllianceRepository()
        {
            SeedInitialData();
        }

        private void SeedInitialData()
        {
            // Seed 1: King Vanguard [KWW] (Open)
            var id1 = new AllianceId("all_kww_001");
            var profile1 = new AllianceProfile(
                id1,
                "King Vanguard",
                "KWW",
                "Welcome to the Vanguard. Mobile mechanized units coordinate here.",
                "usr_leader_01",
                "General Valerius",
                28,
                50,
                12850000,
                AllianceJoinPolicy.Open,
                DateTime.UtcNow.AddDays(-30)
            );
            _alliances[id1] = profile1;

            var members1 = new List<AllianceMember>
            {
                new AllianceMember("usr_leader_01", "General Valerius", AllianceRole.Leader, 1850000, 34200, DateTime.UtcNow.AddDays(-30)),
                new AllianceMember("usr_officer_01", "Colonel Vance", AllianceRole.Officer, 1200000, 21500, DateTime.UtcNow.AddDays(-25)),
                new AllianceMember("usr_veteran_01", "Major Sterling", AllianceRole.Veteran, 950000, 16400, DateTime.UtcNow.AddDays(-20)),
                new AllianceMember("usr_member_01", "Captain Rex", AllianceRole.Member, 680000, 8900, DateTime.UtcNow.AddDays(-15)),
                new AllianceMember("usr_recruit_01", "Lieutenant Fox", AllianceRole.Recruit, 420000, 2300, DateTime.UtcNow.AddDays(-2))
            };
            _members[id1] = members1;
            _applications[id1] = new List<AllianceApplication>();

            // Seed 2: Iron Legion [IRON] (ApplyOnly)
            var id2 = new AllianceId("all_iron_002");
            var profile2 = new AllianceProfile(
                id2,
                "Iron Legion",
                "IRON",
                "Heavy armor coalition. Apply with 500k+ combat power. Strict discipline.",
                "usr_leader_02",
                "Marshal Ironhide",
                42,
                50,
                21500000,
                AllianceJoinPolicy.ApplyOnly,
                DateTime.UtcNow.AddDays(-60)
            );
            _alliances[id2] = profile2;

            var members2 = new List<AllianceMember>
            {
                new AllianceMember("usr_leader_02", "Marshal Ironhide", AllianceRole.Leader, 2400000, 48000, DateTime.UtcNow.AddDays(-60)),
                new AllianceMember("usr_officer_02", "Commander Steele", AllianceRole.Officer, 1750000, 31000, DateTime.UtcNow.AddDays(-50)),
                new AllianceMember("usr_veteran_02", "Major Thorne", AllianceRole.Veteran, 1100000, 19500, DateTime.UtcNow.AddDays(-40))
            };
            _members[id2] = members2;

            var apps2 = new List<AllianceApplication>
            {
                new AllianceApplication("app_iron_001", id2, "usr_applicant_01", "Major Echo", 620000, DateTime.UtcNow.AddHours(-5)),
                new AllianceApplication("app_iron_002", id2, "usr_applicant_02", "Captain Strike", 540000, DateTime.UtcNow.AddHours(-1))
            };
            _applications[id2] = apps2;

            // Seed 3: Desert Falcons [DES] (Open)
            var id3 = new AllianceId("all_des_003");
            var profile3 = new AllianceProfile(
                id3,
                "Desert Falcons",
                "DES",
                "Fast motorized raiders of the south. All active commanders welcome.",
                "usr_leader_03",
                "Colonel Tariq",
                15,
                50,
                6400000,
                AllianceJoinPolicy.Open,
                DateTime.UtcNow.AddDays(-10)
            );
            _alliances[id3] = profile3;

            var members3 = new List<AllianceMember>
            {
                new AllianceMember("usr_leader_03", "Colonel Tariq", AllianceRole.Leader, 1150000, 14200, DateTime.UtcNow.AddDays(-10)),
                new AllianceMember("usr_officer_03", "Major Faris", AllianceRole.Officer, 850000, 9800, DateTime.UtcNow.AddDays(-8))
            };
            _members[id3] = members3;
            _applications[id3] = new List<AllianceApplication>();
        }

        public async Task<Result<AllianceProfile, GameError>> GetAllianceAsync(AllianceId id, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (!id.IsValid)
                {
                    // Look up caller's current alliance
                    foreach (var kvp in _members)
                    {
                        if (kvp.Value.Any(m => m.PlayerId == CurrentPlayerId))
                        {
                            return Result<AllianceProfile, GameError>.Ok(_alliances[kvp.Key]);
                        }
                    }
                    return Result<AllianceProfile, GameError>.Fail(GameError.NotInAlliance());
                }

                if (_alliances.TryGetValue(id, out var profile))
                {
                    return Result<AllianceProfile, GameError>.Ok(profile);
                }

                return Result<AllianceProfile, GameError>.Fail(GameError.AllianceNotFound(id.Value));
            }
        }

        public async Task<Result<IReadOnlyList<AllianceProfile>, GameError>> SearchAlliancesAsync(string query, AllianceJoinPolicy? policyFilter = null, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                var queryTrimmed = query?.Trim() ?? string.Empty;
                IEnumerable<AllianceProfile> results = _alliances.Values;

                if (!string.IsNullOrEmpty(queryTrimmed))
                {
                    results = results.Where(a =>
                        a.Name.IndexOf(queryTrimmed, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        a.Tag.IndexOf(queryTrimmed, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (policyFilter.HasValue)
                {
                    results = results.Where(a => a.JoinPolicy == policyFilter.Value);
                }

                return Result<IReadOnlyList<AllianceProfile>, GameError>.Ok(results.OrderByDescending(a => a.TotalCombatPower).ToList());
            }
        }

        public async Task<Result<IReadOnlyList<AllianceMember>, GameError>> GetMembersAsync(AllianceId id, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_members.TryGetValue(id, out var members))
                {
                    return Result<IReadOnlyList<AllianceMember>, GameError>.Ok(
                        members.OrderBy(m => (int)m.Role).ThenByDescending(m => m.CombatPower).ToList());
                }

                return Result<IReadOnlyList<AllianceMember>, GameError>.Fail(GameError.AllianceNotFound(id.Value));
            }
        }

        public async Task<Result<IReadOnlyList<AllianceApplication>, GameError>> GetApplicationsAsync(AllianceId id, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_applications.TryGetValue(id, out var apps))
                {
                    return Result<IReadOnlyList<AllianceApplication>, GameError>.Ok(
                        apps.OrderByDescending(a => a.AppliedAtUtc).ToList());
                }

                return Result<IReadOnlyList<AllianceApplication>, GameError>.Fail(GameError.AllianceNotFound(id.Value));
            }
        }

        public async Task<Result<AllianceProfile, GameError>> CreateAllianceAsync(string requestId, string name, string tag, string notice, AllianceJoinPolicy policy, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.TryGetValue(requestId, out var cached) && cached is AllianceProfile cachedProfile)
                {
                    return Result<AllianceProfile, GameError>.Ok(cachedProfile);
                }

                // Check if player is already in an alliance
                if (IsPlayerInAnyAlliance_Internal(CurrentPlayerId))
                {
                    return Result<AllianceProfile, GameError>.Fail(GameError.AlreadyInAlliance());
                }

                // Validation
                var validation = AllianceProfile.ValidateCreationParameters(name, tag, notice);
                if (!validation.IsSuccess)
                {
                    return Result<AllianceProfile, GameError>.Fail(validation.Error);
                }

                string trimmedTag = tag.Trim().ToUpperInvariant();
                string trimmedName = name.Trim();

                if (_alliances.Values.Any(a => string.Equals(a.Tag, trimmedTag, StringComparison.OrdinalIgnoreCase)))
                {
                    return Result<AllianceProfile, GameError>.Fail(GameError.AllianceTagTaken(trimmedTag));
                }

                if (_alliances.Values.Any(a => string.Equals(a.Name, trimmedName, StringComparison.OrdinalIgnoreCase)))
                {
                    return Result<AllianceProfile, GameError>.Fail(GameError.AllianceNameTaken(trimmedName));
                }

                var newId = new AllianceId("all_" + Guid.NewGuid().ToString("N").Substring(0, 8));
                var profile = new AllianceProfile(
                    newId,
                    trimmedName,
                    trimmedTag,
                    notice?.Trim() ?? string.Empty,
                    CurrentPlayerId,
                    CurrentPlayerName,
                    1,
                    50,
                    CurrentPlayerCombatPower,
                    policy,
                    DateTime.UtcNow
                );

                var leaderMember = new AllianceMember(
                    CurrentPlayerId,
                    CurrentPlayerName,
                    AllianceRole.Leader,
                    CurrentPlayerCombatPower,
                    0,
                    DateTime.UtcNow
                );

                _alliances[newId] = profile;
                _members[newId] = new List<AllianceMember> { leaderMember };
                _applications[newId] = new List<AllianceApplication>();

                _processedRequests[requestId] = profile;
                return Result<AllianceProfile, GameError>.Ok(profile);
            }
        }

        public async Task<Result<bool, GameError>> JoinAllianceAsync(string requestId, AllianceId id, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                if (IsPlayerInAnyAlliance_Internal(CurrentPlayerId))
                    return Result<bool, GameError>.Fail(GameError.AlreadyInAlliance());

                if (!_alliances.TryGetValue(id, out var profile))
                    return Result<bool, GameError>.Fail(GameError.AllianceNotFound(id.Value));

                if (profile.IsFull)
                    return Result<bool, GameError>.Fail(GameError.AllianceFull(id.Value));

                if (profile.JoinPolicy != AllianceJoinPolicy.Open)
                    return Result<bool, GameError>.Fail(GameError.InsufficientPermissions("Alliance policy is not Open."));

                var member = new AllianceMember(CurrentPlayerId, CurrentPlayerName, AllianceRole.Recruit, CurrentPlayerCombatPower, 0, DateTime.UtcNow);
                _members[id].Add(member);

                long newPower = profile.TotalCombatPower + CurrentPlayerCombatPower;
                _alliances[id] = profile.WithMemberCount(profile.MemberCount + 1, newPower);

                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        public async Task<Result<bool, GameError>> ApplyToAllianceAsync(string requestId, AllianceId id, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                if (IsPlayerInAnyAlliance_Internal(CurrentPlayerId))
                    return Result<bool, GameError>.Fail(GameError.AlreadyInAlliance());

                if (!_alliances.TryGetValue(id, out var profile))
                    return Result<bool, GameError>.Fail(GameError.AllianceNotFound(id.Value));

                if (profile.IsFull)
                    return Result<bool, GameError>.Fail(GameError.AllianceFull(id.Value));

                if (profile.JoinPolicy != AllianceJoinPolicy.ApplyOnly)
                    return Result<bool, GameError>.Fail(GameError.InsufficientPermissions("Alliance does not accept applications."));

                if (!_applications.TryGetValue(id, out var apps))
                {
                    apps = new List<AllianceApplication>();
                    _applications[id] = apps;
                }

                if (apps.Any(a => a.PlayerId == CurrentPlayerId))
                {
                    // Already applied
                    return Result<bool, GameError>.Ok(true);
                }

                var application = new AllianceApplication(
                    "app_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    id,
                    CurrentPlayerId,
                    CurrentPlayerName,
                    CurrentPlayerCombatPower,
                    DateTime.UtcNow
                );

                apps.Add(application);
                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        public async Task<Result<bool, GameError>> AcceptApplicationAsync(string requestId, AllianceId id, string applicationId, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                var callerRole = GetCallerRole_Internal(id, CurrentPlayerId);
                if (!callerRole.CanManageApplications())
                    return Result<bool, GameError>.Fail(GameError.InsufficientPermissions("Officer rank or higher required to decide applications."));

                if (!_alliances.TryGetValue(id, out var profile))
                    return Result<bool, GameError>.Fail(GameError.AllianceNotFound(id.Value));

                if (profile.IsFull)
                    return Result<bool, GameError>.Fail(GameError.AllianceFull(id.Value));

                if (!_applications.TryGetValue(id, out var apps))
                    return Result<bool, GameError>.Fail(GameError.ApplicationNotFound(applicationId));

                var appIndex = apps.FindIndex(a => a.ApplicationId == applicationId);
                if (appIndex < 0)
                    return Result<bool, GameError>.Fail(GameError.ApplicationNotFound(applicationId));

                var app = apps[appIndex];
                apps.RemoveAt(appIndex);

                // Add as recruit if not in another alliance
                if (!IsPlayerInAnyAlliance_Internal(app.PlayerId))
                {
                    var newMember = new AllianceMember(app.PlayerId, app.PlayerName, AllianceRole.Recruit, app.CombatPower, 0, DateTime.UtcNow);
                    _members[id].Add(newMember);

                    long newPower = profile.TotalCombatPower + app.CombatPower;
                    _alliances[id] = profile.WithMemberCount(profile.MemberCount + 1, newPower);
                }

                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        public async Task<Result<bool, GameError>> RejectApplicationAsync(string requestId, AllianceId id, string applicationId, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                var callerRole = GetCallerRole_Internal(id, CurrentPlayerId);
                if (!callerRole.CanManageApplications())
                    return Result<bool, GameError>.Fail(GameError.InsufficientPermissions("Officer rank or higher required to decide applications."));

                if (!_applications.TryGetValue(id, out var apps))
                    return Result<bool, GameError>.Fail(GameError.ApplicationNotFound(applicationId));

                var appIndex = apps.FindIndex(a => a.ApplicationId == applicationId);
                if (appIndex >= 0)
                {
                    apps.RemoveAt(appIndex);
                }

                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        public async Task<Result<bool, GameError>> KickMemberAsync(string requestId, AllianceId id, string targetPlayerId, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                if (string.Equals(CurrentPlayerId, targetPlayerId, StringComparison.OrdinalIgnoreCase))
                    return Result<bool, GameError>.Fail(GameError.InsufficientPermissions("Cannot kick self; use leave command."));

                var callerRole = GetCallerRole_Internal(id, CurrentPlayerId);
                if (!_members.TryGetValue(id, out var members))
                    return Result<bool, GameError>.Fail(GameError.AllianceNotFound(id.Value));

                var targetMember = members.FirstOrDefault(m => string.Equals(m.PlayerId, targetPlayerId, StringComparison.OrdinalIgnoreCase));
                if (targetMember == null)
                    return Result<bool, GameError>.Fail(GameError.InvalidPayload("Target member not in this alliance."));

                if (!callerRole.CanKick(targetMember.Role))
                    return Result<bool, GameError>.Fail(GameError.CannotKickHigherOrEqualRole());

                members.Remove(targetMember);

                if (_alliances.TryGetValue(id, out var profile))
                {
                    long newPower = Math.Max(0, profile.TotalCombatPower - targetMember.CombatPower);
                    _alliances[id] = profile.WithMemberCount(Math.Max(1, profile.MemberCount - 1), newPower);
                }

                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        public async Task<Result<bool, GameError>> SetMemberRoleAsync(string requestId, AllianceId id, string targetPlayerId, AllianceRole newRole, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                var callerRole = GetCallerRole_Internal(id, CurrentPlayerId);
                if (!_members.TryGetValue(id, out var members))
                    return Result<bool, GameError>.Fail(GameError.AllianceNotFound(id.Value));

                var targetMember = members.FirstOrDefault(m => string.Equals(m.PlayerId, targetPlayerId, StringComparison.OrdinalIgnoreCase));
                if (targetMember == null)
                    return Result<bool, GameError>.Fail(GameError.InvalidPayload("Target member not in this alliance."));

                bool isPromotion = (int)newRole < (int)targetMember.Role;
                bool allowed = isPromotion
                    ? callerRole.CanPromote(targetMember.Role, newRole)
                    : callerRole.CanDemote(targetMember.Role, newRole);

                if (!allowed)
                    return Result<bool, GameError>.Fail(GameError.InsufficientPermissions("Role change not allowed by rank hierarchy."));

                int index = members.IndexOf(targetMember);
                members[index] = targetMember.WithRole(newRole);

                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        public async Task<Result<bool, GameError>> LeaveAllianceAsync(string requestId, AllianceId id, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                if (!_members.TryGetValue(id, out var members))
                    return Result<bool, GameError>.Fail(GameError.AllianceNotFound(id.Value));

                var caller = members.FirstOrDefault(m => string.Equals(m.PlayerId, CurrentPlayerId, StringComparison.OrdinalIgnoreCase));
                if (caller == null)
                    return Result<bool, GameError>.Fail(GameError.NotInAlliance());

                if (caller.Role == AllianceRole.Leader && members.Count > 1)
                {
                    return Result<bool, GameError>.Fail(
                        GameError.InsufficientPermissions("Leader cannot leave without transferring leadership first."));
                }

                members.Remove(caller);

                if (_alliances.TryGetValue(id, out var profile))
                {
                    if (members.Count == 0)
                    {
                        // Disband empty alliance
                        _alliances.Remove(id);
                        _members.Remove(id);
                        _applications.Remove(id);
                    }
                    else
                    {
                        long newPower = Math.Max(0, profile.TotalCombatPower - caller.CombatPower);
                        _alliances[id] = profile.WithMemberCount(Math.Max(1, profile.MemberCount - 1), newPower);
                    }
                }

                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        public async Task<Result<bool, GameError>> UpdateNoticeAsync(string requestId, AllianceId id, string newNotice, CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                var callerRole = GetCallerRole_Internal(id, CurrentPlayerId);
                if (!callerRole.CanEditNotice())
                    return Result<bool, GameError>.Fail(GameError.InsufficientPermissions("Officer or Leader required to edit notice."));

                if (!_alliances.TryGetValue(id, out var profile))
                    return Result<bool, GameError>.Fail(GameError.AllianceNotFound(id.Value));

                string trimmedNotice = newNotice?.Trim() ?? string.Empty;
                if (trimmedNotice.Length > 256)
                    return Result<bool, GameError>.Fail(GameError.InvalidPayload("Notice exceeds maximum length of 256 characters."));

                _alliances[id] = profile.WithNotice(trimmedNotice);
                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        private bool IsPlayerInAnyAlliance_Internal(string playerId)
        {
            return _members.Values.Any(list => list.Any(m => string.Equals(m.PlayerId, playerId, StringComparison.OrdinalIgnoreCase)));
        }

        private AllianceRole GetCallerRole_Internal(AllianceId allianceId, string playerId)
        {
            if (_members.TryGetValue(allianceId, out var members))
            {
                var m = members.FirstOrDefault(mem => string.Equals(mem.PlayerId, playerId, StringComparison.OrdinalIgnoreCase));
                if (m != null) return m.Role;
            }
            return AllianceRole.None;
        }

        #if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
        public void SetPlayerAllianceForTest(AllianceId id, AllianceRole role)
        {
            lock (_lock)
            {
                // Remove player from any current alliances
                foreach (var kvp in _members)
                {
                    kvp.Value.RemoveAll(m => m.PlayerId == CurrentPlayerId);
                }

                if (id.IsValid && _alliances.ContainsKey(id))
                {
                    _members[id].Add(new AllianceMember(CurrentPlayerId, CurrentPlayerName, role, CurrentPlayerCombatPower, 1000, DateTime.UtcNow));
                    var p = _alliances[id];
                    _alliances[id] = p.WithMemberCount(_members[id].Count, p.TotalCombatPower + CurrentPlayerCombatPower);
                }
            }
        }

        public void ClearPlayerAllianceForTest()
        {
            lock (_lock)
            {
                foreach (var kvp in _members)
                {
                    kvp.Value.RemoveAll(m => m.PlayerId == CurrentPlayerId);
                }
            }
        }
        #endif
    }
}
#endif
