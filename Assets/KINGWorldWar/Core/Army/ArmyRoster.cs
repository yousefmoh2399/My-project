using System;
using System.Collections.Generic;
using System.Linq;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Pure C# model representing player's mobilized military forces.
    /// Tracks unit counts per archetype and evaluates composite combat rating.
    /// </summary>
    [Serializable]
    public sealed class ArmyRoster
    {
        private readonly Dictionary<UnitType, int> _counts = new Dictionary<UnitType, int>();

        public IReadOnlyDictionary<UnitType, int> Counts => _counts;

        public int TotalTroops
        {
            get
            {
                var sum = 0;
                foreach (var kvp in _counts)
                {
                    sum += kvp.Value;
                }
                return sum;
            }
        }

        public ArmyRoster()
        {
            foreach (UnitType t in Enum.GetValues(typeof(UnitType)))
            {
                _counts[t] = 0;
            }
        }

        public ArmyRoster(IDictionary<UnitType, int> initialCounts) : this()
        {
            if (initialCounts == null) return;
            foreach (var kvp in initialCounts)
            {
                _counts[kvp.Key] = Math.Max(0, kvp.Value);
            }
        }

        public int GetCount(UnitType type) =>
            _counts.TryGetValue(type, out var count) ? count : 0;

        public void SetCount(UnitType type, int count)
        {
            _counts[type] = Math.Max(0, count);
        }

        public void AddUnits(UnitType type, int count)
        {
            if (count <= 0) return;
            _counts[type] = GetCount(type) + count;
        }

        public bool TryRemoveUnits(UnitType type, int count)
        {
            if (count <= 0) return true;
            var current = GetCount(type);
            if (current < count) return false;
            _counts[type] = current - count;
            return true;
        }

        public int CalculateTotalCombatPower(IArmyDataSource dataSource)
        {
            if (dataSource == null) return 0;

            var totalPower = 0;
            foreach (var kvp in _counts)
            {
                if (kvp.Value <= 0) continue;
                var def = dataSource.GetDefinition(kvp.Key);
                if (def != null)
                {
                    totalPower += def.Stats.CombatPower * kvp.Value;
                }
            }
            return totalPower;
        }

        public ArmyRoster Clone() => new ArmyRoster(_counts);

        public static ArmyRoster FromSnapshot(ArmyRosterSnapshot snapshot)
        {
            if (snapshot == null) return new ArmyRoster();
            return new ArmyRoster(snapshot.TotalOwned.ToDictionary(k => k.Key, v => v.Value));
        }

        public ArmyRosterSnapshot ToSnapshot(string playerId = "player_default", long revision = 1, string balanceVersion = "1.0.0")
        {
            return new ArmyRosterSnapshot(
                playerId,
                revision,
                balanceVersion,
                DateTime.UtcNow,
                _counts,
                new Dictionary<UnitType, int>(),
                new Dictionary<UnitType, int>()
            );
        }
    }
}
