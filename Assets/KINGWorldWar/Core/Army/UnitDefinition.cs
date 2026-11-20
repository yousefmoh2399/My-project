using System;
using System.Collections.Generic;
using KingWorldWar.Core.Base;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Pure C# immutable definition for a military unit archetype.
    /// Encapsulates tactical stats, resource costs, base building prerequisites, and metadata.
    /// Classifies each attribute according to its authority model (DisplayOnly vs ServerAuthoritative).
    /// </summary>
    [Serializable]
    public sealed class UnitDefinition
    {
        // Identification & Category
        public UnitId Id { get; }
        public UnitType Type { get; }
        public UnitCategory Category { get; }
        public int Tier { get; }

        // Presentation Metadata [DisplayOnly]
        public string NameKey { get; }
        public string DescKey { get; }
        public string RoleKey { get; }
        public string IconAssetPath { get; }
        public string ModelAssetPath { get; }
        public string NextLevelBenefitsKey { get; }

        // Combat & Physical Properties [ServerAuthoritative]
        public UnitStats Stats { get; }
        public UnitCost Cost { get; }
        public BuildingType RequiredBuildingType { get; }
        public int RequiredBuildingLevel { get; }
        public int BatchSize { get; }

        // Upgrade Presentation [ServerAuthoritative Decided, Client Display]
        public UnitCost NextLevelUpgradeCost { get; }
        public int NextLevelUpgradeDurationSeconds { get; }

        public UnitDefinition(
            UnitId id,
            UnitType type,
            UnitCategory category,
            string nameKey,
            string descKey,
            string roleKey,
            UnitStats stats,
            UnitCost cost,
            BuildingType requiredBuildingType,
            int requiredBuildingLevel,
            int tier = 1,
            int batchSize = 1,
            string iconAssetPath = "",
            string modelAssetPath = "",
            string nextLevelBenefitsKey = "",
            UnitCost? nextLevelUpgradeCost = null,
            int nextLevelUpgradeDurationSeconds = 0)
        {
            if (id.IsEmpty) throw new ArgumentException("UnitId cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(nameKey)) throw new ArgumentException("NameKey cannot be null or whitespace.", nameof(nameKey));
            if (string.IsNullOrWhiteSpace(descKey)) throw new ArgumentException("DescKey cannot be null or whitespace.", nameof(descKey));
            if (string.IsNullOrWhiteSpace(roleKey)) throw new ArgumentException("RoleKey cannot be null or whitespace.", nameof(roleKey));

            Id = id;
            Type = type;
            Category = category;
            NameKey = nameKey;
            DescKey = descKey;
            RoleKey = roleKey;
            Stats = stats;
            Cost = cost;
            RequiredBuildingType = requiredBuildingType;
            RequiredBuildingLevel = Math.Max(1, requiredBuildingLevel);
            Tier = Math.Max(1, tier);
            BatchSize = Math.Max(1, batchSize);
            IconAssetPath = iconAssetPath ?? string.Empty;
            ModelAssetPath = modelAssetPath ?? string.Empty;
            NextLevelBenefitsKey = nextLevelBenefitsKey ?? string.Empty;
            NextLevelUpgradeCost = nextLevelUpgradeCost ?? new UnitCost(0, 0, 0, 0);
            NextLevelUpgradeDurationSeconds = Math.Max(0, nextLevelUpgradeDurationSeconds);
        }

        public void Validate()
        {
            if (Id.IsEmpty) throw new InvalidOperationException("UnitDefinition has an empty UnitId.");
            if (Stats.Health <= 0) throw new InvalidOperationException($"Unit {Id} has invalid Health: {Stats.Health}");
            if (Stats.Attack <= 0) throw new InvalidOperationException($"Unit {Id} has invalid Attack: {Stats.Attack}");
            if (Stats.Speed <= 0f) throw new InvalidOperationException($"Unit {Id} has invalid Speed: {Stats.Speed}");
            if (Stats.Range <= 0f) throw new InvalidOperationException($"Unit {Id} has invalid Range: {Stats.Range}");
            if (Stats.CapacityWeight <= 0) throw new InvalidOperationException($"Unit {Id} has invalid CapacityWeight: {Stats.CapacityWeight}");
            if (Cost.RecruitDurationSeconds <= 0) throw new InvalidOperationException($"Unit {Id} has non-positive recruit duration.");
            if (RequiredBuildingLevel < 1) throw new InvalidOperationException($"Unit {Id} has invalid required building level: {RequiredBuildingLevel}");
        }

        /// <summary>
        /// Evaluates whether an attribute of a unit is DisplayOnly or ServerAuthoritative.
        /// </summary>
        public static StatAuthority GetStatAuthority(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(NameKey):
                case nameof(DescKey):
                case nameof(RoleKey):
                case nameof(IconAssetPath):
                case nameof(ModelAssetPath):
                case nameof(NextLevelBenefitsKey):
                    return StatAuthority.DisplayOnly;

                case nameof(Stats):
                case nameof(Cost):
                case nameof(RequiredBuildingType):
                case nameof(RequiredBuildingLevel):
                case nameof(Tier):
                case nameof(BatchSize):
                case nameof(NextLevelUpgradeCost):
                case nameof(NextLevelUpgradeDurationSeconds):
                    return StatAuthority.ServerAuthoritative;

                default:
                    return StatAuthority.ServerAuthoritative;
            }
        }

        public override string ToString() => $"UnitDefinition [{Id}] {Type} ({Category}, Req: {RequiredBuildingType} Lvl {RequiredBuildingLevel})";
    }
}
