using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Alliance;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Economy;
using KingWorldWar.Core.Logging;

namespace KingWorldWar.Infrastructure.Alliance
{
    /// <summary>
    /// Service managing client-side alliance state, reactive event dispatches,
    /// directory searches, member permissions, and server RPC coordination.
    /// Untrusted client: Enforces server-authoritative responses and optimistic state updates.
    /// </summary>
    public sealed class AllianceService : IAllianceService
    {
        private const string LogCat = "AllianceService";
        private const long AllianceCreationCostGold = 10000;

        private readonly IAllianceRepository _repository;
        private readonly IEconomyService _economyService;
        private readonly ILog _log;

        private AllianceProfile _currentAlliance;
        private AllianceRole _currentUserRole = AllianceRole.None;
        private IReadOnlyList<AllianceMember> _currentMembers = Array.Empty<AllianceMember>();
        private IReadOnlyList<AllianceApplication> _pendingApplications = Array.Empty<AllianceApplication>();

        public AllianceProfile CurrentAlliance => _currentAlliance;
        public AllianceRole CurrentUserRole => _currentUserRole;
        public bool HasAlliance => _currentAlliance != null && _currentAlliance.Id.IsValid;
        public IReadOnlyList<AllianceMember> CurrentMembers => _currentMembers;
        public IReadOnlyList<AllianceApplication> PendingApplications => _pendingApplications;

        public event Action<AllianceProfile> OnAllianceUpdated;
        public event Action<IReadOnlyList<AllianceMember>> OnMembersUpdated;
        public event Action<IReadOnlyList<AllianceApplication>> OnApplicationsUpdated;
        public event Action OnLeftAlliance;

        public AllianceService(
            IAllianceRepository repository,
            IEconomyService economyService = null,
            ILog log = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _economyService = economyService;
            _log = log ?? GameLog.Default;
        }

        public async Task<Result<bool, GameError>> RefreshCurrentAllianceAsync(CancellationToken ct = default)
        {
            _log.Debug(LogCat, "Refreshing current alliance profile and roster.");

            var result = await _repository.GetAllianceAsync(AllianceId.Empty, ct).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                if (result.Error.Code == "NOT_IN_ALLIANCE")
                {
                    _currentAlliance = null;
                    _currentUserRole = AllianceRole.None;
                    _currentMembers = Array.Empty<AllianceMember>();
                    _pendingApplications = Array.Empty<AllianceApplication>();
                    OnLeftAlliance?.Invoke();
                    return Result<bool, GameError>.Ok(false);
                }

                _log.Warning(LogCat, $"Failed to refresh alliance: {result.Error}");
                return Result<bool, GameError>.Fail(result.Error);
            }

            _currentAlliance = result.Value;

            // Fetch members
            var membersResult = await _repository.GetMembersAsync(_currentAlliance.Id, ct).ConfigureAwait(false);
            if (membersResult.IsSuccess)
            {
                _currentMembers = membersResult.Value;

                // Resolve caller's role
                if (_repository is LocalMockAllianceRepository mockRepo)
                {
                    var self = _currentMembers.FirstOrDefault(m => string.Equals(m.PlayerId, mockRepo.CurrentPlayerId, StringComparison.OrdinalIgnoreCase));
                    _currentUserRole = self?.Role ?? AllianceRole.None;
                }
                else
                {
                    _currentUserRole = AllianceRole.Member;
                }

                OnMembersUpdated?.Invoke(_currentMembers);
            }

            // Fetch applications if authorized
            if (_currentUserRole.CanManageApplications())
            {
                var appsResult = await _repository.GetApplicationsAsync(_currentAlliance.Id, ct).ConfigureAwait(false);
                if (appsResult.IsSuccess)
                {
                    _pendingApplications = appsResult.Value;
                    OnApplicationsUpdated?.Invoke(_pendingApplications);
                }
            }
            else
            {
                _pendingApplications = Array.Empty<AllianceApplication>();
                OnApplicationsUpdated?.Invoke(_pendingApplications);
            }

            OnAllianceUpdated?.Invoke(_currentAlliance);
            return Result<bool, GameError>.Ok(true);
        }

        public async Task<Result<IReadOnlyList<AllianceProfile>, GameError>> SearchAlliancesAsync(string query, AllianceJoinPolicy? policyFilter = null, CancellationToken ct = default)
        {
            return await _repository.SearchAlliancesAsync(query, policyFilter, ct).ConfigureAwait(false);
        }

