using System;
using KingWorldWar.Core.Economy;
using KingWorldWar.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Common
{
    /// <summary>
    /// Displays non-authoritative client readout of primary game resources (Gold, Oil, Iron).
    /// Uses ResourceFormatter for culture-aware K/M abbreviations, color-coded threshold alerts,
    /// and stale data indicators.
    /// </summary>
    public class ResourceHeaderView : MonoBehaviour
    {
        [Header("Text Displays")]
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _oilText;
        [SerializeField] private TextMeshProUGUI _ironText;

        [Header("Optional Click Button")]
        [SerializeField] private Button _headerButton;

        private IResourceState _legacyState;
        private IEconomyService _economyService;

        public TextMeshProUGUI GoldText => _goldText;
        public TextMeshProUGUI OilText => _oilText;
        public TextMeshProUGUI IronText => _ironText;
        public Button HeaderButton => _headerButton;

        public event Action OnHeaderClicked;

        private static readonly Color ColorNormal = new Color(0.96f, 0.90f, 0.70f, 1f); // Restrained Gold / Off-white
        private static readonly Color ColorZero = new Color(0.55f, 0.55f, 0.55f, 1f);   // Muted Gray
        private static readonly Color ColorNearCap = new Color(1f, 0.75f, 0.15f, 1f);    // Warning Amber/Yellow
        private static readonly Color ColorCapped = new Color(0.95f, 0.25f, 0.25f, 1f);   // Alert Red
        private static readonly Color ColorStale = new Color(0.70f, 0.60f, 0.35f, 1f);    // Dimmed Stale

        private void Awake()
        {
            if (_headerButton != null)
            {
                _headerButton.onClick.AddListener(() => OnHeaderClicked?.Invoke());
            }
        }

        public void BindEconomyService(IEconomyService economyService)
        {
            UnsubscribeEvents();
            _economyService = economyService;

            if (_economyService != null)
            {
                _economyService.OnProjectionTick += HandleProjectionTick;
                _economyService.OnSnapshotUpdated += HandleSnapshotUpdated;
            }

            UpdateDisplay();
        }

        public void Bind(IResourceState state)
        {
            UnsubscribeEvents();
            _legacyState = state;

            if (_legacyState != null)
            {
                _legacyState.OnResourcesChanged += UpdateDisplay;
            }

            UpdateDisplay();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void UnsubscribeEvents()
        {
            if (_economyService != null)
            {
                _economyService.OnProjectionTick -= HandleProjectionTick;
                _economyService.OnSnapshotUpdated -= HandleSnapshotUpdated;
                _economyService = null;
            }

            if (_legacyState != null)
            {
                _legacyState.OnResourcesChanged -= UpdateDisplay;
                _legacyState = null;
            }
        }

        private void HandleProjectionTick(EconomySnapshot projection)
        {
            UpdateDisplay();
        }

        private void HandleSnapshotUpdated(EconomySnapshot snapshot)
        {
            UpdateDisplay();
        }

        public void SetManualValues(long gold, long oil, long iron)
        {
            ApplyFormattedText(_goldText, gold, 0, false);
            ApplyFormattedText(_oilText, oil, 0, false);
            ApplyFormattedText(_ironText, iron, 0, false);
        }

        private void UpdateDisplay()
        {
            if (_economyService != null)
            {
                var projection = _economyService.CurrentProjection ?? _economyService.CurrentSnapshot;
                var caps = projection?.StorageCaps ?? default;
                bool isStale = _economyService.IsStale;

                long g = projection?.Gold ?? 0;
                long o = projection?.Oil ?? 0;
                long i = projection?.Iron ?? 0;

                ApplyFormattedText(_goldText, g, caps.GoldCap, isStale);
                ApplyFormattedText(_oilText, o, caps.OilCap, isStale);
                ApplyFormattedText(_ironText, i, caps.IronCap, isStale);
            }
            else if (_legacyState != null)
            {
                ApplyFormattedText(_goldText, _legacyState.Gold, 0, false);
                ApplyFormattedText(_oilText, _legacyState.Oil, 0, false);
                ApplyFormattedText(_ironText, _legacyState.Iron, 0, false);
            }
        }

        private static void ApplyFormattedText(TextMeshProUGUI label, long amount, long cap, bool isStale)
        {
            if (label == null) return;

            label.text = ResourceFormatter.Format(amount);
            var state = ResourceFormatter.EvaluateVisualState(amount, cap, isStale);

            label.color = state switch
            {
                ResourceVisualState.Zero => ColorZero,
                ResourceVisualState.NearCap => ColorNearCap,
                ResourceVisualState.Capped => ColorCapped,
                ResourceVisualState.Stale => ColorStale,
                _ => ColorNormal
            };
        }

        // Test helper
        public void SetupComponentsForTest(TextMeshProUGUI gold, TextMeshProUGUI oil, TextMeshProUGUI iron, Button btn = null)
        {
            _goldText = gold;
            _oilText = oil;
            _ironText = iron;
            _headerButton = btn;
        }
    }
}
