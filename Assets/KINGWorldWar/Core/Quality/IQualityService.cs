using System;

namespace KingWorldWar.Core.Quality
{
    /// <summary>
    /// Service contract for managing visual quality tiers and hardware-specific scaling.
    /// </summary>
    public interface IQualityService
    {
        QualityTier CurrentTier { get; }
        QualitySettingsProfile CurrentProfile { get; }
        event Action<QualityTier> OnQualityTierChanged;

        void Initialize();
        void SetQualityTier(QualityTier tier, bool persist = true);
        QualityTier EvaluateOptimalTier(int systemMemoryMb, int graphicsMemoryMb, int processorCount);
    }
}
