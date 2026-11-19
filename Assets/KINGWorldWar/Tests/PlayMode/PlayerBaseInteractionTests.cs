using System.Collections;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Features.Base;
using KingWorldWar.Features.Camera;
using KingWorldWar.Infrastructure.Base;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.UI.Base;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class PlayerBaseInteractionTests
    {
        private GameObject _holderGo;
        private UnityEngine.Camera _camera;
        private StrategyCameraRig _cameraRig;
        private PlayerBaseView _baseView;
        private BuildingDetailDrawer _drawer;

        private BaseConfigSO _dataSource;
        private InMemoryBaseStateRepository _stateRepo;
        private PlayerBaseService _baseService;
        private LocalizationService _locService;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _holderGo = new GameObject("Test_PlayerBaseInteraction");

            // Setup camera
            var camGo = new GameObject("Test_Camera");
            camGo.transform.SetParent(_holderGo.transform);
            camGo.transform.position = new Vector3(240, 60, -40);
            camGo.transform.rotation = Quaternion.Euler(45, 0, 0);
            _camera = camGo.AddComponent<UnityEngine.Camera>();
            _camera.tag = "MainCamera";

            var rigGo = new GameObject("Test_CameraRig");
            rigGo.transform.SetParent(_holderGo.transform);
            _cameraRig = rigGo.AddComponent<StrategyCameraRig>();
            _cameraRig.Config = CameraConfigSO.CreateDefaultInstance();

            // Setup Base View
            var baseGo = new GameObject("Test_PlayerBaseView");
            baseGo.transform.SetParent(_holderGo.transform);
            _baseView = baseGo.AddComponent<PlayerBaseView>();

            // Setup BuildingDetailDrawer
            var drawerGo = new GameObject("Test_BuildingDetailDrawer");
            drawerGo.transform.SetParent(_holderGo.transform);
            _drawer = drawerGo.AddComponent<BuildingDetailDrawer>();

            var panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(drawerGo.transform);

            var bName = CreateTmp("BuildingName", panelRoot.transform);
            var lBadge = CreateTmp("LevelBadge", panelRoot.transform);
            var bDesc = CreateTmp("Description", panelRoot.transform);
            var cPerk = CreateTmp("CurrentPerk", panelRoot.transform);
            var nPerk = CreateTmp("NextPerk", panelRoot.transform);

            var reqRoot = new GameObject("RequirementsRoot");
            reqRoot.transform.SetParent(panelRoot.transform);

            var cGold = CreateTmp("CostGold", reqRoot.transform);
            var cOil = CreateTmp("CostOil", reqRoot.transform);
            var cIron = CreateTmp("CostIron", reqRoot.transform);
            var cDuration = CreateTmp("CostDuration", reqRoot.transform);
            var reqHq = CreateTmp("ReqHQ", reqRoot.transform);

            var btnUpgrade = CreateButton("Btn_Upgrade", panelRoot.transform);
            var btnUpgradeTxt = CreateTmp("BtnUpgradeTxt", btnUpgrade.transform);
            var btnCancel = CreateButton("Btn_Cancel", panelRoot.transform);
            var btnClose = CreateButton("Btn_Close", panelRoot.transform);

            var upgRoot = new GameObject("UpgradingRoot");
            upgRoot.transform.SetParent(panelRoot.transform);
            var cdTmp = CreateTmp("CountdownText", upgRoot.transform);

            var pbGo = new GameObject("ProgressBarFill");
            pbGo.transform.SetParent(upgRoot.transform);
            var pbImg = pbGo.AddComponent<Image>();

            _drawer.SetupComponentsForTest(
                panelRoot, bName, lBadge, bDesc, cPerk, nPerk,
                reqRoot, cGold, cOil, cIron, cDuration, reqHq,
                btnUpgrade, btnUpgradeTxt, btnCancel, btnClose,
                upgRoot, cdTmp, pbImg
            );

            // Data & Services
            _dataSource = ScriptableObject.CreateInstance<BaseConfigSO>();
            _dataSource.InitializeFromDefaults();
            _stateRepo = new InMemoryBaseStateRepository(_dataSource);
            _locService = new LocalizationService(GameLog.Default);
            _locService.SetLanguage(Language.English, persist: false);
            _baseService = new PlayerBaseService(_dataSource, _stateRepo, GameLog.Default);

            _baseView.Bind(_baseService, _locService, _cameraRig, _camera);
            _baseView.SetWorldCamera(_camera);
            _drawer.Bind(_baseService, _locService);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_holderGo != null)
            {
                Object.Destroy(_holderGo);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerBaseView_SpawnsAllFiveBuildings()
        {
            Assert.AreEqual(5, _baseView.BuildingViews.Count, "Must spawn all 5 initial base buildings.");

            Assert.IsTrue(_baseView.BuildingViews.ContainsKey(new BuildingId("bld_hq")));
            Assert.IsTrue(_baseView.BuildingViews.ContainsKey(new BuildingId("bld_barracks_01")));
            Assert.IsTrue(_baseView.BuildingViews.ContainsKey(new BuildingId("bld_depot_01")));
            Assert.IsTrue(_baseView.BuildingViews.ContainsKey(new BuildingId("bld_radar_01")));
            Assert.IsTrue(_baseView.BuildingViews.ContainsKey(new BuildingId("bld_factory_01")));

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerBaseView_SelectBuilding_UpdatesSelectedView()
        {
            var hqId = new BuildingId("bld_hq");
            _baseView.SelectBuilding(hqId);

            yield return null;

            Assert.IsNotNull(_baseView.SelectedView);
            Assert.AreEqual(hqId, _baseView.SelectedView.BuildingId);
            Assert.IsTrue(_baseView.SelectedView.IsSelected);
            Assert.IsTrue(_drawer.IsVisible);
            Assert.AreEqual(hqId, _drawer.CurrentBuildingId);
        }

        [UnityTest]
        public IEnumerator PlayerBaseView_Deselect_ClearsSelectedView()
        {
            _baseView.SelectBuilding(new BuildingId("bld_hq"));
            yield return null;
            Assert.IsNotNull(_baseView.SelectedView);

            _baseView.Deselect();
            yield return null;

            Assert.IsNull(_baseView.SelectedView);
            Assert.IsFalse(_drawer.IsVisible);
        }

        [UnityTest]
        public IEnumerator PlayerBaseView_RaycastSelection_SelectsClickedBuilding()
        {
            var hqView = _baseView.BuildingViews[new BuildingId("bld_hq")];
            Vector3 screenPos = _camera.WorldToScreenPoint(hqView.CenterPosition);

            _baseView.PerformRaycastSelection(new Vector2(screenPos.x, screenPos.y));

            yield return null;

            Assert.IsNotNull(_baseView.SelectedView);
            Assert.AreEqual(new BuildingId("bld_hq"), _baseView.SelectedView.BuildingId);
        }

        [UnityTest]
        public IEnumerator PlayerBaseView_RaycastSelection_EmptySpaceDeselects()
        {
            _baseView.SelectBuilding(new BuildingId("bld_hq"));
            yield return null;
            Assert.IsNotNull(_baseView.SelectedView);

            // Raycast into top-left empty corner
            _baseView.PerformRaycastSelection(new Vector2(10f, Screen.height - 10f));

            yield return null;

            Assert.IsNull(_baseView.SelectedView);
            Assert.IsFalse(_drawer.IsVisible);
        }

        [UnityTest]
        public IEnumerator BuildingDetailDrawer_DisplaysCorrectDetails()
        {
            var hqId = new BuildingId("bld_hq");
            _baseView.SelectBuilding(hqId);

            yield return null;

            Assert.IsTrue(_drawer.IsVisible);
            Assert.AreEqual("Command Center", _drawer.BuildingNameText.text);
            Assert.AreEqual("Level 1", _drawer.LevelBadgeText.text);
            Assert.AreEqual("500", _drawer.CostGoldText.text);
            Assert.AreEqual("200", _drawer.CostOilText.text);
            Assert.AreEqual("100", _drawer.CostIronText.text);
        }

        [UnityTest]
        public IEnumerator BuildingDetailDrawer_UpgradeButton_InitiatesUpgrade()
        {
            var hqId = new BuildingId("bld_hq");
            _baseView.SelectBuilding(hqId);

            yield return null;

            Assert.IsTrue(_drawer.BtnUpgrade.gameObject.activeSelf);
            Assert.IsFalse(_drawer.BtnCancelUpgrade.gameObject.activeSelf);

            _drawer.BtnUpgrade.onClick.Invoke();

            yield return new WaitForSeconds(0.1f);

            var hqState = _stateRepo.GetCommandCenter();
            Assert.IsTrue(hqState.IsUpgrading);
            Assert.IsTrue(_drawer.UpgradingStatusRoot.activeSelf);
            Assert.IsFalse(_drawer.BtnUpgrade.gameObject.activeSelf);
            Assert.IsTrue(_drawer.BtnCancelUpgrade.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator BuildingDetailDrawer_CloseButton_DeselectsAndHides()
        {
            _baseView.SelectBuilding(new BuildingId("bld_hq"));
            yield return null;
            Assert.IsTrue(_drawer.IsVisible);

            _drawer.BtnClose.onClick.Invoke();
            yield return null;

            Assert.IsFalse(_drawer.IsVisible);
            Assert.IsNull(_baseView.SelectedView);
        }

        [UnityTest]
        public IEnumerator BuildingDetailDrawer_LanguageChange_UpdatesLocalizationInRealtime()
        {
            _baseView.SelectBuilding(new BuildingId("bld_hq"));
            yield return null;

            Assert.AreEqual("Command Center", _drawer.BuildingNameText.text);

            // Switch to Arabic
            _locService.SetLanguage(Language.Arabic, persist: false);
            yield return null;

            Assert.AreEqual(ArabicTextShaper.Shape("مقر القيادة"), _drawer.BuildingNameText.text);
            Assert.IsTrue(ArabicTextShaper.ContainsArabic(_drawer.BuildingNameText.text));
        }

        private static TextMeshProUGUI CreateTmp(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<TextMeshProUGUI>();
        }

        private static Button CreateButton(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>();
            return go.AddComponent<Button>();
        }
    }
}
