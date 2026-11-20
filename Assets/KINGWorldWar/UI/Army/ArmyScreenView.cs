using System;
using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Army
{
    /// <summary>
    /// Main tactical army management screen (replaces placeholder for ScreenId.Army).
    /// Displays commander combat rating, total mobilized troop strength, active training queues,
    /// categorized unit grid (Ground and Air), status breakdown (Available, InTraining, Deployed),
    /// and archetype inspection / mobilization cards.
    /// </summary>
    public class ArmyScreenView : MonoBehaviour
    {
        [Header("Header Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _combatPowerLabel;
        [SerializeField] private TextMeshProUGUI _combatPowerValue;
        [SerializeField] private TextMeshProUGUI _totalTroopsLabel;
        [SerializeField] private TextMeshProUGUI _totalTroopsValue;
        [SerializeField] private TextMeshProUGUI _activeQueuesLabel;
        [SerializeField] private TextMeshProUGUI _activeQueuesValue;

        [Header("Category Filter Tabs")]
        [SerializeField] private Button _tabAllUnits;
        [SerializeField] private Button _tabGroundUnits;
        [SerializeField] private Button _tabAirUnits;

        [Header("Drawer & Detail References")]
        [SerializeField] private RecruitmentDrawer _recruitmentDrawer;
        [SerializeField] private UnitDetailView _unitDetailView;

        [Header("Ground Units Cards")]
        [SerializeField] private UnitCardUI _tankCard;
        [SerializeField] private UnitCardUI _armoredVehicleCard;
        [SerializeField] private UnitCardUI _artilleryCard;

        [Header("Air Units Cards")]
        [SerializeField] private UnitCardUI _fighterAircraftCard;
        [SerializeField] private UnitCardUI _reconAircraftCard;

        [Header("Legacy Compatibility Cards")]
        [SerializeField] private UnitCardUI _infantryCard;
        [SerializeField] private UnitCardUI _reconCard;

        private IArmyService _armyService;
        private ILocalizationService _loc;
        private UnitCategory? _activeCategoryFilter = null; // null = All

        public TextMeshProUGUI TitleText => _titleText;
        public TextMeshProUGUI CombatPowerValue => _combatPowerValue;
        public TextMeshProUGUI TotalTroopsValue => _totalTroopsValue;
        public TextMeshProUGUI ActiveQueuesValue => _activeQueuesValue;
        public RecruitmentDrawer RecruitmentDrawer => _recruitmentDrawer;
        public UnitDetailView UnitDetailView => _unitDetailView;

        public UnitCardUI TankCard => _tankCard;
        public UnitCardUI ArmoredVehicleCard => _armoredVehicleCard;
        public UnitCardUI ArtilleryCard => _artilleryCard;
        public UnitCardUI FighterAircraftCard => _fighterAircraftCard;
        public UnitCardUI ReconAircraftCard => _reconAircraftCard;

        public UnitCardUI InfantryCard => _infantryCard;
        public UnitCardUI ReconCard => _reconCard;

        public UnitCategory? ActiveCategoryFilter => _activeCategoryFilter;

        [Serializable]
        public class UnitCardUI
        {
            public GameObject Root;
            public UnitType Type;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI RoleText;
            public TextMeshProUGUI CountText;
            public TextMeshProUGUI PowerText;
            public TextMeshProUGUI PrereqText;
            public TextMeshProUGUI StatusBreakdownText;
            public Button RecruitButton;
            public TextMeshProUGUI RecruitButtonText;
            public Button InspectButton;
            public GameObject TrainingBadge;
            public TextMeshProUGUI TrainingCountdown;
        }

        public void Initialize(
            IArmyService armyService,
            ILocalizationService loc,
            RecruitmentDrawer drawer,
            UnitDetailView detailView = null)
        {
            if (_armyService != null)
            {
                _armyService.OnRosterUpdated -= HandleRosterUpdated;
                _armyService.OnRecruitmentStarted -= HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled -= HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted -= HandleRecruitmentCompleted;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            _armyService = armyService;
            _loc = loc;
            _recruitmentDrawer = drawer;
            _unitDetailView = detailView;

            BindCardButtons();
            BindCategoryTabs();

            if (_armyService != null)
            {
                _armyService.OnRosterUpdated += HandleRosterUpdated;
                _armyService.OnRecruitmentStarted += HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled += HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted += HandleRecruitmentCompleted;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (_armyService != null)
            {
                _armyService.OnRosterUpdated -= HandleRosterUpdated;
                _armyService.OnRecruitmentStarted -= HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled -= HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted -= HandleRecruitmentCompleted;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        private void BindCategoryTabs()
        {
            if (_tabAllUnits != null)
            {
                _tabAllUnits.onClick.RemoveAllListeners();
                _tabAllUnits.onClick.AddListener(() => FilterCategory(null));
            }

            if (_tabGroundUnits != null)
            {
                _tabGroundUnits.onClick.RemoveAllListeners();
                _tabGroundUnits.onClick.AddListener(() => FilterCategory(UnitCategory.Ground));
            }

            if (_tabAirUnits != null)
            {
                _tabAirUnits.onClick.RemoveAllListeners();
                _tabAirUnits.onClick.AddListener(() => FilterCategory(UnitCategory.Air));
            }
        }

        public void FilterCategory(UnitCategory? category)
        {
            _activeCategoryFilter = category;
            Refresh();
        }

        private void BindCardButtons()
        {
            // MVP 5 archetypes
            BindCard(_tankCard, UnitType.Tank);
            BindCard(_armoredVehicleCard, UnitType.ArmoredVehicle);
            BindCard(_artilleryCard, UnitType.Artillery);
            BindCard(_fighterAircraftCard, UnitType.FighterAircraft);
            BindCard(_reconAircraftCard, UnitType.ReconAircraft);

            // Legacy backward compatibility cards
            BindCard(_infantryCard, UnitType.Infantry);
            BindCard(_reconCard, UnitType.ReconVehicle);
        }

        private void BindCard(UnitCardUI card, UnitType type)
        {
            if (card == null) return;
            card.Type = type;

            if (card.RecruitButton != null)
            {
                card.RecruitButton.onClick.RemoveAllListeners();
                card.RecruitButton.onClick.AddListener(() =>
                {
                    _recruitmentDrawer?.OpenForUnit(type);
                });
            }

            if (card.InspectButton != null)
            {
                card.InspectButton.onClick.RemoveAllListeners();
                card.InspectButton.onClick.AddListener(() =>
                {
                    if (_armyService != null && _armyService.DataSource.TryGetDefinition(type, out var def))
                    {
                        _unitDetailView?.Open(def);
                    }
                });
            }
        }

        public void Refresh()
        {
            if (_armyService == null) return;

            var roster = _armyService.GetRoster();
            var ds = _armyService.DataSource;
            var queues = _armyService.GetActiveQueues();
            int totalPower = _armyService.GetTotalCombatPower();
            var snapshot = _armyService.CurrentSnapshot;

            // Header readouts
            if (_titleText != null)
            {
                _titleText.text = _loc != null ? _loc.GetString("army.screen.title") : "Military Command";
            }

            if (_combatPowerLabel != null)
            {
                _combatPowerLabel.text = _loc != null ? _loc.GetString("army.screen.total_power") : "Total Combat Power";
            }

            if (_combatPowerValue != null)
            {
                _combatPowerValue.text = $"{totalPower:N0}";
            }

            if (_totalTroopsLabel != null)
            {
                _totalTroopsLabel.text = _loc != null ? _loc.GetString("army.screen.total_troops") : "Total Forces";
            }

            if (_totalTroopsValue != null)
            {
                int totalCount = snapshot != null ? snapshot.TotalForcesCount : roster.TotalTroops;
                _totalTroopsValue.text = $"{totalCount:N0}";
            }

            if (_activeQueuesLabel != null)
            {
                _activeQueuesLabel.text = _loc != null ? _loc.GetString("army.screen.active_queues") : "Operations";
            }

            if (_activeQueuesValue != null)
            {
                int maxQ = _armyService.MaxConcurrentQueues;
                _activeQueuesValue.text = $"{queues.Count} / {maxQ}";
            }

            // Cards refresh
            RefreshCard(_tankCard, UnitType.Tank, roster, ds, queues, snapshot);
            RefreshCard(_armoredVehicleCard, UnitType.ArmoredVehicle, roster, ds, queues, snapshot);
            RefreshCard(_artilleryCard, UnitType.Artillery, roster, ds, queues, snapshot);
            RefreshCard(_fighterAircraftCard, UnitType.FighterAircraft, roster, ds, queues, snapshot);
            RefreshCard(_reconAircraftCard, UnitType.ReconAircraft, roster, ds, queues, snapshot);

            // Legacy
            RefreshCard(_infantryCard, UnitType.Infantry, roster, ds, queues, snapshot);
            RefreshCard(_reconCard, UnitType.ReconVehicle, roster, ds, queues, snapshot);
        }

        private void RefreshCard(
            UnitCardUI card,
            UnitType type,
            ArmyRoster roster,
            IArmyDataSource ds,
            IReadOnlyList<RecruitmentQueueItem> queues,
            ArmyRosterSnapshot snapshot)
        {
            if (card == null || !ds.TryGetDefinition(type, out var def)) return;

            // Category filter visibility
            if (_activeCategoryFilter != null && def.Category != _activeCategoryFilter)
            {
                if (card.Root != null) card.Root.SetActive(false);
                return;
            }
            if (card.Root != null) card.Root.SetActive(true);

            if (card.NameText != null)
            {
                card.NameText.text = _loc != null ? _loc.GetString(def.NameKey) : type.ToString();
            }

            if (card.RoleText != null)
            {
                card.RoleText.text = _loc != null ? _loc.GetString(def.RoleKey) : string.Empty;
            }

            int count = snapshot != null ? snapshot.GetTotalOwnedCount(type) : roster.GetCount(type);
            int available = snapshot != null ? snapshot.GetAvailableCount(type) : count;
            int inTraining = snapshot != null ? snapshot.GetInTrainingCount(type) : 0;
            int deployed = snapshot != null ? snapshot.GetDeployedCount(type) : 0;

            if (card.CountText != null)
            {
                string countFormat = _loc != null ? _loc.GetString("army.screen.roster_count") : "Mobilized: {0}";
                card.CountText.text = string.Format(countFormat, count);
            }

            if (card.StatusBreakdownText != null)
            {
                string availStr = _loc?.GetString("army.status.available") ?? "Avail";
                string trainStr = _loc?.GetString("army.status.in_training") ?? "Train";
                string depStr = _loc?.GetString("army.status.deployed") ?? "Dep";
                card.StatusBreakdownText.text = $"{availStr}: {available} | {trainStr}: {inTraining} | {depStr}: {deployed}";
            }

            int archetypePower = def.Stats.CombatPower * count;
            if (card.PowerText != null)
            {
                card.PowerText.text = $"PWR: {archetypePower:N0}";
            }

            bool prereqMet = _armyService.CheckPrerequisites(type, out string prereqReason);
            if (card.PrereqText != null)
            {
                if (prereqMet)
                {
                    card.PrereqText.text = $"✔ {_loc?.GetString("army.drawer.req_met") ?? "Ready"}";
                    card.PrereqText.color = new Color(0.2f, 0.8f, 0.4f, 1f);
                }
                else
                {
                    card.PrereqText.text = $"✖ {prereqReason}";
                    card.PrereqText.color = new Color(0.9f, 0.3f, 0.3f, 1f);
                }
            }

            // Check if currently training
            RecruitmentQueueItem trainingItem = null;
            for (var i = 0; i < queues.Count; i++)
            {
                if (queues[i].Type == type)
                {
                    trainingItem = queues[i];
                    break;
                }
            }

            bool isTraining = trainingItem != null && !trainingItem.IsCompleted(DateTime.UtcNow);
            if (card.TrainingBadge != null)
            {
                card.TrainingBadge.SetActive(isTraining);
            }

            if (isTraining && card.TrainingCountdown != null)
            {
                var remaining = (int)trainingItem.RemainingSeconds(DateTime.UtcNow);
                card.TrainingCountdown.text = $"{remaining / 60:D2}:{remaining % 60:D2}";
            }

            if (card.RecruitButtonText != null)
            {
                card.RecruitButtonText.text = _loc != null ? _loc.GetString("army.screen.btn_recruit") : "Recruit";
            }

            if (card.RecruitButton != null)
            {
                card.RecruitButton.interactable = prereqMet;
            }
        }

        private void Update()
        {
            if (_armyService == null) return;

            var queues = _armyService.GetActiveQueues();
            bool hasCompleted = false;
            for (var i = 0; i < queues.Count; i++)
            {
                if (queues[i].IsCompleted(DateTime.UtcNow))
                {
                    hasCompleted = true;
                    break;
                }
            }

            if (hasCompleted)
            {
                _armyService.CheckCompletedRecruitments(DateTime.UtcNow);
                Refresh();
            }
        }

        private void HandleRosterUpdated() => Refresh();
        private void HandleRecruitmentStarted(RecruitmentQueueItem item) => Refresh();
        private void HandleRecruitmentCancelled(string queueId) => Refresh();
        private void HandleRecruitmentCompleted(UnitType type, int count) => Refresh();
        private void HandleLanguageChanged(Language language) => Refresh();

        public void SetupComponentsForTest(
            TextMeshProUGUI titleText,
            TextMeshProUGUI combatPowerValue,
            TextMeshProUGUI totalTroopsValue,
            TextMeshProUGUI activeQueuesValue,
            RecruitmentDrawer drawer,
            UnitCardUI infantryCard,
            UnitCardUI reconCard,
            UnitCardUI tankCard,
            UnitCardUI artilleryCard,
            UnitCardUI armoredVehicleCard = null,
            UnitCardUI fighterCard = null,
            UnitCardUI reconAirCard = null,
            UnitDetailView detailView = null)
        {
            _titleText = titleText;
            _combatPowerValue = combatPowerValue;
            _totalTroopsValue = totalTroopsValue;
            _activeQueuesValue = activeQueuesValue;
            _recruitmentDrawer = drawer;
            _infantryCard = infantryCard;
            _reconCard = reconCard;
            _tankCard = tankCard;
            _artilleryCard = artilleryCard;
            _armoredVehicleCard = armoredVehicleCard;
            _fighterAircraftCard = fighterCard;
            _reconAircraftCard = reconAirCard;
            _unitDetailView = detailView;

            BindCardButtons();
        }
    }
}
