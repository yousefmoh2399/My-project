using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KingWorldWar.Core.Alliance;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Alliance
{
    /// <summary>
    /// Tactical dual-state Alliance screen (implements IScreen for ScreenId.Alliance).
    /// State A: Public Coalition Directory, search filtering, and alliance establishment (10,000 Gold).
    /// State B: Active Coalition Command, roster management, permissions enforcement, notice bulletin, and application triage.
    /// </summary>
    public class AllianceScreenView : MonoBehaviour, IScreen
    {
        public ScreenId ScreenId => ScreenId.Alliance;
        public bool IsVisible => gameObject.activeSelf;

        [Header("Dual-State Roots")]
        [SerializeField] private GameObject _noAllianceRoot;
        [SerializeField] private GameObject _inAllianceRoot;

        [Header("State A: No Alliance Elements")]
        [SerializeField] private TextMeshProUGUI _noAllianceTitleText;
        [SerializeField] private TextMeshProUGUI _noAllianceDescText;
        [SerializeField] private TextMeshProUGUI _mockBadgeNoAllianceText;
        [SerializeField] private TMP_InputField _searchInput;
        [SerializeField] private Button _btnSearch;
        [SerializeField] private Transform _directoryContainer;
        [SerializeField] private TextMeshProUGUI _directoryEmptyText;
        [SerializeField] private Button _btnOpenCreatePanel;
        [SerializeField] private GameObject _createPanel;
        [SerializeField] private TMP_InputField _createNameInput;
        [SerializeField] private TMP_InputField _createTagInput;
        [SerializeField] private TMP_InputField _createNoticeInput;
        [SerializeField] private TextMeshProUGUI _createCostText;
        [SerializeField] private Button _btnCreateSubmit;
        [SerializeField] private TextMeshProUGUI _createErrorText;

        [Header("State B: In Alliance Elements")]
        [SerializeField] private TextMeshProUGUI _allianceTitleText;
        [SerializeField] private TextMeshProUGUI _allianceNameText;
        [SerializeField] private TextMeshProUGUI _allianceTagText;
        [SerializeField] private TextMeshProUGUI _alliancePowerText;
        [SerializeField] private TextMeshProUGUI _allianceMembersText;
        [SerializeField] private TextMeshProUGUI _allianceLeaderText;
        [SerializeField] private TextMeshProUGUI _mockBadgeInAllianceText;

        [Header("Tabs & Sub-Views")]
        [SerializeField] private Button _btnTabRoster;
        [SerializeField] private Button _btnTabNotice;
        [SerializeField] private Button _btnTabApps;
        [SerializeField] private TextMeshProUGUI _appsBadgeCountText;
        [SerializeField] private GameObject _rosterRoot;
        [SerializeField] private Transform _rosterContainer;
        [SerializeField] private TextMeshProUGUI _rosterEmptyText;
        [SerializeField] private GameObject _noticeRoot;
        [SerializeField] private TextMeshProUGUI _noticeDisplayText;
        [SerializeField] private Button _btnEditNotice;
        [SerializeField] private GameObject _noticeEditPanel;
        [SerializeField] private TMP_InputField _noticeInputField;
        [SerializeField] private Button _btnSaveNotice;
        [SerializeField] private GameObject _appsRoot;
        [SerializeField] private Transform _appsContainer;
        [SerializeField] private TextMeshProUGUI _appsEmptyText;
        [SerializeField] private Button _btnLeaveAlliance;

        [Header("Prefabs & Drawer")]
        [SerializeField] private AllianceMemberRowView _memberRowPrefab;
        [SerializeField] private AllianceDetailDrawer _detailDrawer;

        private IAllianceService _allianceService;
        private ILocalizationService _loc;
        private IModalService _modal;

        public GameObject NoAllianceRoot => _noAllianceRoot;
        public GameObject InAllianceRoot => _inAllianceRoot;
        public TMP_InputField SearchInput => _searchInput;
        public Button BtnSearch => _btnSearch;
        public Transform DirectoryContainer => _directoryContainer;
        public Button BtnOpenCreatePanel => _btnOpenCreatePanel;
        public GameObject CreatePanel => _createPanel;
        public TMP_InputField CreateNameInput => _createNameInput;
        public TMP_InputField CreateTagInput => _createTagInput;
        public TMP_InputField CreateNoticeInput => _createNoticeInput;
        public Button BtnCreateSubmit => _btnCreateSubmit;
        public TextMeshProUGUI CreateErrorText => _createErrorText;

        public TextMeshProUGUI AllianceNameText => _allianceNameText;
        public TextMeshProUGUI AllianceTagText => _allianceTagText;
        public TextMeshProUGUI AlliancePowerText => _alliancePowerText;
        public TextMeshProUGUI AllianceMembersText => _allianceMembersText;
        public TextMeshProUGUI AllianceLeaderText => _allianceLeaderText;

        public Button BtnTabRoster => _btnTabRoster;
        public Button BtnTabNotice => _btnTabNotice;
        public Button BtnTabApps => _btnTabApps;
        public Transform RosterContainer => _rosterContainer;
        public Transform AppsContainer => _appsContainer;
        public Button BtnLeaveAlliance => _btnLeaveAlliance;
        public AllianceDetailDrawer DetailDrawer => _detailDrawer;

        private void Awake()
        {
            BindLocalButtons();
        }

        private void BindLocalButtons()
        {
            if (_btnSearch != null) _btnSearch.onClick.AddListener(() => _ = HandleSearchClicked());
            if (_btnOpenCreatePanel != null) _btnOpenCreatePanel.onClick.AddListener(ToggleCreatePanel);
            if (_btnCreateSubmit != null) _btnCreateSubmit.onClick.AddListener(() => _ = HandleCreateSubmitClicked());

            if (_btnTabRoster != null) _btnTabRoster.onClick.AddListener(ShowRosterTab);
            if (_btnTabNotice != null) _btnTabNotice.onClick.AddListener(ShowNoticeTab);
            if (_btnTabApps != null) _btnTabApps.onClick.AddListener(ShowAppsTab);

            if (_btnEditNotice != null) _btnEditNotice.onClick.AddListener(ToggleNoticeEdit);
            if (_btnSaveNotice != null) _btnSaveNotice.onClick.AddListener(() => _ = HandleSaveNoticeClicked());

            if (_btnLeaveAlliance != null) _btnLeaveAlliance.onClick.AddListener(HandleLeaveClicked);

            if (_detailDrawer != null)
            {
                _detailDrawer.OnJoinRequested += (profile) => _ = HandleJoinRequested(profile);
                _detailDrawer.OnApplyRequested += (profile) => _ = HandleApplyRequested(profile);
            }
        }

        public void Initialize(
            IAllianceService allianceService,
            ILocalizationService loc,
            IModalService modal,
            AllianceDetailDrawer drawer = null)
        {
            if (_allianceService != null)
            {
                _allianceService.OnAllianceUpdated -= HandleAllianceUpdated;
                _allianceService.OnMembersUpdated -= HandleMembersUpdated;
                _allianceService.OnApplicationsUpdated -= HandleApplicationsUpdated;
                _allianceService.OnLeftAlliance -= HandleLeftAlliance;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            _allianceService = allianceService;
            _loc = loc;
            _modal = modal;

            if (drawer != null)
            {
                _detailDrawer = drawer;
                _detailDrawer.Bind(_loc);
                _detailDrawer.OnJoinRequested += (profile) => _ = HandleJoinRequested(profile);
                _detailDrawer.OnApplyRequested += (profile) => _ = HandleApplyRequested(profile);
            }

            if (_allianceService != null)
            {
                _allianceService.OnAllianceUpdated += HandleAllianceUpdated;
                _allianceService.OnMembersUpdated += HandleMembersUpdated;
                _allianceService.OnApplicationsUpdated += HandleApplicationsUpdated;
                _allianceService.OnLeftAlliance += HandleLeftAlliance;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }

            Refresh();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Refresh();
            if (_allianceService != null)
            {
                _ = _allianceService.RefreshCurrentAllianceAsync();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (_detailDrawer != null) _detailDrawer.Close();
        }

        public void Refresh()
        {
            UpdateStaticLabels();

            bool inAlliance = _allianceService != null && _allianceService.HasAlliance;

            if (_noAllianceRoot != null) _noAllianceRoot.SetActive(!inAlliance);
            if (_inAllianceRoot != null) _inAllianceRoot.SetActive(inAlliance);

            if (inAlliance)
            {
                PopulateInAllianceView();
            }
            else
            {
                _ = SearchAndPopulateDirectory(string.Empty);
            }
        }

        private void UpdateStaticLabels()
        {
            if (_noAllianceTitleText != null)
                _noAllianceTitleText.text = _loc != null ? _loc.GetString("alliance.screen.no_alliance_title") : "Alliances & Coalitions";

            if (_noAllianceDescText != null)
                _noAllianceDescText.text = _loc != null ? _loc.GetString("alliance.screen.no_alliance_desc") : "Join an established coalition or establish your own military alliance.";

            string mockBadge = _loc != null ? _loc.GetString("alliance.screen.mock_badge") : "MOCK DATA (Phase 7)";
            if (_mockBadgeNoAllianceText != null) _mockBadgeNoAllianceText.text = mockBadge;
            if (_mockBadgeInAllianceText != null) _mockBadgeInAllianceText.text = mockBadge;

            if (_allianceTitleText != null)
                _allianceTitleText.text = _loc != null ? _loc.GetString("alliance.screen.title") : "Coalition Command";

            if (_createCostText != null)
                _createCostText.text = _loc != null ? _loc.GetString("alliance.create.cost_label") : "Cost: 10,000 Gold";
        }

        private void PopulateInAllianceView()
        {
            var alliance = _allianceService?.CurrentAlliance;
            if (alliance == null) return;

            if (_allianceNameText != null) _allianceNameText.text = alliance.Name;
            if (_allianceTagText != null) _allianceTagText.text = $"[{alliance.Tag}]";

            if (_alliancePowerText != null)
            {
                string format = _loc != null ? _loc.GetString("alliance.screen.total_power") : "Coalition Power: {0}";
                _alliancePowerText.text = string.Format(format, alliance.TotalCombatPower.ToString("N0"));
            }

            if (_allianceMembersText != null)
            {
                string format = _loc != null ? _loc.GetString("alliance.screen.members_count") : "Members: {0}/{1}";
                _allianceMembersText.text = string.Format(format, alliance.MemberCount, alliance.MaxMembers);
            }

            if (_allianceLeaderText != null)
            {
                string format = _loc != null ? _loc.GetString("alliance.screen.leader") : "Leader: {0}";
                _allianceLeaderText.text = string.Format(format, alliance.LeaderPlayerName);
            }

            if (_noticeDisplayText != null)
            {
                _noticeDisplayText.text = !string.IsNullOrEmpty(alliance.Notice) ? alliance.Notice : "---";
            }

            // Permissions on Notice editing
            bool canEditNotice = _allianceService.CurrentUserRole.CanEditNotice();
            if (_btnEditNotice != null) _btnEditNotice.gameObject.SetActive(canEditNotice);

            // Tab permissions on Applications
            bool canManageApps = _allianceService.CurrentUserRole.CanManageApplications();
            if (_btnTabApps != null) _btnTabApps.gameObject.SetActive(canManageApps);

            PopulateRoster(_allianceService.CurrentMembers);
            PopulateApplications(_allianceService.PendingApplications);

            // Default to roster tab
            ShowRosterTab();
        }

        public async Task SearchAndPopulateDirectory(string query)
        {
            if (_allianceService == null) return;

            var result = await _allianceService.SearchAlliancesAsync(query).ConfigureAwait(true);
            if (!result.IsSuccess) return;

            if (_directoryContainer == null) return;

            // Clear previous items
            for (int i = _directoryContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_directoryContainer.GetChild(i).gameObject);
            }

            var list = result.Value;
            bool isEmpty = list == null || list.Count == 0;

            if (_directoryEmptyText != null)
            {
                _directoryEmptyText.gameObject.SetActive(isEmpty);
                if (isEmpty)
                {
                    _directoryEmptyText.text = _loc != null ? _loc.GetString("alliance.screen.empty_directory") : "No alliances found.";
                }
            }

            if (isEmpty) return;

            foreach (var alliance in list)
            {
                CreateDirectoryRow(alliance);
            }
        }

        private void CreateDirectoryRow(AllianceProfile alliance)
        {
            var rowObj = new GameObject($"Alliance_{alliance.Tag}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            rowObj.transform.SetParent(_directoryContainer, false);

            var img = rowObj.GetComponent<Image>();
            img.color = new Color(0.15f, 0.17f, 0.20f, 0.95f);

            var btn = rowObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (_detailDrawer != null)
                {
                    _detailDrawer.OpenForAlliance(alliance, _allianceService?.HasAlliance ?? false);
                }
            });

            var labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(rowObj.transform, false);

            var tmp = labelObj.GetComponent<TextMeshProUGUI>();
            tmp.text = $"[{alliance.Tag}] {alliance.Name}  •  {alliance.MemberCount}/{alliance.MaxMembers}  •  PWR: {alliance.TotalCombatPower:N0}";
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;

            var rt = labelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12, 4);
            rt.offsetMax = new Vector2(-12, -4);
        }

        private void PopulateRoster(IReadOnlyList<AllianceMember> members)
        {
            if (_rosterContainer == null) return;

            for (int i = _rosterContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_rosterContainer.GetChild(i).gameObject);
            }

            bool isEmpty = members == null || members.Count == 0;
            if (_rosterEmptyText != null)
            {
                _rosterEmptyText.gameObject.SetActive(isEmpty);
                if (isEmpty)
                {
                    _rosterEmptyText.text = _loc != null ? _loc.GetString("alliance.screen.empty_members") : "No members found.";
                }
            }

            if (isEmpty) return;

            var viewerRole = _allianceService != null ? _allianceService.CurrentUserRole : AllianceRole.None;

            foreach (var m in members)
            {
                AllianceMemberRowView row;
                if (_memberRowPrefab != null)
                {
                    row = Instantiate(_memberRowPrefab, _rosterContainer);
                }
                else
                {
                    var go = new GameObject($"Member_{m.PlayerName}", typeof(RectTransform), typeof(AllianceMemberRowView));
                    go.transform.SetParent(_rosterContainer, false);
                    row = go.GetComponent<AllianceMemberRowView>();
                }

                row.Bind(
                    m,
                    viewerRole,
                    _loc,
                    onKick: (target) => HandleMemberKick(target),
                    onPromote: (target) => HandleMemberPromote(target),
                    onDemote: (target) => HandleMemberDemote(target)
                );
            }
        }

        private void PopulateApplications(IReadOnlyList<AllianceApplication> apps)
        {
            if (_appsBadgeCountText != null)
            {
                int count = apps?.Count ?? 0;
                _appsBadgeCountText.text = count > 0 ? count.ToString() : string.Empty;
                _appsBadgeCountText.gameObject.SetActive(count > 0);
            }

            if (_appsContainer == null) return;

            for (int i = _appsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_appsContainer.GetChild(i).gameObject);
            }

            bool isEmpty = apps == null || apps.Count == 0;
            if (_appsEmptyText != null)
            {
                _appsEmptyText.gameObject.SetActive(isEmpty);
                if (isEmpty)
                {
                    _appsEmptyText.text = _loc != null ? _loc.GetString("alliance.screen.empty_apps") : "No pending applications.";
                }
            }

            if (isEmpty) return;

            foreach (var app in apps)
            {
                CreateApplicationRow(app);
            }
        }

        private void CreateApplicationRow(AllianceApplication app)
        {
            var rowObj = new GameObject($"App_{app.PlayerName}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rowObj.transform.SetParent(_appsContainer, false);

            var img = rowObj.GetComponent<Image>();
            img.color = new Color(0.18f, 0.20f, 0.24f, 0.95f);

            var labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(rowObj.transform, false);

            var tmp = labelObj.GetComponent<TextMeshProUGUI>();
            tmp.text = $"{app.PlayerName}  •  PWR: {app.CombatPower:N0}";
            tmp.fontSize = 14;
            tmp.color = Color.white;

            var rt = labelObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0.6f, 1);
            rt.offsetMin = new Vector2(10, 0);
            rt.offsetMax = Vector2.zero;

            // Accept button
            var btnAcceptObj = new GameObject("BtnAccept", typeof(RectTransform), typeof(Button), typeof(Image));
            btnAcceptObj.transform.SetParent(rowObj.transform, false);
            var btnAccept = btnAcceptObj.GetComponent<Button>();
            btnAccept.onClick.AddListener(() =>
            {
                if (_allianceService != null)
                {
                    _ = _allianceService.AcceptApplicationAsync(app.ApplicationId);
                }
            });

            // Reject button
            var btnRejectObj = new GameObject("BtnReject", typeof(RectTransform), typeof(Button), typeof(Image));
            btnRejectObj.transform.SetParent(rowObj.transform, false);
            var btnReject = btnRejectObj.GetComponent<Button>();
            btnReject.onClick.AddListener(() =>
            {
                if (_allianceService != null)
                {
                    _ = _allianceService.RejectApplicationAsync(app.ApplicationId);
                }
            });
        }

        public void ShowRosterTab()
        {
            if (_rosterRoot != null) _rosterRoot.SetActive(true);
            if (_noticeRoot != null) _noticeRoot.SetActive(false);
            if (_appsRoot != null) _appsRoot.SetActive(false);
        }

        public void ShowNoticeTab()
        {
            if (_rosterRoot != null) _rosterRoot.SetActive(false);
            if (_noticeRoot != null) _noticeRoot.SetActive(true);
            if (_appsRoot != null) _appsRoot.SetActive(false);
            if (_noticeEditPanel != null) _noticeEditPanel.SetActive(false);
        }

        public void ShowAppsTab()
        {
            if (_rosterRoot != null) _rosterRoot.SetActive(false);
            if (_noticeRoot != null) _noticeRoot.SetActive(false);
            if (_appsRoot != null) _appsRoot.SetActive(true);
        }

        private void ToggleNoticeEdit()
        {
            if (_noticeEditPanel == null) return;
            bool active = !_noticeEditPanel.activeSelf;
            _noticeEditPanel.SetActive(active);

            if (active && _noticeInputField != null && _allianceService?.CurrentAlliance != null)
            {
                _noticeInputField.text = _allianceService.CurrentAlliance.Notice;
            }
        }

        private async Task HandleSaveNoticeClicked()
        {
            if (_allianceService == null || _noticeInputField == null) return;
            string newNotice = _noticeInputField.text?.Trim() ?? string.Empty;

            var result = await _allianceService.UpdateNoticeAsync(newNotice).ConfigureAwait(true);
            if (result.IsSuccess)
            {
                if (_noticeEditPanel != null) _noticeEditPanel.SetActive(false);
                if (_noticeDisplayText != null) _noticeDisplayText.text = newNotice;
            }
        }

        private void ToggleCreatePanel()
        {
            if (_createPanel != null)
            {
                _createPanel.SetActive(!_createPanel.activeSelf);
                if (_createErrorText != null) _createErrorText.text = string.Empty;
            }
        }

        private async Task HandleCreateSubmitClicked()
        {
            if (_allianceService == null) return;

            string name = _createNameInput?.text?.Trim() ?? string.Empty;
            string tag = _createTagInput?.text?.Trim() ?? string.Empty;
            string notice = _createNoticeInput?.text?.Trim() ?? string.Empty;

            var result = await _allianceService.CreateAllianceAsync(name, tag, notice, AllianceJoinPolicy.Open).ConfigureAwait(true);
            if (!result.IsSuccess)
            {
                if (_createErrorText != null)
                {
                    _createErrorText.text = result.Error.Message;
                }
            }
            else
            {
                if (_createPanel != null) _createPanel.SetActive(false);
                Refresh();
            }
        }

        private async Task HandleSearchClicked()
        {
            string query = _searchInput?.text?.Trim() ?? string.Empty;
            await SearchAndPopulateDirectory(query).ConfigureAwait(true);
        }

        private async Task HandleJoinRequested(AllianceProfile profile)
        {
            if (_allianceService == null || profile == null) return;
            var res = await _allianceService.JoinAllianceAsync(profile.Id).ConfigureAwait(true);
            if (res.IsSuccess)
            {
                if (_detailDrawer != null) _detailDrawer.Close();
                Refresh();
            }
        }

        private async Task HandleApplyRequested(AllianceProfile profile)
        {
            if (_allianceService == null || profile == null) return;
            var res = await _allianceService.ApplyToAllianceAsync(profile.Id).ConfigureAwait(true);
            if (res.IsSuccess)
            {
                if (_detailDrawer != null) _detailDrawer.Close();
                if (_modal != null)
                {
                    string title = _loc != null ? _loc.GetString("dialog.switch_lang_title") : "Success";
                    string msg = _loc != null ? _loc.GetString("alliance.app.submitted") : "Application Submitted Successfully";
                    _modal.ShowAlert(title, msg);
                }
            }
        }

        private void HandleMemberKick(AllianceMember member)
        {
            if (_allianceService == null || member == null) return;

            string title = _loc != null ? _loc.GetString("alliance.member.confirm_kick_title") : "Expel Member";
            string msg = _loc != null ? _loc.GetString("alliance.member.confirm_kick_msg") : "Are you sure you want to expel this member?";

            if (_modal != null)
            {
                _modal.ShowConfirm(title, msg, onConfirm: () =>
                {
                    _ = _allianceService.KickMemberAsync(member.PlayerId);
                });
            }
            else
            {
                _ = _allianceService.KickMemberAsync(member.PlayerId);
            }
        }

        private void HandleMemberPromote(AllianceMember member)
        {
            if (_allianceService == null || member == null || (int)member.Role <= 1) return;
            var newRole = (AllianceRole)((int)member.Role - 1);
            _ = _allianceService.SetMemberRoleAsync(member.PlayerId, newRole);
        }

        private void HandleMemberDemote(AllianceMember member)
        {
            if (_allianceService == null || member == null || (int)member.Role >= 5) return;
            var newRole = (AllianceRole)((int)member.Role + 1);
            _ = _allianceService.SetMemberRoleAsync(member.PlayerId, newRole);
        }

        private void HandleLeaveClicked()
        {
            if (_allianceService == null) return;

            string title = _loc != null ? _loc.GetString("alliance.leave.confirm_title") : "Leave Alliance";
            string msg = _loc != null ? _loc.GetString("alliance.leave.confirm_msg") : "Are you sure you want to leave the alliance?";

            if (_modal != null)
            {
                _modal.ShowConfirm(title, msg, onConfirm: () =>
                {
                    _ = _allianceService.LeaveAllianceAsync();
                });
            }
            else
            {
                _ = _allianceService.LeaveAllianceAsync();
            }
        }

        private void HandleAllianceUpdated(AllianceProfile profile)
        {
            Refresh();
        }

        private void HandleMembersUpdated(IReadOnlyList<AllianceMember> members)
        {
            if (_allianceService != null && _allianceService.HasAlliance)
            {
                PopulateRoster(members);
            }
        }

        private void HandleApplicationsUpdated(IReadOnlyList<AllianceApplication> apps)
        {
            if (_allianceService != null && _allianceService.HasAlliance)
            {
                PopulateApplications(apps);
            }
        }

        private void HandleLeftAlliance()
        {
            Refresh();
        }

        private void HandleLanguageChanged(Language lang)
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (_allianceService != null)
            {
                _allianceService.OnAllianceUpdated -= HandleAllianceUpdated;
                _allianceService.OnMembersUpdated -= HandleMembersUpdated;
                _allianceService.OnApplicationsUpdated -= HandleApplicationsUpdated;
                _allianceService.OnLeftAlliance -= HandleLeftAlliance;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        #if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
        public void SetupComponentsForTest(
            GameObject noAllianceRoot,
            GameObject inAllianceRoot,
            TMP_InputField searchInput,
            Button btnSearch,
            Transform directoryContainer,
            Button btnOpenCreate,
            GameObject createPanel,
            TMP_InputField createNameInput,
            TMP_InputField createTagInput,
            TMP_InputField createNoticeInput,
            Button btnCreateSubmit,
            TextMeshProUGUI createErrorText,
            TextMeshProUGUI nameText,
            TextMeshProUGUI tagText,
            TextMeshProUGUI powerText,
            TextMeshProUGUI membersText,
            TextMeshProUGUI leaderText,
            Button btnTabRoster,
            Button btnTabNotice,
            Button btnTabApps,
            Transform rosterContainer,
            Transform appsContainer,
            Button btnLeave,
            AllianceDetailDrawer drawer)
        {
            _noAllianceRoot = noAllianceRoot;
            _inAllianceRoot = inAllianceRoot;
            _searchInput = searchInput;
            _btnSearch = btnSearch;
            _directoryContainer = directoryContainer;
            _btnOpenCreatePanel = btnOpenCreate;
            _createPanel = createPanel;
            _createNameInput = createNameInput;
            _createTagInput = createTagInput;
            _createNoticeInput = createNoticeInput;
            _btnCreateSubmit = btnCreateSubmit;
            _createErrorText = createErrorText;

            _allianceNameText = nameText;
            _allianceTagText = tagText;
            _alliancePowerText = powerText;
            _allianceMembersText = membersText;
            _allianceLeaderText = leaderText;

            _btnTabRoster = btnTabRoster;
            _btnTabNotice = btnTabNotice;
            _btnTabApps = btnTabApps;
            _rosterContainer = rosterContainer;
            _appsContainer = appsContainer;
            _btnLeaveAlliance = btnLeave;
            _detailDrawer = drawer;

            BindLocalButtons();
        }
        #endif
    }
}
