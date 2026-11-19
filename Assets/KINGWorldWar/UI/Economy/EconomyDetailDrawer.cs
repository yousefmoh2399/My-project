using System;
using KingWorldWar.Core.Economy;
using KingWorldWar.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Economy
{
    /// <summary>
    /// Slide-in detail drawer presenting hourly production yields, storage limits,
    /// time-to-full estimations, and explicit estimation disclaimers.
    /// </summary>
    public class EconomyDetailDrawer : MonoBehaviour
    {
        [Header("Root & Panels")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnRefresh;

        [Header("Gold Details")]
        [SerializeField] private TextMeshProUGUI _goldBalanceText;
        [SerializeField] private TextMeshProUGUI _goldRateText;
        [SerializeField] private TextMeshProUGUI _goldCapacityText;
        [SerializeField] private TextMeshProUGUI _goldTimeToFullText;
        [SerializeField] private Slider _goldProgressBar;

        [Header("Oil Details")]
        [SerializeField] private TextMeshProUGUI _oilBalanceText;
        [SerializeField] private TextMeshProUGUI _oilRateText;
        [SerializeField] private TextMeshProUGUI _oilCapacityText;
        [SerializeField] private TextMeshProUGUI _oilTimeToFullText;
        [SerializeField] private Slider _oilProgressBar;

        [Header("Iron Details")]
        [SerializeField] private TextMeshProUGUI _ironBalanceText;
        [SerializeField] private TextMeshProUGUI _ironRateText;
        [SerializeField] private TextMeshProUGUI _ironCapacityText;
        [SerializeField] private TextMeshProUGUI _ironTimeToFullText;
        [SerializeField] private Slider _ironProgressBar;

        [Header("Disclaimer / Info")]
        [SerializeField] private TextMeshProUGUI _disclaimerText;
        [SerializeField] private GameObject _mockBadgeRoot;
        [SerializeField] private TextMeshProUGUI _mockBadgeText;

        private IEconomyService _economyService;
        private ILocalizationService _locService;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;
        public Button BtnClose => _btnClose;
        public Button BtnRefresh => _btnRefresh;
        public TextMeshProUGUI TitleText => _titleText;
        public TextMeshProUGUI GoldBalanceText => _goldBalanceText;
        public TextMeshProUGUI OilBalanceText => _oilBalanceText;
        public TextMeshProUGUI IronBalanceText => _ironBalanceText;
        public GameObject MockBadgeRoot => _mockBadgeRoot;

        private void Awake()
        {
            if (_btnClose != null)
            {
                _btnClose.onClick.AddListener(Close);
            }

            if (_btnRefresh != null)
            {
                _btnRefresh.onClick.AddListener(HandleRefreshClicked);
            }
        }

        public void Bind(IEconomyService economyService, ILocalizationService locService)
        {
            Unsubscribe();
            _economyService = economyService;
            _locService = locService;

            if (_economyService != null)
            {
                _economyService.OnProjectionTick += HandleProjectionTick;
                _economyService.OnSnapshotUpdated += HandleSnapshotUpdated;
            }

            if (_locService != null)
            {
                _locService.OnLanguageChanged += HandleLanguageChanged;
            }

            UpdateDisplay();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_economyService != null)
            {
                _economyService.OnProjectionTick -= HandleProjectionTick;
                _economyService.OnSnapshotUpdated -= HandleSnapshotUpdated;
                _economyService = null;
            }

            if (_locService != null)
            {
                _locService.OnLanguageChanged -= HandleLanguageChanged;
                _locService = null;
            }
        }

        public void Open()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
            UpdateDisplay();
        }

        public void Close()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private async void HandleRefreshClicked()
        {
            if (_economyService != null)
            {
                await _economyService.RefreshSnapshotAsync();
                UpdateDisplay();
            }
        }

        private void HandleProjectionTick(EconomySnapshot projection)
        {
            if (IsVisible)
            {
                UpdateDisplay();
            }
        }

        private void HandleSnapshotUpdated(EconomySnapshot snapshot)
        {
            if (IsVisible)
            {
                UpdateDisplay();
            }
        }

        private void HandleLanguageChanged(Language lang)
        {
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_panelRoot == null || !_panelRoot.activeSelf) return;

            var projection = _economyService?.CurrentProjection ?? _economyService?.CurrentSnapshot;
            if (projection == null) return;

            var caps = projection.StorageCaps;
            var rates = projection.ProductionRatesPerHour;

            // Title & Disclaimer
            if (_titleText != null)
            {
                string raw = _locService != null ? _locService.GetString("res.drawer.title") : "Resource Economy & Production";
                _titleText.text = ArabicTextShaper.Shape(raw);
            }

            if (_disclaimerText != null)
            {
                string raw = _locService != null ? _locService.GetString("res.projection_estimate") : "* Continuous estimate";
                _disclaimerText.text = ArabicTextShaper.Shape(raw);
            }

            // Mock badge
#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
            if (_mockBadgeRoot != null)
            {
                _mockBadgeRoot.SetActive(_economyService?.IsMock ?? true);
                if (_mockBadgeText != null)
                {
                    string raw = _locService != null ? _locService.GetString("base.drawer.mock_badge") : "MOCK DATA (DEV ONLY)";
                    _mockBadgeText.text = ArabicTextShaper.Shape(raw);
                }
            }
#else
            if (_mockBadgeRoot != null) _mockBadgeRoot.SetActive(false);
#endif

            // Gold
            UpdateResourceRow(
                _goldBalanceText, _goldRateText, _goldCapacityText, _goldTimeToFullText, _goldProgressBar,
                projection.Gold, caps.GoldCap, rates.GoldPerHour, "res.gold");

            // Oil
            UpdateResourceRow(
                _oilBalanceText, _oilRateText, _oilCapacityText, _oilTimeToFullText, _oilProgressBar,
                projection.Oil, caps.OilCap, rates.OilPerHour, "res.oil");

            // Iron
            UpdateResourceRow(
                _ironBalanceText, _ironRateText, _ironCapacityText, _ironTimeToFullText, _ironProgressBar,
                projection.Iron, caps.IronCap, rates.IronPerHour, "res.iron");
        }

        private void UpdateResourceRow(
            TextMeshProUGUI balanceText,
            TextMeshProUGUI rateText,
            TextMeshProUGUI capacityText,
            TextMeshProUGUI timeText,
            Slider progressBar,
            long amount,
            long cap,
            long ratePerHour,
            string locKey)
        {
            if (balanceText != null)
            {
                balanceText.text = ResourceFormatter.FormatExact(amount);
            }

            if (rateText != null)
            {
                string fmt = _locService != null ? _locService.GetString("res.rate_per_hour") : "+{0}/hr";
                string formattedRate = string.Format(fmt, ResourceFormatter.Format(ratePerHour));
                rateText.text = ArabicTextShaper.Shape(formattedRate);
            }

            if (capacityText != null)
            {
                string fmt = _locService != null ? _locService.GetString("res.capacity") : "Capacity: {0} / {1}";
                string formattedCap = string.Format(fmt, ResourceFormatter.Format(amount), ResourceFormatter.Format(cap));
                capacityText.text = ArabicTextShaper.Shape(formattedCap);
            }

            if (timeText != null)
            {
                string timeStr = ResourceFormatter.FormatTimeToFull(amount, cap, ratePerHour);
                string fmt = _locService != null ? _locService.GetString("res.time_to_full") : "Full in: {0}";
                string formattedTime = string.Format(fmt, timeStr);
                timeText.text = ArabicTextShaper.Shape(formattedTime);
            }

            if (progressBar != null)
            {
                progressBar.minValue = 0;
                progressBar.maxValue = 1;
                progressBar.value = cap > 0 ? Mathf.Clamp01((float)amount / cap) : 0f;
            }
        }

        // Test helper
        public void SetupComponentsForTest(
            GameObject root,
            TextMeshProUGUI title,
            Button btnClose,
            Button btnRefresh,
            TextMeshProUGUI goldBal,
            TextMeshProUGUI oilBal,
            TextMeshProUGUI ironBal,
            GameObject mockBadge = null,
            TextMeshProUGUI mockText = null)
        {
            _panelRoot = root;
            _titleText = title;
            _btnClose = btnClose;
            _btnRefresh = btnRefresh;
            _goldBalanceText = goldBal;
            _oilBalanceText = oilBal;
            _ironBalanceText = ironBal;
            _mockBadgeRoot = mockBadge;
            _mockBadgeText = mockText;

            if (_btnClose != null)
            {
                _btnClose.onClick.RemoveAllListeners();
                _btnClose.onClick.AddListener(Close);
            }

            if (_btnRefresh != null)
            {
                _btnRefresh.onClick.RemoveAllListeners();
                _btnRefresh.onClick.AddListener(HandleRefreshClicked);
            }
        }
    }
}
