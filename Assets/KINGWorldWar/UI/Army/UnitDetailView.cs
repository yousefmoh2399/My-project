using System;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Army
{
    /// <summary>
    /// Detailed inspection screen for military unit archetypes.
    /// Features stat breakdown categorized by authority (DisplayOnly vs ServerAuthoritative),
    /// technology tier / upgrade level benefits, 3D rotating model preview stage,
    /// and mobilization / research action dispatchers.
    /// </summary>
    public class UnitDetailView : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Header Info")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private TextMeshProUGUI _roleText;
        [SerializeField] private TextMeshProUGUI _tierLevelText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("Combat Stats & Authority")]
        [SerializeField] private TextMeshProUGUI _hpStatText;
        [SerializeField] private TextMeshProUGUI _attackStatText;
        [SerializeField] private TextMeshProUGUI _defenseStatText;
        [SerializeField] private TextMeshProUGUI _speedStatText;
        [SerializeField] private TextMeshProUGUI _rangeStatText;
        [SerializeField] private TextMeshProUGUI _capacityStatText;
        [SerializeField] private TextMeshProUGUI _powerStatText;

        [Header("Next Level Upgrade")]
        [SerializeField] private GameObject _upgradeSectionRoot;
        [SerializeField] private TextMeshProUGUI _nextLevelPerksText;
        [SerializeField] private TextMeshProUGUI _upgradeCostText;
        [SerializeField] private TextMeshProUGUI _upgradeDurationText;
        [SerializeField] private Button _btnUpgradeTech;
        [SerializeField] private TextMeshProUGUI _btnUpgradeTechText;

        [Header("Action Buttons")]
        [SerializeField] private Button _btnTrainUnits;
        [SerializeField] private TextMeshProUGUI _btnTrainUnitsText;
        [SerializeField] private Button _btnClose;

        [Header("3D Rotating Stage")]
        [SerializeField] private Transform _previewStageAnchor;
        [SerializeField] private float _rotationSpeed = 25f;

        private IArmyService _armyService;
        private ILocalizationService _loc;
        private UnitDefinition _currentDefinition;
        private GameObject _spawnedModelInstance;
        private Action<UnitType> _onTrainRequested;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;
        public UnitDefinition CurrentDefinition => _currentDefinition;
        public TextMeshProUGUI UnitNameText => _unitNameText;
        public TextMeshProUGUI CategoryText => _categoryText;
        public TextMeshProUGUI RoleText => _roleText;
        public TextMeshProUGUI TierLevelText => _tierLevelText;
        public TextMeshProUGUI HpStatText => _hpStatText;
        public TextMeshProUGUI PowerStatText => _powerStatText;
        public TextMeshProUGUI NextLevelPerksText => _nextLevelPerksText;
        public Button BtnTrainUnits => _btnTrainUnits;
        public Button BtnUpgradeTech => _btnUpgradeTech;
        public Button BtnClose => _btnClose;
        public Transform PreviewStageAnchor => _previewStageAnchor;

        private void Awake()
        {
            if (_btnClose != null)
            {
                _btnClose.onClick.RemoveAllListeners();
                _btnClose.onClick.AddListener(Close);
            }

            if (_btnUpgradeTech != null)
            {
                _btnUpgradeTech.onClick.RemoveAllListeners();
                _btnUpgradeTech.onClick.AddListener(OnUpgradeClicked);
            }

            if (_btnTrainUnits != null)
            {
                _btnTrainUnits.onClick.RemoveAllListeners();
                _btnTrainUnits.onClick.AddListener(OnTrainClicked);
            }
        }

        private void Update()
        {
            if (_previewStageAnchor != null && IsVisible)
            {
                _previewStageAnchor.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.Self);
            }
        }

        public void Initialize(
            IArmyService armyService,
            ILocalizationService loc,
            Action<UnitType> onTrainRequested = null)
        {
            _armyService = armyService;
            _loc = loc;
            _onTrainRequested = onTrainRequested;

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }
        }

        private void OnDestroy()
        {
            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            ClearModelInstance();
        }

        public void Open(UnitDefinition definition)
        {
            if (definition == null) return;
            _currentDefinition = definition;

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            Refresh();
            MountModelPreview(definition);
        }

        public void Close()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }

            ClearModelInstance();
        }

        public void Refresh()
        {
            if (_currentDefinition == null) return;

            if (_titleText != null)
                _titleText.text = _loc != null ? _loc.GetString("army.detail.title") : "Unit Specifications";

            if (_unitNameText != null)
                _unitNameText.text = _loc != null ? _loc.GetString(_currentDefinition.NameKey) : _currentDefinition.Type.ToString();

            if (_roleText != null)
                _roleText.text = _loc != null ? _loc.GetString(_currentDefinition.RoleKey) : string.Empty;

            if (_descriptionText != null)
                _descriptionText.text = _loc != null ? _loc.GetString(_currentDefinition.DescKey) : string.Empty;

            if (_categoryText != null)
            {
                string catKey = _currentDefinition.Category == UnitCategory.Ground ? "army.category.ground" : "army.category.air";
                string catName = _loc != null ? _loc.GetString(catKey) : _currentDefinition.Category.ToString();
                string format = _loc != null ? _loc.GetString("army.detail.category") : "Category: {0}";
                _categoryText.text = string.Format(format, catName);
            }

            if (_tierLevelText != null)
            {
                string tierFormat = _loc != null ? _loc.GetString("army.detail.tier") : "Tier {0}";
                string levelFormat = _loc != null ? _loc.GetString("army.detail.level") : "Level {0}";
                _tierLevelText.text = $"{string.Format(tierFormat, _currentDefinition.Tier)} • {string.Format(levelFormat, 1)}";
            }

            // Stats with ServerAuthoritative indicator
            var stats = _currentDefinition.Stats;
            if (_hpStatText != null) _hpStatText.text = $"HP: {stats.Health:N0}";
            if (_attackStatText != null) _attackStatText.text = $"ATK: {stats.Attack:N0}";
            if (_defenseStatText != null) _defenseStatText.text = $"DEF: {stats.Defense:N0}";
            if (_speedStatText != null) _speedStatText.text = $"SPD: {stats.Speed:F1}";
            if (_rangeStatText != null) _rangeStatText.text = $"RNG: {stats.Range:F0}m";
            if (_capacityStatText != null) _capacityStatText.text = $"CAP: {stats.CapacityWeight}";
            if (_powerStatText != null) _powerStatText.text = $"PWR: {stats.CombatPower:N0}";

            // Upgrade perks
            if (_nextLevelPerksText != null)
            {
                _nextLevelPerksText.text = _loc != null && !string.IsNullOrEmpty(_currentDefinition.NextLevelBenefitsKey)
                    ? _loc.GetString(_currentDefinition.NextLevelBenefitsKey)
                    : "+15% Combat Efficiency";
            }

            if (_upgradeCostText != null)
            {
                var upCost = _currentDefinition.NextLevelUpgradeCost;
                _upgradeCostText.text = $"G: {upCost.Gold} | O: {upCost.Oil} | I: {upCost.Iron}";
            }

            if (_upgradeDurationText != null)
            {
                int duration = _currentDefinition.NextLevelUpgradeDurationSeconds;
                string durFormat = _loc != null ? _loc.GetString("army.detail.upgrade_duration") : "Research: {0}";
                _upgradeDurationText.text = string.Format(durFormat, $"{duration / 60:D2}:{duration % 60:D2}");
            }

            if (_btnTrainUnitsText != null)
                _btnTrainUnitsText.text = _loc != null ? _loc.GetString("army.detail.action_train") : "Mobilize Units";

            if (_btnUpgradeTechText != null)
                _btnUpgradeTechText.text = _loc != null ? _loc.GetString("army.detail.action_upgrade") : "Upgrade Tech";
        }

        private void MountModelPreview(UnitDefinition def)
        {
            ClearModelInstance();
            if (_previewStageAnchor == null || string.IsNullOrEmpty(def.ModelAssetPath)) return;

            // Load model prefab from Resources or Resources cache
            var prefab = Resources.Load<GameObject>(def.ModelAssetPath.Replace("Assets/KINGWorldWar/Art/Prefabs/Units/", "Units/").Replace(".prefab", ""));
            if (prefab != null)
            {
                _spawnedModelInstance = Instantiate(prefab, _previewStageAnchor);
                _spawnedModelInstance.transform.localPosition = Vector3.zero;
                _spawnedModelInstance.transform.localRotation = Quaternion.identity;
                _spawnedModelInstance.transform.localScale = Vector3.one;
            }
        }

        private void ClearModelInstance()
        {
            if (_spawnedModelInstance != null)
            {
                Destroy(_spawnedModelInstance);
                _spawnedModelInstance = null;
            }
        }

        private void OnUpgradeClicked()
        {
            if (_armyService == null || _currentDefinition == null) return;
            _ = _armyService.RequestUnitUpgradeAsync(_currentDefinition.Type);
        }

        private void OnTrainClicked()
        {
            if (_currentDefinition == null) return;
            _onTrainRequested?.Invoke(_currentDefinition.Type);
        }

        private void HandleLanguageChanged(Language lang) => Refresh();

        public void SetupForTest(
            GameObject panelRoot,
            TextMeshProUGUI unitNameText,
            TextMeshProUGUI categoryText,
            TextMeshProUGUI roleText,
            TextMeshProUGUI hpStatText,
            TextMeshProUGUI powerStatText,
            TextMeshProUGUI nextLevelPerksText,
            Button btnTrain,
            Button btnUpgrade,
            Button btnClose,
            Transform previewStageAnchor = null)
        {
            _panelRoot = panelRoot;
            _unitNameText = unitNameText;
            _categoryText = categoryText;
            _roleText = roleText;
            _hpStatText = hpStatText;
            _powerStatText = powerStatText;
            _nextLevelPerksText = nextLevelPerksText;
            _btnTrainUnits = btnTrain;
            _btnUpgradeTech = btnUpgrade;
            _btnClose = btnClose;
            _previewStageAnchor = previewStageAnchor;

            Awake();
        }
    }
}
