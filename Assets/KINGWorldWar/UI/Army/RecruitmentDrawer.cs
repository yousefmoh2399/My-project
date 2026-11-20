using System;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Army
{
    /// <summary>
    /// Tactical slide-in drawer for military unit recruitment, stat inspection,
    /// and active mobilization queue countdown.
    /// </summary>
    public class RecruitmentDrawer : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Header Texts")]
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _roleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("Combat Stats")]
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _rangeText;
        [SerializeField] private TextMeshProUGUI _powerText;

        [Header("Quantity Stepper")]
        [SerializeField] private Button _btnDecrement;
        [SerializeField] private Button _btnIncrement;
        [SerializeField] private Button _btnMax;
        [SerializeField] private TextMeshProUGUI _quantityText;

        [Header("Resource Costs & Time")]
        [SerializeField] private GameObject _costsRoot;
        [SerializeField] private TextMeshProUGUI _costGoldText;
        [SerializeField] private TextMeshProUGUI _costOilText;
        [SerializeField] private TextMeshProUGUI _costIronText;
        [SerializeField] private TextMeshProUGUI _durationText;
        [SerializeField] private TextMeshProUGUI _prerequisiteText;

        [Header("Action Buttons")]
        [SerializeField] private Button _btnRecruit;
        [SerializeField] private TextMeshProUGUI _btnRecruitText;
        [SerializeField] private Button _btnCancelRecruit;
        [SerializeField] private Button _btnClose;

        [Header("Training Progress Display")]
        [SerializeField] private GameObject _trainingStatusRoot;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private Image _progressBarFill;

        private IArmyService _armyService;
        private ILocalizationService _loc;
        private IModalService _modal;
        private UnitType? _currentType;
        private int _currentQuantity = 1;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;
        public UnitType? CurrentType => _currentType;
        public int CurrentQuantity => _currentQuantity;

        public TextMeshProUGUI UnitNameText => _unitNameText;
        public TextMeshProUGUI RoleText => _roleText;
        public TextMeshProUGUI DescriptionText => _descriptionText;
        public TextMeshProUGUI HpText => _hpText;
        public TextMeshProUGUI AttackText => _attackText;
        public TextMeshProUGUI DefenseText => _defenseText;
        public TextMeshProUGUI SpeedText => _speedText;
        public TextMeshProUGUI RangeText => _rangeText;
        public TextMeshProUGUI PowerText => _powerText;
        public TextMeshProUGUI QuantityText => _quantityText;
        public TextMeshProUGUI CostGoldText => _costGoldText;
        public TextMeshProUGUI CostOilText => _costOilText;
        public TextMeshProUGUI CostIronText => _costIronText;
        public TextMeshProUGUI DurationText => _durationText;
        public TextMeshProUGUI PrerequisiteText => _prerequisiteText;
        public Button BtnRecruit => _btnRecruit;
        public Button BtnCancelRecruit => _btnCancelRecruit;
        public Button BtnClose => _btnClose;
        public Button BtnDecrement => _btnDecrement;
        public Button BtnIncrement => _btnIncrement;
        public Button BtnMax => _btnMax;
        public TextMeshProUGUI CountdownText => _countdownText;
        public GameObject TrainingStatusRoot => _trainingStatusRoot;

        private void Awake()
        {
            BindButtonListeners();
        }

        private void BindButtonListeners()
        {
            if (_btnClose != null)
            {
                _btnClose.onClick.RemoveListener(Close);
                _btnClose.onClick.AddListener(Close);
            }

            if (_btnRecruit != null)
            {
                _btnRecruit.onClick.RemoveListener(HandleRecruitClicked);
                _btnRecruit.onClick.AddListener(HandleRecruitClicked);
            }

            if (_btnCancelRecruit != null)
            {
                _btnCancelRecruit.onClick.RemoveListener(HandleCancelClicked);
                _btnCancelRecruit.onClick.AddListener(HandleCancelClicked);
            }

            if (_btnDecrement != null)
            {
                _btnDecrement.onClick.RemoveListener(HandleDecrement);
                _btnDecrement.onClick.AddListener(HandleDecrement);
            }

            if (_btnIncrement != null)
            {
                _btnIncrement.onClick.RemoveListener(HandleIncrement);
                _btnIncrement.onClick.AddListener(HandleIncrement);
            }

            if (_btnMax != null)
            {
                _btnMax.onClick.RemoveListener(HandleMax);
                _btnMax.onClick.AddListener(HandleMax);
            }
        }

        public void Initialize(IArmyService armyService, ILocalizationService loc, IModalService modal)
        {
            if (_armyService != null)
            {
                _armyService.OnRecruitmentStarted -= HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled -= HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted -= HandleRecruitmentCompleted;
                _armyService.OnRosterUpdated -= HandleRosterUpdated;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            _armyService = armyService;
            _loc = loc;
            _modal = modal;

            if (_armyService != null)
            {
                _armyService.OnRecruitmentStarted += HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled += HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted += HandleRecruitmentCompleted;
                _armyService.OnRosterUpdated += HandleRosterUpdated;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }

            BindButtonListeners();
            Close();
        }

        private void OnDestroy()
        {
            if (_armyService != null)
            {
                _armyService.OnRecruitmentStarted -= HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled -= HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted -= HandleRecruitmentCompleted;
                _armyService.OnRosterUpdated -= HandleRosterUpdated;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        public void OpenForUnit(UnitType type)
        {
            _currentType = type;
            _currentQuantity = type == UnitType.Infantry ? 10 : 1;

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            Refresh();
        }

        public void Close()
        {
            _currentType = null;
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void HandleDecrement()
        {
            if (!_currentType.HasValue) return;
            int step = _currentType.Value == UnitType.Infantry ? 5 : 1;
            int min = _currentType.Value == UnitType.Infantry ? 5 : 1;
            _currentQuantity = Math.Max(min, _currentQuantity - step);
            Refresh();
        }

        private void HandleIncrement()
        {
            if (!_currentType.HasValue) return;
            int step = _currentType.Value == UnitType.Infantry ? 5 : 1;
            int max = _currentType.Value == UnitType.Infantry ? 100 : 20;
            _currentQuantity = Math.Min(max, _currentQuantity + step);
            Refresh();
        }

        private void HandleMax()
        {
            if (!_currentType.HasValue) return;
            _currentQuantity = _currentType.Value == UnitType.Infantry ? 100 : 20;
            Refresh();
        }

        public void Refresh()
        {
            if (!_currentType.HasValue || _armyService == null)
            {
                if (_panelRoot != null) _panelRoot.SetActive(false);
                return;
            }

            var type = _currentType.Value;
            if (!_armyService.DataSource.TryGetDefinition(type, out var def))
            {
                Close();
                return;
            }

            // Header
            if (_unitNameText != null)
            {
                _unitNameText.text = _loc != null ? _loc.GetString(def.NameKey) : def.Type.ToString();
            }

            if (_roleText != null)
            {
                _roleText.text = _loc != null ? _loc.GetString(def.RoleKey) : def.Type.ToString();
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = _loc != null ? _loc.GetString(def.DescKey) : string.Empty;
            }

            // Stats
            if (_hpText != null) _hpText.text = $"{def.Stats.Health}";
            if (_attackText != null) _attackText.text = $"{def.Stats.Attack}";
            if (_defenseText != null) _defenseText.text = $"{def.Stats.Defense}";
            if (_speedText != null) _speedText.text = $"{def.Stats.Speed:F1} m/s";
            if (_rangeText != null) _rangeText.text = $"{def.Stats.Range:F1} m";
            if (_powerText != null) _powerText.text = $"{def.Stats.CombatPower}";

            // Quantity
            if (_quantityText != null)
            {
                _quantityText.text = $"x{_currentQuantity}";
            }

            // Costs scaled by quantity
            var totalCost = def.Cost.Multiply(_currentQuantity);
            if (_costGoldText != null) _costGoldText.text = $"{totalCost.Gold:N0}";
            if (_costOilText != null) _costOilText.text = $"{totalCost.Oil:N0}";
            if (_costIronText != null) _costIronText.text = $"{totalCost.Iron:N0}";
            if (_durationText != null)
            {
                var min = totalCost.RecruitDurationSeconds / 60;
                var sec = totalCost.RecruitDurationSeconds % 60;
                _durationText.text = $"{min:D2}:{sec:D2}";
            }

            // Prerequisite check
            bool prereqMet = _armyService.CheckPrerequisites(type, out string prereqReason);
            if (_prerequisiteText != null)
            {
                if (prereqMet)
                {
                    string metStr = _loc != null ? _loc.GetString("army.drawer.req_met") : "Prerequisites Satisfied";
                    _prerequisiteText.text = $"✔ {metStr}";
                    _prerequisiteText.color = new Color(0.2f, 0.8f, 0.4f, 1f); // Green
                }
                else
                {
                    _prerequisiteText.text = $"✖ {prereqReason}";
                    _prerequisiteText.color = new Color(0.9f, 0.3f, 0.3f, 1f); // Red
                }
            }

            // Active recruitment check
            var activeQueues = _armyService.GetActiveQueues();
            RecruitmentQueueItem activeForThisType = null;
            for (var i = 0; i < activeQueues.Count; i++)
            {
                if (activeQueues[i].Type == type)
                {
                    activeForThisType = activeQueues[i];
                    break;
                }
            }

            bool isTraining = activeForThisType != null && !activeForThisType.IsCompleted(DateTime.UtcNow);
            if (_trainingStatusRoot != null)
            {
                _trainingStatusRoot.SetActive(isTraining);
            }

            if (_costsRoot != null)
            {
                _costsRoot.SetActive(!isTraining);
            }

            if (_btnRecruit != null)
            {
                int maxQueues = _armyService.MaxConcurrentQueues;
                bool canRecruit = prereqMet && !isTraining && (activeQueues.Count < maxQueues);
                _btnRecruit.gameObject.SetActive(!isTraining);
                _btnRecruit.interactable = canRecruit;
            }

            if (_btnRecruitText != null)
            {
                _btnRecruitText.text = _loc != null ? _loc.GetString("army.drawer.action_recruit") : "Recruit";
            }

            if (_btnCancelRecruit != null)
            {
                _btnCancelRecruit.gameObject.SetActive(isTraining);
            }

            if (isTraining)
            {
                UpdateCountdown(activeForThisType);
            }
        }

        private void Update()
        {
            if (!IsVisible || !_currentType.HasValue || _armyService == null) return;

            var activeQueues = _armyService.GetActiveQueues();
            RecruitmentQueueItem activeItem = null;
            for (var i = 0; i < activeQueues.Count; i++)
            {
                if (activeQueues[i].Type == _currentType.Value)
                {
                    activeItem = activeQueues[i];
                    break;
                }
            }

            if (activeItem != null && !activeItem.IsCompleted(DateTime.UtcNow))
            {
                UpdateCountdown(activeItem);
            }
            else if (activeItem != null && activeItem.IsCompleted(DateTime.UtcNow))
            {
                // Synchronize completed recruitments
                _armyService.CheckCompletedRecruitments(DateTime.UtcNow);
                Refresh();
            }
        }

        private void UpdateCountdown(RecruitmentQueueItem item)
        {
            var nowUtc = DateTime.UtcNow;
            var remaining = Math.Max(0, (int)item.RemainingSeconds(nowUtc));
            var minutes = remaining / 60;
            var seconds = remaining % 60;

            if (_countdownText != null)
            {
                string format = _loc != null ? _loc.GetString("army.drawer.time_remaining") : "Time Remaining: {0}";
                _countdownText.text = string.Format(format, $"{minutes:D2}:{seconds:D2}");
            }

            if (_progressBarFill != null)
            {
                _progressBarFill.fillAmount = item.ProgressNormalized(nowUtc);
            }
        }

        private async void HandleRecruitClicked()
        {
            if (!_currentType.HasValue || _armyService == null) return;
            var result = await _armyService.RequestRecruitmentAsync(_currentType.Value, _currentQuantity);
            if (result.IsSuccess)
            {
                Refresh();
            }
            else
            {
                string errTitle = _loc != null ? _loc.GetString("dialog.error_title") : "Error";
                _modal?.ShowAlert(errTitle, result.Error.Message);
            }
        }

        private void HandleCancelClicked()
        {
            if (!_currentType.HasValue || _armyService == null) return;

            var activeQueues = _armyService.GetActiveQueues();
            RecruitmentQueueItem target = null;
            for (var i = 0; i < activeQueues.Count; i++)
            {
                if (activeQueues[i].Type == _currentType.Value)
                {
                    target = activeQueues[i];
                    break;
                }
            }

            if (target == null) return;

            string title = _loc != null ? _loc.GetString("army.cancel_dialog.title") : "Cancel Recruitment";
            string msg = _loc != null ? _loc.GetString("army.cancel_dialog.msg") : "Are you sure you want to abort?";
            string confirmText = _loc != null ? _loc.GetString("common.confirm") : "Confirm";
            string cancelText = _loc != null ? _loc.GetString("common.cancel") : "Cancel";

            _modal?.ShowConfirm(
                title: title,
                message: msg,
                confirmText: confirmText,
                cancelText: cancelText,
                onConfirm: async () =>
                {
                    await _armyService.CancelRecruitmentAsync(target.QueueId);
                    Refresh();
                },
                onCancel: null);
        }

        private void HandleRecruitmentStarted(RecruitmentQueueItem item)
        {
            if (_currentType.HasValue && item.Type == _currentType.Value)
            {
                Refresh();
            }
        }

        private void HandleRecruitmentCancelled(string queueId)
        {
            Refresh();
        }

        private void HandleRecruitmentCompleted(UnitType type, int count)
        {
            Refresh();
        }

        private void HandleRosterUpdated()
        {
            Refresh();
        }

        private void HandleLanguageChanged(Language language)
        {
            Refresh();
        }

        public void SetupComponentsForTest(
            GameObject panelRoot,
            TextMeshProUGUI unitNameText,
            TextMeshProUGUI roleText,
            TextMeshProUGUI descriptionText,
            TextMeshProUGUI hpText,
            TextMeshProUGUI attackText,
            TextMeshProUGUI defenseText,
            TextMeshProUGUI speedText,
            TextMeshProUGUI rangeText,
            TextMeshProUGUI powerText,
            Button btnDecrement,
            Button btnIncrement,
            Button btnMax,
            TextMeshProUGUI quantityText,
            GameObject costsRoot,
            TextMeshProUGUI costGoldText,
            TextMeshProUGUI costOilText,
            TextMeshProUGUI costIronText,
            TextMeshProUGUI durationText,
            TextMeshProUGUI prerequisiteText,
            Button btnRecruit,
            TextMeshProUGUI btnRecruitText,
            Button btnCancelRecruit,
            Button btnClose,
            GameObject trainingStatusRoot,
            TextMeshProUGUI countdownText,
            Image progressBarFill)
        {
            _panelRoot = panelRoot;
            _unitNameText = unitNameText;
            _roleText = roleText;
            _descriptionText = descriptionText;
            _hpText = hpText;
            _attackText = attackText;
            _defenseText = defenseText;
            _speedText = speedText;
            _rangeText = rangeText;
            _powerText = powerText;
            _btnDecrement = btnDecrement;
            _btnIncrement = btnIncrement;
            _btnMax = btnMax;
            _quantityText = quantityText;
            _costsRoot = costsRoot;
            _costGoldText = costGoldText;
            _costOilText = costOilText;
            _costIronText = costIronText;
            _durationText = durationText;
            _prerequisiteText = prerequisiteText;
            _btnRecruit = btnRecruit;
            _btnRecruitText = btnRecruitText;
            _btnCancelRecruit = btnCancelRecruit;
            _btnClose = btnClose;
            _trainingStatusRoot = trainingStatusRoot;
            _countdownText = countdownText;
            _progressBarFill = progressBarFill;

            BindButtonListeners();
        }
    }
}
