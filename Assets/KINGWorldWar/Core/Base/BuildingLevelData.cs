using System;

namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Pure C# immutable specification for a specific upgrade level of a building.
    /// Non-authoritative display model on client; authoritative formulas reside on the server.
    /// </summary>
    [Serializable]
    public sealed class BuildingLevelData
    {
        public int Level { get; }
        public BuildingCost Cost { get; }
        public int UpgradeDurationSeconds { get; }
        public int RequiredHQLevel { get; }
        public long OutputValue { get; }
        public string OutputFormatKey { get; }

        public BuildingLevelData(
            int level,
            BuildingCost cost,
            int upgradeDurationSeconds,
            int requiredHQLevel,
            long outputValue = 0,
            string outputFormatKey = null)
        {
            if (level < 1)
                throw new ArgumentOutOfRangeException(nameof(level), "Level must be >= 1.");

            Level = level;
            Cost = cost;
            UpgradeDurationSeconds = Math.Max(0, upgradeDurationSeconds);
            RequiredHQLevel = Math.Max(0, requiredHQLevel);
            OutputValue = outputValue;
            OutputFormatKey = outputFormatKey ?? string.Empty;
        }

        public override string ToString() =>
            $"Level {Level} (Cost: {Cost}, Time: {UpgradeDurationSeconds}s, ReqHQ: {RequiredHQLevel})";
    }
}
