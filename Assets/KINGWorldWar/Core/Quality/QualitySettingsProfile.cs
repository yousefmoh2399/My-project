using System;

namespace KingWorldWar.Core.Quality
{
    /// <summary>
    /// Pure data specification for a visual quality tier profile.
    /// </summary>
    [Serializable]
    public class QualitySettingsProfile
    {
        public QualityTier Tier { get; set; }
        public int TargetFrameRate { get; set; }
        public float RenderScale { get; set; }
        public float ShadowDistance { get; set; }
        public int MsaaSamples { get; set; }
        public bool SoftShadowsEnabled { get; set; }
        public bool SrpBatcherEnabled { get; set; }

        public QualitySettingsProfile(
            QualityTier tier,
            int targetFrameRate,
            float renderScale,
            float shadowDistance,
            int msaaSamples,
            bool softShadowsEnabled,
            bool srpBatcherEnabled = true)
        {
            Tier = tier;
            TargetFrameRate = targetFrameRate;
            RenderScale = renderScale;
            ShadowDistance = shadowDistance;
            MsaaSamples = msaaSamples;
            SoftShadowsEnabled = softShadowsEnabled;
            SrpBatcherEnabled = srpBatcherEnabled;
        }

        public static QualitySettingsProfile CreateDefault(QualityTier tier)
        {
            switch (tier)
            {
                case QualityTier.Low:
                    return new QualitySettingsProfile(
                        tier: QualityTier.Low,
                        targetFrameRate: 30,
                        renderScale: 0.8f,
                        shadowDistance: 20f,
                        msaaSamples: 1,
                        softShadowsEnabled: false,
                        srpBatcherEnabled: true);

                case QualityTier.Medium:
                    return new QualitySettingsProfile(
                        tier: QualityTier.Medium,
                        targetFrameRate: 60,
                        renderScale: 1.0f,
                        shadowDistance: 45f,
                        msaaSamples: 2,
                        softShadowsEnabled: true,
                        srpBatcherEnabled: true);

                case QualityTier.High:
                default:
                    return new QualitySettingsProfile(
                        tier: QualityTier.High,
                        targetFrameRate: 60,
                        renderScale: 1.0f,
                        shadowDistance: 75f,
                        msaaSamples: 4,
                        softShadowsEnabled: true,
                        srpBatcherEnabled: true);
            }
        }
    }
}
