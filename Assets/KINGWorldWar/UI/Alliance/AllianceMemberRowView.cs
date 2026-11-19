using System;
using KingWorldWar.Core.Alliance;
using KingWorldWar.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Alliance
{
    /// <summary>
    /// UI row widget representing a member within the alliance roster.
    /// Dynamically evaluates and reveals action controls based on military hierarchy permissions.
    /// </summary>
    public class AllianceMemberRowView : MonoBehaviour
    {
        [Header("Member Labels")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _roleText;
        [SerializeField] private TextMeshProUGUI _powerText;
        [SerializeField] private TextMeshProUGUI _contributionText;

        [Header("Action Controls")]
        [SerializeField] private Button _btnKick;
        [SerializeField] private Button _btnPromote;
        [SerializeField] private Button _btnDemote;

        public TextMeshProUGUI NameText => _nameText;
        public TextMeshProUGUI RoleText => _roleText;
        public TextMeshProUGUI PowerText => _powerText;
        public TextMeshProUGUI ContributionText => _contributionText;
        public Button BtnKick => _btnKick;
        public Button BtnPromote => _btnPromote;
        public Button BtnDemote => _btnDemote;

        private AllianceMember _member;
        private Action<AllianceMember> _onKick;
        private Action<AllianceMember> _onPromote;
        private Action<AllianceMember> _onDemote;

        private void Awake()
        {
            if (_btnKick != null) _btnKick.onClick.AddListener(HandleKickClicked);
            if (_btnPromote != null) _btnPromote.onClick.AddListener(HandlePromoteClicked);
            if (_btnDemote != null) _btnDemote.onClick.AddListener(HandleDemoteClicked);
        }

        public void Bind(
            AllianceMember member,
            AllianceRole viewerRole,
            ILocalizationService loc,
            Action<AllianceMember> onKick = null,
            Action<AllianceMember> onPromote = null,
            Action<AllianceMember> onDemote = null)
        {
            _member = member;
            _onKick = onKick;
            _onPromote = onPromote;
            _onDemote = onDemote;

            if (_nameText != null)
            {
                _nameText.text = member != null ? member.PlayerName : string.Empty;
            }

            if (_roleText != null)
            {
                string roleKey = member != null ? $"alliance.role.{member.Role.ToString().ToLowerInvariant()}" : "alliance.role.member";
                _roleText.text = loc != null ? loc.GetString(roleKey) : (member?.Role.ToString() ?? "Member");
            }

            if (_powerText != null)
            {
                string format = loc != null ? loc.GetString("alliance.member.power") : "Power: {0}";
                _powerText.text = string.Format(format, member?.CombatPower.ToString("N0") ?? "0");
            }

            if (_contributionText != null)
            {
                string format = loc != null ? loc.GetString("alliance.member.contrib") : "Contribution: {0}";
                _contributionText.text = string.Format(format, member?.ContributionScore.ToString("N0") ?? "0");
            }

            // Permissions evaluation
            bool canKick = member != null && viewerRole.CanKick(member.Role);
            if (_btnKick != null)
            {
                _btnKick.gameObject.SetActive(canKick);
            }

            // Can promote if there is a higher role and viewer has permission
            bool canPromote = false;
            if (member != null && (int)member.Role > 1)
            {
                var targetRank = (AllianceRole)((int)member.Role - 1);
                canPromote = viewerRole.CanPromote(member.Role, targetRank);
            }
            if (_btnPromote != null)
            {
                _btnPromote.gameObject.SetActive(canPromote);
            }

            // Can demote if there is a lower role and viewer has permission
            bool canDemote = false;
            if (member != null && (int)member.Role < 5)
            {
                var targetRank = (AllianceRole)((int)member.Role + 1);
                canDemote = viewerRole.CanDemote(member.Role, targetRank);
            }
            if (_btnDemote != null)
            {
                _btnDemote.gameObject.SetActive(canDemote);
            }
        }

        private void HandleKickClicked()
        {
            if (_member != null) _onKick?.Invoke(_member);
        }

        private void HandlePromoteClicked()
        {
            if (_member != null) _onPromote?.Invoke(_member);
        }

        private void HandleDemoteClicked()
        {
            if (_member != null) _onDemote?.Invoke(_member);
        }

        #if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
        public void SetupComponentsForTest(
            TextMeshProUGUI nameText,
            TextMeshProUGUI roleText,
            TextMeshProUGUI powerText,
            TextMeshProUGUI contribText,
            Button btnKick,
            Button btnPromote,
            Button btnDemote)
        {
            _nameText = nameText;
            _roleText = roleText;
            _powerText = powerText;
            _contributionText = contribText;
            _btnKick = btnKick;
            _btnPromote = btnPromote;
            _btnDemote = btnDemote;

            if (_btnKick != null) _btnKick.onClick.AddListener(HandleKickClicked);
            if (_btnPromote != null) _btnPromote.onClick.AddListener(HandlePromoteClicked);
            if (_btnDemote != null) _btnDemote.onClick.AddListener(HandleDemoteClicked);
        }
        #endif
    }
}