        public async Task<Result<AllianceProfile, GameError>> CreateAllianceAsync(string name, string tag, string notice, AllianceJoinPolicy policy, CancellationToken ct = default)
        {
            _log.Info(LogCat, $"Initiating creation of alliance '{name}' [{tag}].");

            if (HasAlliance)
            {
                return Result<AllianceProfile, GameError>.Fail(GameError.AlreadyInAlliance());
            }

            if (_economyService != null)
            {
                if (!_economyService.CanAfford(AllianceCreationCostGold, 0, 0))
                {
                    return Result<AllianceProfile, GameError>.Fail(
                        GameError.InsufficientResources($"Founding an alliance requires {AllianceCreationCostGold:N0} Gold."));
                }
            }

            string requestId = Guid.NewGuid().ToString("N");

            if (_economyService != null)
            {
                var spendResult = await _economyService.RequestSpendAsync(
                    requestId,
                    AllianceCreationCostGold,
                    0,
                    0,
                    "alliance_creation",
                    ct).ConfigureAwait(false);

                if (!spendResult.IsSuccess)
                {
                    return Result<AllianceProfile, GameError>.Fail(spendResult.Error);
                }
            }

            var createResult = await _repository.CreateAllianceAsync(requestId, name, tag, notice, policy, ct).ConfigureAwait(false);
            if (!createResult.IsSuccess)
            {
                return Result<AllianceProfile, GameError>.Fail(createResult.Error);
            }

            _currentAlliance = createResult.Value;
            _currentUserRole = AllianceRole.Leader;

            var membersResult = await _repository.GetMembersAsync(_currentAlliance.Id, ct).ConfigureAwait(false);
            if (membersResult.IsSuccess)
            {
                _currentMembers = membersResult.Value;
                OnMembersUpdated?.Invoke(_currentMembers);
            }

            _pendingApplications = Array.Empty<AllianceApplication>();
            OnApplicationsUpdated?.Invoke(_pendingApplications);
            OnAllianceUpdated?.Invoke(_currentAlliance);

            return Result<AllianceProfile, GameError>.Ok(_currentAlliance);
        }

        public async Task<Result<bool, GameError>> JoinAllianceAsync(AllianceId allianceId, CancellationToken ct = default)
        {
            string requestId = Guid.NewGuid().ToString("N");
            var result = await _repository.JoinAllianceAsync(requestId, allianceId, ct).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                await RefreshCurrentAllianceAsync(ct).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<Result<bool, GameError>> ApplyToAllianceAsync(AllianceId allianceId, CancellationToken ct = default)
        {
            string requestId = Guid.NewGuid().ToString("N");
            return await _repository.ApplyToAllianceAsync(requestId, allianceId, ct).ConfigureAwait(false);
        }

        public async Task<Result<bool, GameError>> AcceptApplicationAsync(string applicationId, CancellationToken ct = default)
        {
            if (!HasAlliance) return Result<bool, GameError>.Fail(GameError.NotInAlliance());

            string requestId = Guid.NewGuid().ToString("N");
            var result = await _repository.AcceptApplicationAsync(requestId, _currentAlliance.Id, applicationId, ct).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                await RefreshCurrentAllianceAsync(ct).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<Result<bool, GameError>> RejectApplicationAsync(string applicationId, CancellationToken ct = default)
        {
            if (!HasAlliance) return Result<bool, GameError>.Fail(GameError.NotInAlliance());

            string requestId = Guid.NewGuid().ToString("N");
            var result = await _repository.RejectApplicationAsync(requestId, _currentAlliance.Id, applicationId, ct).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                var appsResult = await _repository.GetApplicationsAsync(_currentAlliance.Id, ct).ConfigureAwait(false);
                if (appsResult.IsSuccess)
                {
                    _pendingApplications = appsResult.Value;
                    OnApplicationsUpdated?.Invoke(_pendingApplications);
                }
            }
            return result;
        }

        public async Task<Result<bool, GameError>> KickMemberAsync(string targetPlayerId, CancellationToken ct = default)
        {
            if (!HasAlliance) return Result<bool, GameError>.Fail(GameError.NotInAlliance());

            string requestId = Guid.NewGuid().ToString("N");
            var result = await _repository.KickMemberAsync(requestId, _currentAlliance.Id, targetPlayerId, ct).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                await RefreshCurrentAllianceAsync(ct).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<Result<bool, GameError>> SetMemberRoleAsync(string targetPlayerId, AllianceRole newRole, CancellationToken ct = default)
        {
            if (!HasAlliance) return Result<bool, GameError>.Fail(GameError.NotInAlliance());

            string requestId = Guid.NewGuid().ToString("N");
            var result = await _repository.SetMemberRoleAsync(requestId, _currentAlliance.Id, targetPlayerId, newRole, ct).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                await RefreshCurrentAllianceAsync(ct).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<Result<bool, GameError>> LeaveAllianceAsync(CancellationToken ct = default)
        {
            if (!HasAlliance) return Result<bool, GameError>.Fail(GameError.NotInAlliance());

            string requestId = Guid.NewGuid().ToString("N");
            var result = await _repository.LeaveAllianceAsync(requestId, _currentAlliance.Id, ct).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                _currentAlliance = null;
                _currentUserRole = AllianceRole.None;
                _currentMembers = Array.Empty<AllianceMember>();
                _pendingApplications = Array.Empty<AllianceApplication>();
                OnLeftAlliance?.Invoke();
            }
            return result;
        }

        public async Task<Result<bool, GameError>> UpdateNoticeAsync(string newNotice, CancellationToken ct = default)
        {
            if (!HasAlliance) return Result<bool, GameError>.Fail(GameError.NotInAlliance());

            string requestId = Guid.NewGuid().ToString("N");
            var result = await _repository.UpdateNoticeAsync(requestId, _currentAlliance.Id, newNotice, ct).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                _currentAlliance = _currentAlliance.WithNotice(newNotice);
                OnAllianceUpdated?.Invoke(_currentAlliance);
            }
            return result;
        }
    }
}
