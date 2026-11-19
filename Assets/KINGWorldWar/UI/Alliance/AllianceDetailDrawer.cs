using System;
using KingWorldWar.Core.Alliance;
using KingWorldWar.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Alliance
{
    /// <summary>
    /// Slide-in inspection drawer displaying public alliance credentials,
    /// power statistics, leadership, notice bulletin, and join/apply dispatch controls.
    /// </summary>
    public class AllianceDetailDrawer : MonoBehaviour
    {
        [Header("Header Labels")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _tagText;

        [Header("Statistics & Policy")]
        [SerializeField] private TextMeshProUGUI _combatPowerText;
        [SerializeField] private TextMeshProUGUI _membersCountText;
        [SerializeField] private TextMeshProUGUI _leaderText;
        [SerializeField] private TextMeshProUGUI _policyText;
        [SerializeField] private TextMeshProUGUI _noticeText;

        [Header("Action Controls")]
        [SerializeField] private Button _btnAction;
        [SerializeField] private TextMeshProUGUI _btnActionText;
        [SerializeField] private Button _btnClose;

        public TextMeshProUGUI NameText => _nameText;
        public TextMeshProUGUI TagText => _tagText;
        public TextMeshProUGUI CombatPowerText => _combatPowerText;
        public TextMeshProUGUI MembersCountText => _membersCountText;
        public TextMeshProUGUI LeaderText => _leaderText;
        public TextMeshProUGUI PolicyText => _policyText;
        public TextMeshProUGUI NoticeText => _noticeText;
        public Button BtnAction => _btnAction;
        public TextMeshProUGUI BtnActionText => _btnActionText;
        public Button BtnClose => _btnClose;
        public bool IsOpen => gameObject.activeSelf;

        public event Action<AllianceProfile> OnJoinRequested;
        public event Action<AllianceProfile> OnApplyRequested;

        private AllianceProfile _targetAlliance;
        private ILocalizationService _loc;

        private void Awake()
        {
            if (_btnClose != null) _btnClose.onClick.AddListener(Close);
            if (_btnAction != null) _btnAction.onClick.AddListener(HandleActionClicked);
        }

        public void Bind(ILocalizationService loc)
        {
            _loc = loc;
        }

        public void OpenForAlliance(AllianceProfile profile, bool userHasAlliance)
        {
            _targetAlliance = profile;
            gameObject.SetActive(true);

            if (_titleText != null)
            {
                _titleText.text = _loc != null ? _loc.GetString("alliance.drawer.title") : "Coalition Dossier";
            }

            if (_nameText != null)
            {
                _nameText.text = profile?.Name ?? string.Empty;
            }

            if (_tagText != null)
            {
                string tagFormat = _loc != null ? _loc.GetString("alliance.drawer.tag") : "Tag: [{0}]";
                _tagText.text = string.Format(tagFormat, profile?.Tag ?? string.Empty);
            }

            if (_combatPowerText != null)
            {
                string powerFormat = _loc != null ? _loc.GetString("alliance.drawer.combat_power") : "Total Power: {0}";
                _combatPowerText.text = string.Format(powerFormat, profile?.TotalCombatPower.ToString("N0") ?? "0");
            }

            if (_membersCountText != null)
            {
                string membersFormat = _loc != null ? _loc.GetString("alliance.drawer.members") : "Members: {0} / {1}";
                _membersCountText.text = string.Format(membersFormat, profile?.MemberCount ?? 0, profile?.MaxMembers ?? 50);
            }

            if (_leaderText != null)
            {
                string leaderFormat = _loc != null ? _loc.GetString("alliance.drawer.leader") : "Commander: {0}";
                _leaderText.text = string.Format(leaderFormat, profile?.LeaderPlayerName ?? string.Empty);
            }

            if (_policyText != null)
            {
                string policyKey = profile != null ? $"alliance.policy.{profile.JoinPolicy.ToString().ToLowerInvariant()}" : "alliance.policy.open";
                _policyText.text = _loc != null ? _loc.GetString(policyKey) : (profile?.JoinPolicy.ToString() ?? "Open");
            }

            if (_noticeText != null)
            {
                _noticeText.text = profile?.Notice ?? string.Empty;
            }

            // Update Action Button
            UpdateActionButton(profile, userHasAlliance);
        }

        private void UpdateActionButton(AllianceProfile profile, bool userHasAlliance)
        {
            if (_btnAction == null) return;

            if (userHasAlliance)
            {
                _btnAction.interactable = false;
                if (_btnActionText != null)
                {
                    _btnActionText.text = _loc != null ? _loc.GetString("alliance.error.already_in") : "Already in an alliance";
                }
                return;
            }

            if (profile == null)
            {
                _btnAction.interactable = false;
                return;
            }

            if (profile.IsFull)
            {
                _btnAction.interactable = false;
                if (_btnActionText != null)
                {
                    _btnActionText.text = _loc != null ? _loc.GetString("alliance.drawer.full") : "Alliance is Full";
                }
                return;
            }

            if (profile.JoinPolicy == AllianceJoinPolicy.Open)
            {
                _btnAction.interactable = true;
                if (_btnActionText != null)
                {
                    _btnActionText.text = _loc != null ? _loc.GetString("alliance.drawer.action_join") : "Join Coalition";
                }
            }
            else if (profile.JoinPolicy == AllianceJoinPolicy.ApplyOnly)
            {
                _btnAction.interactable = true;
                if (_btnActionText != null)
                {
                    _btnActionText.text = _loc != null ? _loc.GetString("alliance.drawer.action_apply") : "Submit Application";
                }
            }
            else // Closed
            {
                _btnAction.interactable = false;
                if (_btnActionText != null)
                {
                    _btnActionText.text = _loc != null ? _loc.GetString("alliance.policy.closed") : "Closed";
                }
            }
        }

        private void HandleActionClicked()
        {
            if (_targetAlliance == null) return;

            if (_targetAlliance.JoinPolicy == AllianceJoinPolicy.Open)
            {
                OnJoinRequested?.Invoke(_targetAlliance);
            }
            else if (_targetAlliance.JoinPolicy == AllianceJoinPolicy.ApplyOnly)
            {
                OnApplyRequested?.Invoke(_targetAlliance);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            _targetAlliance = null;
        }

        #if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
        public void SetupComponentsForTest(
            TextMeshProUGUI titleText,
            TextMeshProUGUI nameText,
            TextMeshProUGUI tagText,
            TextMeshProUGUI powerText,
            TextMeshProUGUI membersText,
            TextMeshProUGUI leaderText,
            TextMeshProUGUI policyText,
            TextMeshProUGUI noticeText,
            Button btnAction,
            TextMeshProUGUI btnActionText,
            Button btnClose)
        {
            _titleText = titleText;
            _nameText = nameText;
            _tagText = tagText;
            _combatPowerText = powerText;
            _membersCountText = membersText;
            _leaderText = leaderText;
            _policyText = policyText;
            _noticeText = noticeText;
            _btnAction = btnAction;
            _btnActionText = btnActionText;
            _btnClose = btnClose;

            if (_btnClose != null) _btnClose.onClick.AddListener(Close);
            if (_btnAction != null) _btnAction.onClick.AddListener(HandleActionClicked);
        }
        #endif
    }
}
