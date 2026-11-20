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
    /// UI component representing a single military training queue operation slot.
    /// Presents state breakdown (Loading, Pending, InProgress, Completed, Failed, Cancelled),
    /// real-time server timestamp countdown, progress bar, and cancellation with server-defined refund notice.
    /// </summary>
    public class TrainingQueueItemView : MonoBehaviour
    {
        [Header("Item Info")]
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private TextMeshProUGUI _stateBadgeText;
        [SerializeField] private Image _stateBadgeBg;

        [Header("Progress & Countdown")]
        [SerializeField] private Image _progressBarFill;
        [SerializeField] private TextMeshProUGUI _countdownText;

        [Header("Actions")]
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;

        private RecruitmentQueueItem _item;
        private IArmyService _armyService;
        private ILocalizationService _loc;
        private IModalService _modal;

        public RecruitmentQueueItem Item => _item;
        public TextMeshProUGUI UnitNameText => _unitNameText;
        public TextMeshProUGUI QuantityText => _quantityText;
        public TextMeshProUGUI StateBadgeText => _stateBadgeText;
        public TextMeshProUGUI CountdownText => _countdownText;
        public Button CancelButton => _cancelButton;

        public void Initialize(
            RecruitmentQueueItem item,
            IArmyService armyService,
            ILocalizationService loc,
            IModalService modal = null)
        {
            _item = item;
            _armyService = armyService;
            _loc = loc;
            _modal = modal;

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }

            Refresh();
        }

        public void BindItem(RecruitmentQueueItem item)
        {
            _item = item;
            Refresh();
        }

        public void Refresh()
        {
            if (_item == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_unitNameText != null)
            {
                string key = $"unit.{_item.Type.ToString().ToLowerInvariant()}.name";
                _unitNameText.text = _loc != null ? _loc.GetString(key) : _item.Type.ToString();
            }

            if (_quantityText != null)
            {
                _quantityText.text = $"x{_item.Count}";
            }

            if (_cancelButtonText != null)
            {
                _cancelButtonText.text = _loc != null ? _loc.GetString("common.cancel") : "Cancel";
            }

            UpdateStateDisplay();
            UpdateProgress();
        }

        private void Update()
        {
            if (_item == null || _item.State != TrainingQueueState.InProgress) return;
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (_item == null) return;

            var now = DateTime.UtcNow;
            float progress = _item.ProgressNormalized(now);

            if (_progressBarFill != null)
            {
                _progressBarFill.fillAmount = Mathf.Clamp01(progress);
            }

            if (_countdownText != null)
            {
                if (_item.State == TrainingQueueState.InProgress)
                {
                    int remaining = (int)Math.Max(0, _item.RemainingSeconds(now));
                    _countdownText.text = $"{remaining / 60:D2}:{remaining % 60:D2}";
                }
                else
                {
                    _countdownText.text = string.Empty;
                }
            }
        }

        private void UpdateStateDisplay()
        {
            if (_stateBadgeText == null) return;

            string stateKey;
            Color badgeColor;

            switch (_item.State)
            {
                case TrainingQueueState.Loading:
                    stateKey = "army.queue.loading";
                    badgeColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                    break;
                case TrainingQueueState.Pending:
                    stateKey = "army.queue.pending";
                    badgeColor = new Color(0.85f, 0.65f, 0.15f, 1f); // Amber
                    break;
                case TrainingQueueState.InProgress:
                    stateKey = "army.queue.in_progress";
                    badgeColor = new Color(0.2f, 0.6f, 0.9f, 1f); // Blue
                    break;
                case TrainingQueueState.Completed:
                    stateKey = "army.queue.completed";
                    badgeColor = new Color(0.2f, 0.8f, 0.3f, 1f); // Green
                    break;
                case TrainingQueueState.Failed:
                    stateKey = "army.queue.failed";
                    badgeColor = new Color(0.9f, 0.25f, 0.25f, 1f); // Red
                    break;
                case TrainingQueueState.Cancelled:
                    stateKey = "army.queue.cancelled";
                    badgeColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Gray
                    break;
                default:
                    stateKey = "army.queue.in_progress";
                    badgeColor = Color.gray;
                    break;
            }

            _stateBadgeText.text = _loc != null ? _loc.GetString(stateKey) : _item.State.ToString();
            if (_stateBadgeBg != null)
            {
                _stateBadgeBg.color = badgeColor;
            }

            if (_cancelButton != null)
            {
                _cancelButton.gameObject.SetActive(_item.State == TrainingQueueState.InProgress || _item.State == TrainingQueueState.Pending);
            }
        }

        private void OnCancelClicked()
        {
            if (_armyService == null || _item == null) return;

            string title = _loc != null ? _loc.GetString("army.cancel_dialog.title") : "Cancel Mobilization";
            string refundMsg = _loc != null ? _loc.GetString("army.queue.refund_notice") : "Cancelling will refund 50% of invested resources.";

            if (_modal != null)
            {
                _modal.ShowConfirm(
                    title: title,
                    message: refundMsg,
                    onConfirm: () =>
                    {
                        _ = _armyService.CancelRecruitmentAsync(_item.QueueId);
                    },
                    onCancel: null);
            }
            else
            {
                _ = _armyService.CancelRecruitmentAsync(_item.QueueId);
            }
        }

        public void SetupForTest(
            TextMeshProUGUI unitNameText,
            TextMeshProUGUI quantityText,
            TextMeshProUGUI stateBadgeText,
            TextMeshProUGUI countdownText,
            Button cancelButton,
            Image progressBarFill = null)
        {
            _unitNameText = unitNameText;
            _quantityText = quantityText;
            _stateBadgeText = stateBadgeText;
            _countdownText = countdownText;
            _cancelButton = cancelButton;
            _progressBarFill = progressBarFill;
        }
    }
}
