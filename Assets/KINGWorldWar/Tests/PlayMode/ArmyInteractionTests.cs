using System.Collections;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;
using KingWorldWar.Features.Army;
using KingWorldWar.Features.Camera;
using KingWorldWar.Infrastructure.Army;
using KingWorldWar.Infrastructure.Base;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.UI;
using KingWorldWar.UI.Army;
using KingWorldWar.UI.Common;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class ArmyInteractionTests
    {
        private GameObject _holderGo;
        private UnityEngine.Camera _camera;
        private StrategyCameraRig _cameraRig;
        private PlayerArmyView _armyView;
        private RecruitmentDrawer _drawer;

        private ArmyConfigSO _armyConfig;
        private InMemoryArmyStateRepository _armyRepo;
        private BaseConfigSO _baseConfig;
        private InMemoryBaseStateRepository _baseRepo;
        private ArmyService _armyService;
        private LocalizationService _locService;
        private ModalService _modalService;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _holderGo = new GameObject("Test_ArmyInteraction");

            // Setup camera
            var camGo = new GameObject("Test_Camera");
            camGo.transform.SetParent(_holderGo.transform);
            camGo.transform.position = new Vector3(400, 50, -35);
            camGo.transform.rotation = Quaternion.Euler(45, 0, 0);
            _camera = camGo.AddComponent<UnityEngine.Camera>();
            _camera.tag = "MainCamera";

            var rigGo = new GameObject("Test_CameraRig");
            rigGo.transform.SetParent(_holderGo.transform);
            _cameraRig = rigGo.AddComponent<StrategyCameraRig>();
            var camCfg = CameraConfigSO.CreateDefaultInstance();
            camCfg.SetBounds(new Vector2(-100, -100), new Vector2(500, 100));
            _cameraRig.Config = camCfg;

            // Setup PlayerArmyView
            var armyGo = new GameObject("Test_PlayerArmyView");
            armyGo.transform.SetParent(_holderGo.transform);
            armyGo.transform.position = new Vector3(400, 0, 0);
            _armyView = armyGo.AddComponent<PlayerArmyView>();

            // Setup RecruitmentDrawer
            var drawerGo = new GameObject("Test_RecruitmentDrawer");
            drawerGo.transform.SetParent(_holderGo.transform);
            _drawer = drawerGo.AddComponent<RecruitmentDrawer>();

            var panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(drawerGo.transform);

            var nameTxt = CreateTmp("Name", panelRoot.transform);
            var roleTxt = CreateTmp("Role", panelRoot.transform);
            var descTxt = CreateTmp("Desc", panelRoot.transform);
            var hpTxt = CreateTmp("HP", panelRoot.transform);
            var atkTxt = CreateTmp("ATK", panelRoot.transform);
            var defTxt = CreateTmp("DEF", panelRoot.transform);
            var spdTxt = CreateTmp("SPD", panelRoot.transform);
            var rngTxt = CreateTmp("RNG", panelRoot.transform);
            var pwrTxt = CreateTmp("PWR", panelRoot.transform);

            var decBtn = CreateBtn("Dec", panelRoot.transform);
            var incBtn = CreateBtn("Inc", panelRoot.transform);
            var maxBtn = CreateBtn("Max", panelRoot.transform);
            var qtyTxt = CreateTmp("Qty", panelRoot.transform);

            var costsRoot = new GameObject("CostsRoot");
            costsRoot.transform.SetParent(panelRoot.transform);
            var goldTxt = CreateTmp("Gold", costsRoot.transform);
            var oilTxt = CreateTmp("Oil", costsRoot.transform);
            var ironTxt = CreateTmp("Iron", costsRoot.transform);
            var durTxt = CreateTmp("Duration", costsRoot.transform);
            var prereqTxt = CreateTmp("Prereq", costsRoot.transform);

            var recBtn = CreateBtn("Recruit", panelRoot.transform);
            var recBtnTxt = CreateTmp("RecruitText", recBtn.transform);
            var cancelBtn = CreateBtn("Cancel", panelRoot.transform);
            var closeBtn = CreateBtn("Close", panelRoot.transform);

            var trainRoot = new GameObject("TrainRoot");
            trainRoot.transform.SetParent(panelRoot.transform);
            var cdTxt = CreateTmp("Countdown", trainRoot.transform);

            var pbImgGo = new GameObject("ProgressBarFill");
            pbImgGo.transform.SetParent(trainRoot.transform);
            var pbImg = pbImgGo.AddComponent<Image>();

            _drawer.SetupComponentsForTest(
                panelRoot, nameTxt, roleTxt, descTxt,
                hpTxt, atkTxt, defTxt, spdTxt, rngTxt, pwrTxt,
                decBtn, incBtn, maxBtn, qtyTxt,
                costsRoot, goldTxt, oilTxt, ironTxt, durTxt, prereqTxt,
                recBtn, recBtnTxt, cancelBtn, closeBtn,
                trainRoot, cdTxt, pbImg);

            // Setup Data & Services
            _armyConfig = ScriptableObject.CreateInstance<ArmyConfigSO>();
            _armyConfig.InitializeFromDefaults();

            _armyRepo = new InMemoryArmyStateRepository();

            _baseConfig = ScriptableObject.CreateInstance<BaseConfigSO>();
            _baseConfig.InitializeFromDefaults();
            _baseRepo = new InMemoryBaseStateRepository(_baseConfig);

            _locService = new LocalizationService(GameLog.Default);
            _locService.SetLanguage(Language.English, persist: false);
            _modalService = new ModalService(GameLog.Default);
            _armyService = new ArmyService(_armyConfig, _armyRepo, _baseRepo, GameLog.Default);

            _armyView.Initialize(_armyService, _locService, _cameraRig, _camera);
            _drawer.Initialize(_armyService, _locService, _modalService);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_holderGo != null) Object.DestroyImmediate(_holderGo);
            if (_armyConfig != null) Object.DestroyImmediate(_armyConfig);
            if (_baseConfig != null) Object.DestroyImmediate(_baseConfig);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerArmyView_SpawnsAllFiveMvpArchetypes()
        {
            Assert.That(_armyView.UnitViews.Count, Is.EqualTo(5));
            Assert.That(_armyView.UnitViews.ContainsKey(UnitType.Tank), Is.True);
            Assert.That(_armyView.UnitViews.ContainsKey(UnitType.ArmoredVehicle), Is.True);
            Assert.That(_armyView.UnitViews.ContainsKey(UnitType.Artillery), Is.True);
            Assert.That(_armyView.UnitViews.ContainsKey(UnitType.FighterAircraft), Is.True);
            Assert.That(_armyView.UnitViews.ContainsKey(UnitType.ReconAircraft), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerArmyView_SelectUnit_UpdatesSelectionAndFocusesCamera()
        {
            ArmyUnitView selectedUnit = null;
            _armyView.OnUnitSelected += u => selectedUnit = u;

            _armyView.SelectUnit(UnitType.Tank);
            yield return null;

            Assert.That(_armyView.SelectedView, Is.Not.Null);
            Assert.That(_armyView.SelectedView.UnitType, Is.EqualTo(UnitType.Tank));
            Assert.That(_armyView.SelectedView.IsSelected, Is.True);
            Assert.That(selectedUnit, Is.EqualTo(_armyView.SelectedView));

            // Verify Camera rig target moved to Tank position
            var tankPos = _armyView.SelectedView.CenterPosition;
            Assert.That(Mathf.Abs(_cameraRig.TargetFocusPosition.x - tankPos.x), Is.LessThan(2.0f));
        }

        [UnityTest]
        public IEnumerator PlayerArmyView_Deselect_ClearsSelection()
        {
            _armyView.SelectUnit(UnitType.ArmoredVehicle);
            yield return null;
            Assert.That(_armyView.SelectedView, Is.Not.Null);

            _armyView.Deselect();
            yield return null;

            Assert.That(_armyView.SelectedView, Is.Null);
        }

        [UnityTest]
        public IEnumerator PlayerArmyView_RaycastSelectsClickedUnit()
        {
            var tankView = _armyView.UnitViews[UnitType.Tank];
            Vector3 screenPoint = _camera.WorldToScreenPoint(tankView.CenterPosition);

            _armyView.PerformRaycastSelection(new Vector2(screenPoint.x, screenPoint.y));
            yield return null;

            Assert.That(_armyView.SelectedView, Is.Not.Null);
            Assert.That(_armyView.SelectedView.UnitType, Is.EqualTo(UnitType.Tank));
        }

        [UnityTest]
        public IEnumerator PlayerArmyView_RaycastEmpty_Deselects()
        {
            _armyView.SelectUnit(UnitType.ArmoredVehicle);
            yield return null;
            Assert.That(_armyView.SelectedView, Is.Not.Null);

            // Raycast into empty corner
            _armyView.PerformRaycastSelection(new Vector2(10, 10));
            yield return null;

            Assert.That(_armyView.SelectedView, Is.Null);
        }

        [UnityTest]
        public IEnumerator RecruitmentDrawer_DisplaysCorrectUnitDetails()
        {
            _drawer.OpenForUnit(UnitType.Tank);
            yield return null;

            Assert.That(_drawer.IsVisible, Is.True);
            Assert.That(_drawer.CurrentType, Is.EqualTo(UnitType.Tank));
            Assert.That(_drawer.UnitNameText.text, Is.EqualTo("Main Battle Tank"));
            Assert.That(_drawer.RoleText.text, Is.EqualTo("Armored Strike"));
            Assert.That(_drawer.HpText.text, Is.EqualTo("850"));
            Assert.That(_drawer.AttackText.text, Is.EqualTo("160"));
            Assert.That(_drawer.DefenseText.text, Is.EqualTo("120"));
        }

        [UnityTest]
        public IEnumerator RecruitmentDrawer_QuantityStepper_ModifiesQuantityAndCost()
        {
            _drawer.OpenForUnit(UnitType.Tank);
            yield return null;

            Assert.That(_drawer.CurrentQuantity, Is.EqualTo(1));
            Assert.That(_drawer.CostGoldText.text, Is.EqualTo("350"));

            // Increment
            _drawer.BtnIncrement.onClick.Invoke();
            yield return null;

            Assert.That(_drawer.CurrentQuantity, Is.EqualTo(2));
            Assert.That(_drawer.CostGoldText.text, Is.EqualTo("700"));

            // Max
            _drawer.BtnMax.onClick.Invoke();
            yield return null;

            Assert.That(_drawer.CurrentQuantity, Is.EqualTo(20));
            Assert.That(_drawer.CostGoldText.text, Is.EqualTo("7,000"));

            // Decrement
            _drawer.BtnDecrement.onClick.Invoke();
            yield return null;

            Assert.That(_drawer.CurrentQuantity, Is.EqualTo(19));
        }

        [UnityTest]
        public IEnumerator RecruitmentDrawer_RecruitButton_InitiatesRecruitment()
        {
            _drawer.OpenForUnit(UnitType.Tank);
            yield return null;

            Assert.That(_drawer.BtnRecruit.interactable, Is.True);

            _drawer.BtnRecruit.onClick.Invoke();
            yield return null;

            // Active queue started
            Assert.That(_armyService.GetActiveQueues().Count, Is.EqualTo(1));
            Assert.That(_drawer.TrainingStatusRoot.activeSelf, Is.True);
            Assert.That(_drawer.BtnCancelRecruit.gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator RecruitmentDrawer_CloseButton_HidesDrawer()
        {
            _drawer.OpenForUnit(UnitType.ArmoredVehicle);
            yield return null;
            Assert.That(_drawer.IsVisible, Is.True);

            _drawer.BtnClose.onClick.Invoke();
            yield return null;

            Assert.That(_drawer.IsVisible, Is.False);
            Assert.That(_drawer.CurrentType, Is.Null);
        }

        [UnityTest]
        public IEnumerator RecruitmentDrawer_LanguageToggle_UpdatesTextInRealTime()
        {
            _drawer.OpenForUnit(UnitType.Tank);
            yield return null;
            Assert.That(_drawer.UnitNameText.text, Is.EqualTo("Main Battle Tank"));

            // Switch to Arabic
            _locService.SetLanguage(Language.Arabic);
            yield return null;

            string expectedArabic = ArabicTextShaper.Shape("دبابة قتال رئيسية");
            Assert.That(_drawer.UnitNameText.text, Is.EqualTo(expectedArabic));

            // Switch back to English
            _locService.SetLanguage(Language.English);
            yield return null;

            Assert.That(_drawer.UnitNameText.text, Is.EqualTo("Main Battle Tank"));
        }

        private TextMeshProUGUI CreateTmp(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<TextMeshProUGUI>();
        }

        private Button CreateBtn(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>();
            return go.AddComponent<Button>();
        }
    }
}
