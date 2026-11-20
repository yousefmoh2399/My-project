using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Immutable server-authoritative snapshot of a player's army forces.
    /// Tracks total owned, currently training, and actively deployed units across all archetypes.
    /// Includes revision number and server balanceVersion for optimistic concurrency and rule validation.
    /// </summary>
    public sealed class ArmyRosterSnapshot : IEquatable<ArmyRosterSnapshot>
    {
        public string PlayerId { get; }
        public long Revision { get; }
        public string BalanceVersion { get; }
        public DateTime ServerTimeUtc { get; }

        public IReadOnlyDictionary<UnitType, int> TotalOwned { get; }
        public IReadOnlyDictionary<UnitType, int> InTraining { get; }
        public IReadOnlyDictionary<UnitType, int> Deployed { get; }

        public ArmyRosterSnapshot(
            string playerId,
            long revision,
            string balanceVersion,
            DateTime serverTimeUtc,
            IDictionary<UnitType, int> totalOwned,
            IDictionary<UnitType, int> inTraining = null,
            IDictionary<UnitType, int> deployed = null)
        {
            PlayerId = playerId ?? string.Empty;
            Revision = Math.Max(0, revision);
            BalanceVersion = string.IsNullOrWhiteSpace(balanceVersion) ? "1.0.0" : balanceVersion.Trim();
            ServerTimeUtc = serverTimeUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(serverTimeUtc, DateTimeKind.Utc)
                : serverTimeUtc.ToUniversalTime();

            TotalOwned = new ReadOnlyDictionary<UnitType, int>(
                totalOwned != null ? new Dictionary<UnitType, int>(totalOwned) : new Dictionary<UnitType, int>());

            InTraining = new ReadOnlyDictionary<UnitType, int>(
                inTraining != null ? new Dictionary<UnitType, int>(inTraining) : new Dictionary<UnitType, int>());

            Deployed = new ReadOnlyDictionary<UnitType, int>(
                deployed != null ? new Dictionary<UnitType, int>(deployed) : new Dictionary<UnitType, int>());
        }

        public int GetTotalCount(UnitType type) =>
            TotalOwned.TryGetValue(type, out int count) ? Math.Max(0, count) : 0;

        public int GetTotalOwnedCount(UnitType type) => GetTotalCount(type);

        public int GetInTrainingCount(UnitType type) =>
            InTraining.TryGetValue(type, out int count) ? Math.Max(0, count) : 0;

        public int GetDeployedCount(UnitType type) =>
            Deployed.TryGetValue(type, out int count) ? Math.Max(0, count) : 0;

        public int GetAvailableCount(UnitType type)
        {
            int total = GetTotalCount(type);
            int training = GetInTrainingCount(type);
            int deployed = GetDeployedCount(type);
            return Math.Max(0, total - training - deployed);
        }

        public int TotalForcesCount => TotalOwned.Values.Sum();
        public int GetTotalForcesCount() => TotalForcesCount;

        public int CalculateTotalCombatPower(IArmyDataSource dataSource)
        {
            if (dataSource == null) return 0;
            int power = 0;
            foreach (var kvp in TotalOwned)
            {
                var def = dataSource.GetUnit(kvp.Key);
                if (def != null)
                {
                    power += def.Stats.CombatPower * kvp.Value;
                }
            }
            return power;
        }

        public static ArmyRosterSnapshot CreateEmpty(string playerId = "player_default")
        {
            return new ArmyRosterSnapshot(
                playerId,
                1,
                "1.0.0",
                DateTime.UtcNow,
                new Dictionary<UnitType, int>(),
                new Dictionary<UnitType, int>(),
                new Dictionary<UnitType, int>()
            );
        }

        public bool Equals(ArmyRosterSnapshot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(PlayerId, other.PlayerId, StringComparison.Ordinal) &&
                   Revision == other.Revision &&
                   string.Equals(BalanceVersion, other.BalanceVersion, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || (obj is ArmyRosterSnapshot other && Equals(other));

        public override int GetHashCode() =>
            HashCode.Combine(PlayerId, Revision, BalanceVersion);
    }
}
