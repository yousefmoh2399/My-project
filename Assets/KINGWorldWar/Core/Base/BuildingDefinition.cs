using System;
using System.Collections.Generic;

namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Pure C# immutable definition for a building archetype.
    /// Defines physical footprint on the tactical grid, maximum level, and upgrade requirements.
    /// </summary>
    public sealed class BuildingDefinition
    {
        public BuildingType Type { get; }
        public string NameLocalizationKey { get; }
        public string DescriptionLocalizationKey { get; }
        public int FootprintWidth { get; }
        public int FootprintHeight { get; }
        public int MaxLevel { get; }

        private readonly Dictionary<int, BuildingLevelData> _levels;
        public IReadOnlyDictionary<int, BuildingLevelData> Levels => _levels;

        public BuildingDefinition(
            BuildingType type,
            string nameLocalizationKey,
            string descriptionLocalizationKey,
            int footprintWidth,
            int footprintHeight,
            IEnumerable<BuildingLevelData> levels)
        {
            Type = type;
            NameLocalizationKey = nameLocalizationKey ?? throw new ArgumentNullException(nameof(nameLocalizationKey));
            DescriptionLocalizationKey = descriptionLocalizationKey ?? throw new ArgumentNullException(nameof(descriptionLocalizationKey));

            if (footprintWidth <= 0 || footprintHeight <= 0)
                throw new ArgumentException("Footprint dimensions must be positive integers.");

            FootprintWidth = footprintWidth;
            FootprintHeight = footprintHeight;

            _levels = new Dictionary<int, BuildingLevelData>();
            int max = 0;
            if (levels != null)
            {
                foreach (var lvl in levels)
                {
                    if (lvl == null) continue;
                    _levels[lvl.Level] = lvl;
                    if (lvl.Level > max) max = lvl.Level;
                }
            }

            MaxLevel = max;
        }

        public bool TryGetLevelData(int level, out BuildingLevelData data)
        {
            return _levels.TryGetValue(level, out data);
        }

        public BuildingLevelData GetLevelData(int level)
        {
            if (_levels.TryGetValue(level, out var data))
                return data;
            throw new KeyNotFoundException($"Level {level} data not found for building type {Type}.");
        }

        public override string ToString() =>
            $"{Type} [Size: {FootprintWidth}x{FootprintHeight}, MaxLv: {MaxLevel}]";
    }
}
