using System.Text;
using KingWorldWar.Core.Quality;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.DebugOverlay
{
    /// <summary>
    /// Lightweight diagnostic HUD for visual testing and performance verification.
    /// Active exclusively in development builds and Editor.
    /// </summary>
    public class DebugOverlayView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private Button _tierLowButton;
        [SerializeField] private Button _tierMediumButton;
        [SerializeField] private Button _tierHighButton;

        private IQualityService _qualityService;
        private float _fpsAccumulator;
        private int _fpsFrames;
        private float _fpsTimeLeft = 0.5f;
        private float _currentFps;
        private readonly StringBuilder _builder = new StringBuilder(256);

        public void Bind(IQualityService qualityService)
        {
            _qualityService = qualityService;
        }

        private void Awake()
        {
#if !DEBUG && !UNITY_EDITOR
            gameObject.SetActive(false);
            return;
#endif
            if (_tierLowButton != null)
                _tierLowButton.onClick.AddListener(() => OnTierButtonClicked(QualityTier.Low));
            if (_tierMediumButton != null)
                _tierMediumButton.onClick.AddListener(() => OnTierButtonClicked(QualityTier.Medium));
            if (_tierHighButton != null)
                _tierHighButton.onClick.AddListener(() => OnTierButtonClicked(QualityTier.High));
        }

        private void OnDestroy()
        {
            if (_tierLowButton != null) _tierLowButton.onClick.RemoveAllListeners();
            if (_tierMediumButton != null) _tierMediumButton.onClick.RemoveAllListeners();
            if (_tierHighButton != null) _tierHighButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrames++;
            _fpsTimeLeft -= Time.unscaledDeltaTime;

            if (_fpsTimeLeft <= 0f)
            {
                _currentFps = _fpsFrames / Mathf.Max(0.0001f, _fpsAccumulator);
                _fpsTimeLeft = 0.5f;
                _fpsAccumulator = 0f;
                _fpsFrames = 0;
                RefreshDisplay();
            }
        }

        private void OnTierButtonClicked(QualityTier tier)
        {
            if (_qualityService != null)
            {
                _qualityService.SetQualityTier(tier, persist: true);
            }
            else
            {
                QualitySettings.SetQualityLevel((int)tier, true);
            }
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if (_statsText == null) return;

            string tierName = _qualityService != null ? _qualityService.CurrentTier.ToString() : QualitySettings.names[QualitySettings.GetQualityLevel()];
            float renderScale = _qualityService?.CurrentProfile?.RenderScale ?? 1.0f;
            int targetFps = Application.targetFrameRate;

            _builder.Clear();
            _builder.AppendLine("<b>KING WORLD WAR [VISUAL DEV]</b>");
            _builder.Append("FPS: ");
            if (_currentFps >= 50f)
                _builder.Append("<color=#4CAF50>");
            else if (_currentFps >= 30f)
                _builder.Append("<color=#FFEB3B>");
            else
                _builder.Append("<color=#F44336>");
            _builder.Append(_currentFps.ToString("F1")).Append("</color> (Target: ").Append(targetFps).AppendLine(")");
            _builder.Append("Tier: <b>").Append(tierName).AppendLine("</b>");
            _builder.Append("Res: ").Append(Screen.width).Append("x").Append(Screen.height);
            _builder.Append(" | Scale: ").Append(renderScale.ToString("F2"));

            _statsText.text = _builder.ToString();
        }
    }
}
