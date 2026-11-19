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
    /// and archetype mobilization cards.
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

        [Header("Drawer Reference")]
        [SerializeField] private RecruitmentDrawer _recruitmentDrawer;

        [Header("Unit Roster Cards")]
        [SerializeField] private UnitCardUI _infantryCard;
        [SerializeField] private UnitCardUI _reconCard;
        [SerializeField] private UnitCardUI _tankCard;
        [SerializeField] private UnitCardUI _artilleryCard;

        private IArmyService _armyService;
        private ILocalizationService _loc;

        public TextMeshProUGUI TitleText => _titleText;
        public TextMeshProUGUI CombatPowerValue => _combatPowerValue;
        public TextMeshProUGUI TotalTroopsValue => _totalTroopsValue;
        public TextMeshProUGUI ActiveQueuesValue => _activeQueuesValue;
        public RecruitmentDrawer RecruitmentDrawer => _recruitmentDrawer;
        public UnitCardUI InfantryCard => _infantryCard;
        public UnitCardUI ReconCard => _reconCard;
        public UnitCardUI TankCard => _tankCard;
        public UnitCardUI ArtilleryCard => _artilleryCard;

        [Serializable]
        public class UnitCardUI
        {
            public UnitType Type;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI RoleText;
            public TextMeshProUGUI CountText;
            public TextMeshProUGUI PowerText;
            public TextMeshProUGUI PrereqText;
            public Button RecruitButton;
            public TextMeshProUGUI RecruitButtonText;
            public GameObject TrainingBadge;
            public TextMeshProUGUI TrainingCountdown;
        }

        public void Initialize(IArmyService armyService, ILocalizationService loc, RecruitmentDrawer drawer)
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

            BindCardButtons();

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

        private void BindCardButtons()
        {
            BindCard(_infantryCard, UnitType.Infantry);
            BindCard(_reconCard, UnitType.ReconVehicle);
            BindCard(_tankCard, UnitType.Tank);
            BindCard(_artilleryCard, UnitType.Artillery);
        }

        private void BindCard(UnitCardUI card, UnitType type)
        {
            if (card != null && card.RecruitButton != null)
            {
                card.Type = type;
                card.RecruitButton.onClick.RemoveAllListeners();
                card.RecruitButton.onClick.AddListener(() =>
                {
                    _recruitmentDrawer?.OpenForUnit(type);
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
                _totalTroopsValue.text = $"{roster.TotalTroops:N0}";
            }

            if (_activeQueuesLabel != null)
            {
                _activeQueuesLabel.text = _loc != null ? _loc.GetString("army.screen.active_queues") : "Operations";
            }

            if (_activeQueuesValue != null)
            {
                _activeQueuesValue.text = $"{queues.Count} / {_armyService.StateRepository.MaxConcurrentQueues}";
            }

            // Cards refresh
            RefreshCard(_infantryCard, UnitType.Infantry, roster, ds, queues);
            RefreshCard(_reconCard, UnitType.ReconVehicle, roster, ds, queues);
            RefreshCard(_tankCard, UnitType.Tank, roster, ds, queues);
            RefreshCard(_artilleryCard, UnitType.Artillery, roster, ds, queues);
        }

        private void RefreshCard(
            UnitCardUI card,
            UnitType type,
            ArmyRoster roster,
            IArmyDataSource ds,
            IReadOnlyList<RecruitmentQueueItem> queues)
        {
            if (card == null || !ds.TryGetDefinition(type, out var def)) return;

            if (card.NameText != null)
            {
                card.NameText.text = _loc != null ? _loc.GetString(def.NameKey) : type.ToString();
            }

            if (card.RoleText != null)
            {
                card.RoleText.text = _loc != null ? _loc.GetString(def.RoleKey) : string.Empty;
            }

            int count = roster.GetCount(type);
            if (card.CountText != null)
            {
                string countFormat = _loc != null ? _loc.GetString("army.screen.roster_count") : "Mobilized: {0}";
                card.CountText.text = string.Format(countFormat, count);
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
            UnitCardUI artilleryCard)
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

            BindCardButtons();
        }
    }
}
