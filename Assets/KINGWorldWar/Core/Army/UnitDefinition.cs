using System;
using KingWorldWar.Core.Base;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Pure C# immutable definition for a military unit archetype.
    /// Encapsulates tactical stats, resource costs, and base building prerequisites.
    /// </summary>
    [Serializable]
    public sealed class UnitDefinition
    {
        public UnitId Id { get; }
        public UnitType Type { get; }
        public string NameKey { get; }
        public string DescKey { get; }
        public string RoleKey { get; }
        public UnitStats Stats { get; }
        public UnitCost Cost { get; }
        public BuildingType RequiredBuildingType { get; }
        public int RequiredBuildingLevel { get; }
        public int BatchSize { get; }

        public UnitDefinition(
            UnitId id,
            UnitType type,
            string nameKey,
            string descKey,
            string roleKey,
            UnitStats stats,
            UnitCost cost,
            BuildingType requiredBuildingType,
            int requiredBuildingLevel,
            int batchSize = 1)
        {
            if (id.IsEmpty) throw new ArgumentException("UnitId cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(nameKey)) throw new ArgumentException("NameKey cannot be null or whitespace.", nameof(nameKey));
            if (string.IsNullOrWhiteSpace(descKey)) throw new ArgumentException("DescKey cannot be null or whitespace.", nameof(descKey));
            if (string.IsNullOrWhiteSpace(roleKey)) throw new ArgumentException("RoleKey cannot be null or whitespace.", nameof(roleKey));

            Id = id;
            Type = type;
            NameKey = nameKey;
            DescKey = descKey;
            RoleKey = roleKey;
            Stats = stats;
            Cost = cost;
            RequiredBuildingType = requiredBuildingType;
            RequiredBuildingLevel = Math.Max(1, requiredBuildingLevel);
            BatchSize = Math.Max(1, batchSize);
        }

        public void Validate()
        {
            if (Id.IsEmpty) throw new InvalidOperationException("UnitDefinition has an empty UnitId.");
            if (Stats.Health <= 0) throw new InvalidOperationException($"Unit {Id} has invalid Health: {Stats.Health}");
            if (Stats.Attack <= 0) throw new InvalidOperationException($"Unit {Id} has invalid Attack: {Stats.Attack}");
            if (Stats.Speed <= 0f) throw new InvalidOperationException($"Unit {Id} has invalid Speed: {Stats.Speed}");
            if (Stats.Range <= 0f) throw new InvalidOperationException($"Unit {Id} has invalid Range: {Stats.Range}");
            if (Cost.RecruitDurationSeconds <= 0) throw new InvalidOperationException($"Unit {Id} has non-positive recruit duration.");
            if (RequiredBuildingLevel < 1) throw new InvalidOperationException($"Unit {Id} has invalid required building level: {RequiredBuildingLevel}");
        }

        public override string ToString() => $"UnitDefinition [{Id}] {Type} (Req: {RequiredBuildingType} Lvl {RequiredBuildingLevel})";
    }
}
