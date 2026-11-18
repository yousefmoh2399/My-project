using System;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.Quality;
using UnityEngine;

namespace KingWorldWar.Infrastructure.Quality
{
    /// <summary>
    /// Implements visual quality tier selection with hardware heuristics and persistent user preferences.
    /// </summary>
    public class QualityService : IQualityService
    {
        public const string QualityPrefKey = "kww_quality_tier";

        private readonly ILog _log;
        private QualityTier _currentTier;
        private QualitySettingsProfile _currentProfile;

        public QualityTier CurrentTier => _currentTier;
        public QualitySettingsProfile CurrentProfile => _currentProfile;

        public event Action<QualityTier> OnQualityTierChanged;

        public QualityService(ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void Initialize()
        {
            QualityTier selectedTier;

            if (PlayerPrefs.HasKey(QualityPrefKey))
            {
                int saved = PlayerPrefs.GetInt(QualityPrefKey);
                selectedTier = (QualityTier)Mathf.Clamp(saved, (int)QualityTier.Low, (int)QualityTier.High);
                _log.Info("QualityService", $"Loaded persisted quality preference: {selectedTier}");
            }
            else
            {
                selectedTier = EvaluateOptimalTier(
                    SystemInfo.systemMemorySize,
                    SystemInfo.graphicsMemorySize,
                    SystemInfo.processorCount);
                _log.Info("QualityService", $"Evaluated automatic hardware tier: {selectedTier} (RAM: {SystemInfo.systemMemorySize}MB, VRAM: {SystemInfo.graphicsMemorySize}MB, Cores: {SystemInfo.processorCount})");
            }

            ApplyTier(selectedTier, persist: false);
        }

        public void SetQualityTier(QualityTier tier, bool persist = true)
        {
            ApplyTier(tier, persist);
        }

        public QualityTier EvaluateOptimalTier(int systemMemoryMb, int graphicsMemoryMb, int processorCount)
        {
            // Low-end mobile threshold: < 3GB RAM, < 1GB VRAM, or <= 4 cores
            if (systemMemoryMb > 0 && systemMemoryMb < 3000)
            {
                return QualityTier.Low;
            }

            if (graphicsMemoryMb > 0 && graphicsMemoryMb < 1024)
            {
                return QualityTier.Low;
            }

            if (processorCount > 0 && processorCount <= 4)
            {
                return QualityTier.Low;
            }

            // High-end mobile threshold: >= 6GB RAM, >= 2GB VRAM, >= 8 cores
            if (systemMemoryMb >= 6000 && (graphicsMemoryMb <= 0 || graphicsMemoryMb >= 2048) && processorCount >= 8)
            {
                return QualityTier.High;
            }

            // Default mid-range
            return QualityTier.Medium;
        }

        private void ApplyTier(QualityTier tier, bool persist)
        {
            _currentTier = tier;
            _currentProfile = QualitySettingsProfile.CreateDefault(tier);

            // Apply frame rate
            Application.targetFrameRate = _currentProfile.TargetFrameRate;

            // Map tier to Unity quality level index if within range
            int targetLevel = Mathf.Clamp((int)tier, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            if (QualitySettings.GetQualityLevel() != targetLevel && QualitySettings.names.Length > 0)
            {
                QualitySettings.SetQualityLevel(targetLevel, applyExpensiveChanges: true);
            }

            if (persist)
            {
                PlayerPrefs.SetInt(QualityPrefKey, (int)tier);
                PlayerPrefs.Save();
                _log.Info("QualityService", $"Persisted quality tier: {tier}");
            }

            _log.Info("QualityService", $"Applied Quality Tier: {tier} (FPS: {_currentProfile.TargetFrameRate}, RenderScale: {_currentProfile.RenderScale}, ShadowDist: {_currentProfile.ShadowDistance}m)");
            OnQualityTierChanged?.Invoke(tier);
        }
    }
}
