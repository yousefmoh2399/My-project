using System;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Base
{
    /// <summary>
    /// UI drawer displaying structure intelligence, level progression, resource costs,
    /// and upgrade execution for the selected base building.
    /// </summary>
    public class BuildingDetailDrawer : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Header Texts")]
        [SerializeField] private TextMeshProUGUI _buildingNameText;
        [SerializeField] private TextMeshProUGUI _levelBadgeText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private GameObject _mockDataBadgeRoot;
        [SerializeField] private TextMeshProUGUI _mockDataBadgeText;

        [Header("Performance Stats")]
        [SerializeField] private TextMeshProUGUI _currentPerkText;
        [SerializeField] private TextMeshProUGUI _nextPerkText;

        [Header("Upgrade Costs & Prerequisites")]
        [SerializeField] private GameObject _upgradeRequirementsRoot;
        [SerializeField] private TextMeshProUGUI _costGoldText;
        [SerializeField] private TextMeshProUGUI _costOilText;
        [SerializeField] private TextMeshProUGUI _costIronText;
        [SerializeField] private TextMeshProUGUI _durationText;
        [SerializeField] private TextMeshProUGUI _prerequisiteHQText;

        [Header("Action Buttons")]
        [SerializeField] private Button _btnUpgrade;
        [SerializeField] private TextMeshProUGUI _btnUpgradeText;
        [SerializeField] private Button _btnCancelUpgrade;
        [SerializeField] private Button _btnClose;

        [Header("Upgrade Progress Display")]
        [SerializeField] private GameObject _upgradingStatusRoot;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private Image _progressBarFill;

        private IPlayerBaseService _baseService;
        private ILocalizationService _loc;
        private IModalService _modal;
        private BuildingId? _currentBuildingId;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;
        public BuildingId? CurrentBuildingId => _currentBuildingId;

        public TextMeshProUGUI BuildingNameText => _buildingNameText;
        public TextMeshProUGUI LevelBadgeText => _levelBadgeText;
        public TextMeshProUGUI DescriptionText => _descriptionText;
        public GameObject MockDataBadgeRoot => _mockDataBadgeRoot;
        public TextMeshProUGUI MockDataBadgeText => _mockDataBadgeText;
        public TextMeshProUGUI CurrentPerkText => _currentPerkText;
        public TextMeshProUGUI NextPerkText => _nextPerkText;
        public TextMeshProUGUI CostGoldText => _costGoldText;
        public TextMeshProUGUI CostOilText => _costOilText;
        public TextMeshProUGUI CostIronText => _costIronText;
        public TextMeshProUGUI DurationText => _durationText;
        public TextMeshProUGUI PrerequisiteHQText => _prerequisiteHQText;
        public Button BtnUpgrade => _btnUpgrade;
        public Button BtnCancelUpgrade => _btnCancelUpgrade;
        public Button BtnClose => _btnClose;
        public TextMeshProUGUI CountdownText => _countdownText;
        public GameObject UpgradingStatusRoot => _upgradingStatusRoot;

        public void SetupComponentsForTest(
            GameObject panelRoot,
            TextMeshProUGUI buildingNameText,
            TextMeshProUGUI levelBadgeText,
            TextMeshProUGUI descriptionText,
            TextMeshProUGUI currentPerkText,
            TextMeshProUGUI nextPerkText,
            GameObject upgradeRequirementsRoot,
            TextMeshProUGUI costGoldText,
            TextMeshProUGUI costOilText,
            TextMeshProUGUI costIronText,
            TextMeshProUGUI durationText,
            TextMeshProUGUI prerequisiteHQText,
            Button btnUpgrade,
            TextMeshProUGUI btnUpgradeText,
            Button btnCancelUpgrade,
            Button btnClose,
            GameObject upgradingStatusRoot,
            TextMeshProUGUI countdownText,
            Image progressBarFill,
            GameObject mockDataBadgeRoot = null,
            TextMeshProUGUI mockDataBadgeText = null)
        {
            _panelRoot = panelRoot;
            _buildingNameText = buildingNameText;
            _levelBadgeText = levelBadgeText;
            _descriptionText = descriptionText;
            _mockDataBadgeRoot = mockDataBadgeRoot;
            _mockDataBadgeText = mockDataBadgeText;
            _currentPerkText = currentPerkText;
            _nextPerkText = nextPerkText;
            _upgradeRequirementsRoot = upgradeRequirementsRoot;
            _costGoldText = costGoldText;
            _costOilText = costOilText;
            _costIronText = costIronText;
            _durationText = durationText;
            _prerequisiteHQText = prerequisiteHQText;
            _btnUpgrade = btnUpgrade;
            _btnUpgradeText = btnUpgradeText;
            _btnCancelUpgrade = btnCancelUpgrade;
            _btnClose = btnClose;
            _upgradingStatusRoot = upgradingStatusRoot;
            _countdownText = countdownText;
            _progressBarFill = progressBarFill;

            if (_btnUpgrade != null)
            {
                _btnUpgrade.onClick.RemoveAllListeners();
                _btnUpgrade.onClick.AddListener(OnUpgradeClicked);
            }
            if (_btnCancelUpgrade != null)
            {
                _btnCancelUpgrade.onClick.RemoveAllListeners();
                _btnCancelUpgrade.onClick.AddListener(OnCancelUpgradeClicked);
            }
            if (_btnClose != null)
            {
                _btnClose.onClick.RemoveAllListeners();
                _btnClose.onClick.AddListener(OnCloseClicked);
            }

            if (_panelRoot != null) _panelRoot.SetActive(false);
            if (_upgradingStatusRoot != null) _upgradingStatusRoot.SetActive(false);
        }

        private void Awake()
        {
            if (_btnUpgrade != null) _btnUpgrade.onClick.AddListener(OnUpgradeClicked);
            if (_btnCancelUpgrade != null) _btnCancelUpgrade.onClick.AddListener(OnCancelUpgradeClicked);
            if (_btnClose != null) _btnClose.onClick.AddListener(OnCloseClicked);

            if (_panelRoot != null) _panelRoot.SetActive(false);
            if (_upgradingStatusRoot != null) _upgradingStatusRoot.SetActive(false);
        }

        public void Bind(IPlayerBaseService baseService, ILocalizationService loc, IModalService modal = null)
        {
            if (_baseService != null)
            {
                _baseService.OnBuildingSelected -= HandleBuildingSelected;
                _baseService.OnUpgradeStarted -= HandleUpgradeStateChanged;
                _baseService.OnBuildingUpgraded -= HandleUpgradeStateChanged;
                _baseService.OnUpgradeCancelled -= HandleUpgradeStateChanged;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            _baseService = baseService;
            _loc = loc;
            _modal = modal;

            if (_baseService != null)
            {
                _baseService.OnBuildingSelected += HandleBuildingSelected;
                _baseService.OnUpgradeStarted += HandleUpgradeStateChanged;
                _baseService.OnBuildingUpgraded += HandleUpgradeStateChanged;
                _baseService.OnUpgradeCancelled += HandleUpgradeStateChanged;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }
        }

        private void Update()
        {
            if (!IsVisible || !_currentBuildingId.HasValue || _baseService == null) return;

            if (_baseService.StateRepository.TryGetBuilding(_currentBuildingId.Value, out var building))
            {
                if (building.IsUpgrading)
                {
                    UpdateCountdown(building);
                }
            }
        }

        private void UpdateCountdown(BuildingRuntimeState building)
        {
            var remaining = building.GetRemainingUpgradeTime(DateTime.UtcNow);
            if (_countdownText != null)
            {
                string format = _loc != null ? _loc.GetString("base.drawer.time_remaining") : "Time Remaining: {0}";
                _countdownText.text = string.Format(format, $"{remaining.Minutes:D2}:{remaining.Seconds:D2}");
            }

            if (_progressBarFill != null)
            {
                _progressBarFill.fillAmount = building.GetUpgradeProgress(DateTime.UtcNow);
            }
        }

        private void HandleBuildingSelected(BuildingId id)
        {
            if (!id.IsValid)
            {
                Hide();
                return;
            }

            _currentBuildingId = id;
            DisplayBuilding(id);
        }

        private void HandleUpgradeStateChanged(BuildingRuntimeState state)
        {
            if (_currentBuildingId.HasValue && _currentBuildingId.Value == state.Id)
            {
                DisplayBuilding(state.Id);
            }
        }

        private void HandleLanguageChanged(Language lang)
        {
            if (IsVisible && _currentBuildingId.HasValue)
            {
                DisplayBuilding(_currentBuildingId.Value);
            }
        }

        public void DisplayBuilding(BuildingId id)
        {
            _currentBuildingId = id;
            if (_baseService == null) return;

            if (!_baseService.StateRepository.TryGetBuilding(id, out var building))
            {
                Hide();
                return;
            }

            if (!_baseService.DataSource.TryGetDefinition(building.Type, out var def))
            {
                Hide();
                return;
            }

            if (_panelRoot != null) _panelRoot.SetActive(true);

            // 1. Header
            string nameKey = def.NameLocalizationKey;
            string descKey = def.DescriptionLocalizationKey;
            string nameText = _loc != null ? _loc.GetString(nameKey) : def.Type.ToString();
            string descText = _loc != null ? _loc.GetString(descKey) : string.Empty;

            string levelFormat = _loc != null ? _loc.GetString("base.drawer.level") : "Level {0}";
            string levelText = string.Format(levelFormat, building.CurrentLevel);

            if (_buildingNameText != null) _buildingNameText.text = nameText;
            if (_levelBadgeText != null) _levelBadgeText.text = levelText;
            if (_descriptionText != null) _descriptionText.text = descText;

            // Mock Data Badge (Editor / Development Build Only)
            if (_mockDataBadgeRoot != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _mockDataBadgeRoot.SetActive(true);
                if (_mockDataBadgeText != null)
                {
                    _mockDataBadgeText.text = _loc != null ? _loc.GetString("base.drawer.mock_badge") : "MOCK DATA (DEV ONLY)";
                }
#else
                _mockDataBadgeRoot.SetActive(false);
#endif
            }

            // 2. Stats
            if (def.TryGetLevelData(building.CurrentLevel, out var currentData))
            {
                string perkFormat = _loc != null ? _loc.GetString(currentData.OutputFormatKey) : "{0}";
                string currentPerk = string.Format(perkFormat, currentData.OutputValue);
                if (_currentPerkText != null) _currentPerkText.text = currentPerk;
            }

            bool isMaxLevel = building.CurrentLevel >= def.MaxLevel;
            bool isUpgrading = building.IsUpgrading;

            if (_upgradingStatusRoot != null)
            {
                _upgradingStatusRoot.SetActive(isUpgrading);
            }

            if (isMaxLevel)
            {
                if (_nextPerkText != null)
                {
                    _nextPerkText.text = _loc != null ? _loc.GetString("base.drawer.max_level") : "MAX LEVEL";
                }

                if (_upgradeRequirementsRoot != null) _upgradeRequirementsRoot.SetActive(false);
                if (_btnUpgrade != null) _btnUpgrade.gameObject.SetActive(false);
                if (_btnCancelUpgrade != null) _btnCancelUpgrade.gameObject.SetActive(false);
                return;
            }

            int targetLevel = building.CurrentLevel + 1;
            if (def.TryGetLevelData(targetLevel, out var nextData))
            {
                string nextPerkFormat = _loc != null ? _loc.GetString(nextData.OutputFormatKey) : "{0}";
                string nextPerk = string.Format(nextPerkFormat, nextData.OutputValue);
                if (_nextPerkText != null) _nextPerkText.text = nextPerk;

                if (_upgradeRequirementsRoot != null) _upgradeRequirementsRoot.SetActive(!isUpgrading);

                if (!isUpgrading)
                {
                    if (_costGoldText != null) _costGoldText.text = $"{nextData.Cost.Gold:N0}";
                    if (_costOilText != null) _costOilText.text = $"{nextData.Cost.Oil:N0}";
                    if (_costIronText != null) _costIronText.text = $"{nextData.Cost.Iron:N0}";
                    if (_durationText != null)
                    {
                        var ts = TimeSpan.FromSeconds(nextData.UpgradeDurationSeconds);
                        _durationText.text = $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                    }

                    // Check HQ prerequisite
                    if (building.Type != BuildingType.CommandCenter)
                    {
                        var hq = _baseService.StateRepository.GetCommandCenter();
                        int hqLevel = hq?.CurrentLevel ?? 1;
                        bool met = hqLevel >= nextData.RequiredHQLevel;

                        if (_prerequisiteHQText != null)
                        {
                            if (met)
                            {
                                _prerequisiteHQText.text = _loc != null ? _loc.GetString("base.drawer.req_hq_met") : "HQ Requirement Satisfied";
                                _prerequisiteHQText.color = new Color(0.2f, 0.8f, 0.3f);
                            }
                            else
                            {
                                string hqReqFormat = _loc != null ? _loc.GetString("base.drawer.req_hq") : "Requires HQ Level {0}";
                                _prerequisiteHQText.text = string.Format(hqReqFormat, nextData.RequiredHQLevel);
                                _prerequisiteHQText.color = new Color(0.9f, 0.2f, 0.2f);
                            }
                        }

                        if (_btnUpgrade != null)
                        {
                            _btnUpgrade.interactable = met;
                        }
                    }
                    else
                    {
                        if (_prerequisiteHQText != null) _prerequisiteHQText.text = string.Empty;
                        if (_btnUpgrade != null) _btnUpgrade.interactable = true;
                    }
                }
            }

            if (_btnUpgrade != null)
            {
                _btnUpgrade.gameObject.SetActive(!isUpgrading);
            }

            if (_btnCancelUpgrade != null)
            {
                _btnCancelUpgrade.gameObject.SetActive(isUpgrading);
            }

            if (isUpgrading)
            {
                UpdateCountdown(building);
            }
        }

        private async void OnUpgradeClicked()
        {
            if (!_currentBuildingId.HasValue || _baseService == null) return;
            await _baseService.RequestUpgradeAsync(_currentBuildingId.Value);
        }

        private void OnCancelUpgradeClicked()
        {
            if (!_currentBuildingId.HasValue || _baseService == null) return;

            if (_modal != null)
            {
                string title = _loc != null ? _loc.GetString("base.cancel_dialog.title") : "Cancel Construction";
                string msg = _loc != null ? _loc.GetString("base.cancel_dialog.msg") : "Are you sure you want to abort the upgrade in progress?";
                string confirm = _loc != null ? _loc.GetString("common.confirm") : "Confirm";
                string cancel = _loc != null ? _loc.GetString("common.cancel") : "Cancel";

                _modal.ShowConfirm(title, msg, confirm, cancel, () =>
                {
                    if (_currentBuildingId.HasValue)
                    {
                        _ = _baseService.CancelUpgradeAsync(_currentBuildingId.Value);
                    }
                });
            }
            else
            {
                _ = _baseService.CancelUpgradeAsync(_currentBuildingId.Value);
            }
        }

        private void OnCloseClicked()
        {
            _baseService?.Deselect();
            Hide();
        }

        public void Hide()
        {
            _currentBuildingId = null;
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_baseService != null)
            {
                _baseService.OnBuildingSelected -= HandleBuildingSelected;
                _baseService.OnUpgradeStarted -= HandleUpgradeStateChanged;
                _baseService.OnBuildingUpgraded -= HandleUpgradeStateChanged;
                _baseService.OnUpgradeCancelled -= HandleUpgradeStateChanged;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }
        }
    }
}
