using System;
using System.Collections;
using System.Collections.Generic;
using KingWorldWar.Core.Alliance;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;
using KingWorldWar.Infrastructure.Alliance;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.UI;
using KingWorldWar.UI.Alliance;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class AllianceUITests
    {
        private GameObject _canvasGo;
        private Canvas _canvas;
        private ILocalizationService _locService;
        private LocalMockAllianceRepository _repo;
        private AllianceService _service;
        private IModalService _modalService;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("TestCanvas");
            _canvas = _canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvasGo.AddComponent<CanvasScaler>();
            _canvasGo.AddComponent<GraphicRaycaster>();

            _locService = new LocalizationService(GameLog.Default);
            _locService.SetLanguage(Language.English, persist: false);

            _modalService = new ModalService(GameLog.Default);

            _repo = new LocalMockAllianceRepository();
            _repo.SimulatedLatencyMs = 0;
            _repo.CurrentPlayerId = "usr_tester_01";
            _repo.CurrentPlayerName = "Test Commander";

            _service = new AllianceService(_repo, null, GameLog.Default);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasGo != null)
            {
                UnityEngine.Object.Destroy(_canvasGo);
            }
        }

        private TextMeshProUGUI CreateTmp(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = "";
            return tmp;
        }

        private Button CreateButton(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(Image));
            go.transform.SetParent(parent, false);
            return go.GetComponent<Button>();
        }

        private TMP_InputField CreateInputField(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            return go.GetComponent<TMP_InputField>();
        }

        [UnityTest]
        public IEnumerator AllianceScreenView_WhenNotInAlliance_ShowsNoAllianceRootAndPopulatesDirectory()
        {
            var screenGo = new GameObject("AllianceScreenView");
            screenGo.transform.SetParent(_canvasGo.transform, false);
            var screenView = screenGo.AddComponent<AllianceScreenView>();

            var noAllianceRoot = new GameObject("NoAllianceRoot");
            noAllianceRoot.transform.SetParent(screenGo.transform, false);

            var inAllianceRoot = new GameObject("InAllianceRoot");
            inAllianceRoot.transform.SetParent(screenGo.transform, false);

            var dirContainer = new GameObject("DirectoryContainer");
            dirContainer.transform.SetParent(noAllianceRoot.transform, false);

            var searchInput = CreateInputField("SearchInput", noAllianceRoot.transform);
            var btnSearch = CreateButton("BtnSearch", noAllianceRoot.transform);
            var btnOpenCreate = CreateButton("BtnOpenCreate", noAllianceRoot.transform);
            var createPanel = new GameObject("CreatePanel");
            createPanel.transform.SetParent(noAllianceRoot.transform, false);
            var createNameInput = CreateInputField("CreateName", createPanel.transform);
            var createTagInput = CreateInputField("CreateTag", createPanel.transform);
            var createNoticeInput = CreateInputField("CreateNotice", createPanel.transform);
            var btnCreateSubmit = CreateButton("BtnCreateSubmit", createPanel.transform);
            var createErrorText = CreateTmp("CreateErrorText", createPanel.transform);

            var nameText = CreateTmp("NameText", inAllianceRoot.transform);
            var tagText = CreateTmp("TagText", inAllianceRoot.transform);
            var powerText = CreateTmp("PowerText", inAllianceRoot.transform);
            var membersText = CreateTmp("MembersText", inAllianceRoot.transform);
            var leaderText = CreateTmp("LeaderText", inAllianceRoot.transform);

            var btnTabRoster = CreateButton("BtnTabRoster", inAllianceRoot.transform);
            var btnTabNotice = CreateButton("BtnTabNotice", inAllianceRoot.transform);
            var btnTabApps = CreateButton("BtnTabApps", inAllianceRoot.transform);

            var rosterContainer = new GameObject("RosterContainer");
            rosterContainer.transform.SetParent(inAllianceRoot.transform, false);

            var appsContainer = new GameObject("AppsContainer");
            appsContainer.transform.SetParent(inAllianceRoot.transform, false);

            var btnLeave = CreateButton("BtnLeave", inAllianceRoot.transform);

            screenView.SetupComponentsForTest(
                noAllianceRoot,
                inAllianceRoot,
                searchInput,
                btnSearch,
                dirContainer.transform,
                btnOpenCreate,
                createPanel,
                createNameInput,
                createTagInput,
                createNoticeInput,
                btnCreateSubmit,
                createErrorText,
                nameText,
                tagText,
                powerText,
                membersText,
                leaderText,
                btnTabRoster,
                btnTabNotice,
                btnTabApps,
                rosterContainer.transform,
                appsContainer.transform,
                btnLeave,
                null
            );

            screenView.Initialize(_service, _locService, _modalService);
            yield return null;

            Assert.IsTrue(noAllianceRoot.activeSelf, "NoAllianceRoot must be active when player is not in an alliance");
            Assert.IsFalse(inAllianceRoot.activeSelf, "InAllianceRoot must be inactive");

            // Wait for directory search to populate
            var searchTask = screenView.SearchAndPopulateDirectory(string.Empty);
            while (!searchTask.IsCompleted) yield return null;

            Assert.Greater(dirContainer.transform.childCount, 0, "Directory container must display seeded alliances");
        }

        [UnityTest]
        public IEnumerator AllianceScreenView_WhenInAlliance_ShowsInAllianceRootAndDisplaysProfile()
        {
            // Place player in King Vanguard
            _repo.SetPlayerAllianceForTest(new AllianceId("all_kww_001"), AllianceRole.Officer);

            var t = _service.RefreshCurrentAllianceAsync();
            while (!t.IsCompleted) yield return null;

            var screenGo = new GameObject("AllianceScreenView");
            screenGo.transform.SetParent(_canvasGo.transform, false);
            var screenView = screenGo.AddComponent<AllianceScreenView>();

            var noAllianceRoot = new GameObject("NoAllianceRoot");
            noAllianceRoot.transform.SetParent(screenGo.transform, false);

            var inAllianceRoot = new GameObject("InAllianceRoot");
            inAllianceRoot.transform.SetParent(screenGo.transform, false);

            var dirContainer = new GameObject("DirectoryContainer");
            dirContainer.transform.SetParent(noAllianceRoot.transform, false);

            var searchInput = CreateInputField("SearchInput", noAllianceRoot.transform);
            var btnSearch = CreateButton("BtnSearch", noAllianceRoot.transform);
            var btnOpenCreate = CreateButton("BtnOpenCreate", noAllianceRoot.transform);
            var createPanel = new GameObject("CreatePanel");
            var createNameInput = CreateInputField("CreateName", createPanel.transform);
            var createTagInput = CreateInputField("CreateTag", createPanel.transform);
            var createNoticeInput = CreateInputField("CreateNotice", createPanel.transform);
            var btnCreateSubmit = CreateButton("BtnCreateSubmit", createPanel.transform);
            var createErrorText = CreateTmp("CreateErrorText", createPanel.transform);

            var nameText = CreateTmp("NameText", inAllianceRoot.transform);
            var tagText = CreateTmp("TagText", inAllianceRoot.transform);
            var powerText = CreateTmp("PowerText", inAllianceRoot.transform);
            var membersText = CreateTmp("MembersText", inAllianceRoot.transform);
            var leaderText = CreateTmp("LeaderText", inAllianceRoot.transform);

            var btnTabRoster = CreateButton("BtnTabRoster", inAllianceRoot.transform);
            var btnTabNotice = CreateButton("BtnTabNotice", inAllianceRoot.transform);
            var btnTabApps = CreateButton("BtnTabApps", inAllianceRoot.transform);

            var rosterContainer = new GameObject("RosterContainer");
            rosterContainer.transform.SetParent(inAllianceRoot.transform, false);

            var appsContainer = new GameObject("AppsContainer");
            appsContainer.transform.SetParent(inAllianceRoot.transform, false);

            var btnLeave = CreateButton("BtnLeave", inAllianceRoot.transform);

            screenView.SetupComponentsForTest(
                noAllianceRoot,
                inAllianceRoot,
                searchInput,
                btnSearch,
                dirContainer.transform,
                btnOpenCreate,
                createPanel,
                createNameInput,
                createTagInput,
                createNoticeInput,
                btnCreateSubmit,
                createErrorText,
                nameText,
                tagText,
                powerText,
                membersText,
                leaderText,
                btnTabRoster,
                btnTabNotice,
                btnTabApps,
                rosterContainer.transform,
                appsContainer.transform,
                btnLeave,
                null
            );

            screenView.Initialize(_service, _locService, _modalService);
            yield return null;

            Assert.IsFalse(noAllianceRoot.activeSelf, "NoAllianceRoot must be inactive");
            Assert.IsTrue(inAllianceRoot.activeSelf, "InAllianceRoot must be active");
            Assert.AreEqual("King Vanguard", nameText.text);
            Assert.AreEqual("[KWW]", tagText.text);
        }

        [UnityTest]
        public IEnumerator AllianceDetailDrawer_OpenAndClose_RendersAllianceData()
        {
            var drawerGo = new GameObject("AllianceDetailDrawer");
            drawerGo.transform.SetParent(_canvasGo.transform, false);
            var drawer = drawerGo.AddComponent<AllianceDetailDrawer>();

            var title = CreateTmp("Title", drawerGo.transform);
            var nameText = CreateTmp("Name", drawerGo.transform);
            var tagText = CreateTmp("Tag", drawerGo.transform);
            var powerText = CreateTmp("Power", drawerGo.transform);
            var membersText = CreateTmp("Members", drawerGo.transform);
            var leaderText = CreateTmp("Leader", drawerGo.transform);
            var policyText = CreateTmp("Policy", drawerGo.transform);
            var noticeText = CreateTmp("Notice", drawerGo.transform);
            var btnAction = CreateButton("BtnAction", drawerGo.transform);
            var btnActionText = CreateTmp("BtnActionText", btnAction.transform);
            var btnClose = CreateButton("BtnClose", drawerGo.transform);

            drawer.SetupComponentsForTest(
                title, nameText, tagText, powerText, membersText, leaderText, policyText, noticeText,
                btnAction, btnActionText, btnClose
            );

            drawer.Bind(_locService);
            drawer.Close();
            yield return null;
            Assert.IsFalse(drawer.IsOpen);

            var profileTask = _repo.GetAllianceAsync(new AllianceId("all_kww_001"));
            while (!profileTask.IsCompleted) yield return null;
            var profile = profileTask.Result.Value;
            drawer.OpenForAlliance(profile, userHasAlliance: false);
            yield return null;

            Assert.IsTrue(drawer.IsOpen);
            Assert.AreEqual("King Vanguard", nameText.text);
            Assert.IsTrue(tagText.text.Contains("KWW"));
            Assert.IsTrue(btnAction.interactable);
            Assert.AreEqual("Join Coalition", btnActionText.text);

            drawer.BtnClose.onClick.Invoke();
            yield return null;
            Assert.IsFalse(drawer.IsOpen);
        }

        [UnityTest]
        public IEnumerator AllianceMemberRowView_EvaluatesViewerRolePermissions()
        {
            var rowGo = new GameObject("MemberRow");
            rowGo.transform.SetParent(_canvasGo.transform, false);
            var rowView = rowGo.AddComponent<AllianceMemberRowView>();

            var nameText = CreateTmp("Name", rowGo.transform);
            var roleText = CreateTmp("Role", rowGo.transform);
            var powerText = CreateTmp("Power", rowGo.transform);
            var contribText = CreateTmp("Contrib", rowGo.transform);
            var btnKick = CreateButton("BtnKick", rowGo.transform);
            var btnPromote = CreateButton("BtnPromote", rowGo.transform);
            var btnDemote = CreateButton("BtnDemote", rowGo.transform);

            rowView.SetupComponentsForTest(nameText, roleText, powerText, contribText, btnKick, btnPromote, btnDemote);

            var recruit = new AllianceMember("usr_rec_1", "Private John", AllianceRole.Recruit, 250000, 100, DateTime.UtcNow);

            // Officer viewing Recruit
            rowView.Bind(recruit, AllianceRole.Officer, _locService);
            yield return null;

            Assert.IsTrue(btnKick.gameObject.activeSelf, "Officer can kick Recruit");
            Assert.IsTrue(btnPromote.gameObject.activeSelf, "Officer can promote Recruit");
            Assert.IsFalse(btnDemote.gameObject.activeSelf, "Cannot demote Recruit (lowest rank)");

            // Member viewing Recruit
            rowView.Bind(recruit, AllianceRole.Member, _locService);
            yield return null;

            Assert.IsFalse(btnKick.gameObject.activeSelf, "Member cannot kick Recruit");
            Assert.IsFalse(btnPromote.gameObject.activeSelf, "Member cannot promote Recruit");
        }
    }
}
