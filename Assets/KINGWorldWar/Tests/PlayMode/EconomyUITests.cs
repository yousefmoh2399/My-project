using System;
using System.Collections;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Economy;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Infrastructure.Base;
using KingWorldWar.Infrastructure.Economy;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.UI.Base;
using KingWorldWar.UI.Common;
using KingWorldWar.UI.Economy;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class EconomyUITests
    {
        private GameObject _canvasGo;
        private Canvas _canvas;
        private ILocalizationService _locService;
        private LocalMockEconomyRepository _repo;
        private EconomyService _service;

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

            _repo = new LocalMockEconomyRepository(
                initialGold: 125000,
                initialOil: 48000,
                initialIron: 89000,
                storageCaps: new StorageCapacity(200000, 200000, 200000),
                productionRates: new ResourceYieldRate(3600, 1800, 900))
            {
                SimulatedLatencyMs = 0
            };

            _service = new EconomyService(_repo);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasGo != null)
            {
                UnityEngine.Object.Destroy(_canvasGo);
            }
        }

        [UnityTest]
        public IEnumerator ResourceHeaderView_BindsEconomyService_DisplaysFormattedAmounts()
        {
            var headerGo = new GameObject("ResourceHeader");
            headerGo.transform.SetParent(_canvasGo.transform, false);
            var headerView = headerGo.AddComponent<ResourceHeaderView>();

            var gText = CreateTmp("GoldText", headerGo.transform);
            var oText = CreateTmp("OilText", headerGo.transform);
            var iText = CreateTmp("IronText", headerGo.transform);

            headerView.SetupComponentsForTest(gText, oText, iText);

            // Fetch snapshot
            var task = _service.RefreshSnapshotAsync();
            while (!task.IsCompleted) yield return null;

            headerView.BindEconomyService(_service);
            yield return null;

            // 125,000 -> 125K, 48,000 -> 48K, 89,000 -> 89K
            Assert.AreEqual("125K", gText.text);
            Assert.AreEqual("48K", oText.text);
            Assert.AreEqual("89K", iText.text);
        }

        [UnityTest]
        public IEnumerator ResourceHeaderView_NearCap_AppliesWarningColor()
        {
            // 195,000 gold out of 200,000 capacity (> 90%)
            var nearCapRepo = new LocalMockEconomyRepository(
                initialGold: 195000,
                initialOil: 10000,
                initialIron: 10000,
                storageCaps: new StorageCapacity(200000, 200000, 200000),
                productionRates: new ResourceYieldRate(0, 0, 0))
            {
                SimulatedLatencyMs = 0
            };
            var nearCapService = new EconomyService(nearCapRepo);
            var t = nearCapService.RefreshSnapshotAsync();
            while (!t.IsCompleted) yield return null;

            var headerGo = new GameObject("ResourceHeader");
            headerGo.transform.SetParent(_canvasGo.transform, false);
            var headerView = headerGo.AddComponent<ResourceHeaderView>();

            var gText = CreateTmp("GoldText", headerGo.transform);
            var oText = CreateTmp("OilText", headerGo.transform);
            var iText = CreateTmp("IronText", headerGo.transform);
            headerView.SetupComponentsForTest(gText, oText, iText);

            headerView.BindEconomyService(nearCapService);
            yield return null;

            // Yellow warning color for NearCap
            Assert.Greater(gText.color.r, 0.8f);
            Assert.Greater(gText.color.g, 0.6f);
            Assert.Less(gText.color.b, 0.3f);
        }

        [UnityTest]
        public IEnumerator EconomyDetailDrawer_OpenAndClose_TogglesVisibility()
        {
            var drawerGo = new GameObject("EconomyDrawer");
            drawerGo.transform.SetParent(_canvasGo.transform, false);
            var drawer = drawerGo.AddComponent<EconomyDetailDrawer>();

            var rootPanel = new GameObject("RootPanel");
            rootPanel.transform.SetParent(drawerGo.transform, false);

            var title = CreateTmp("Title", rootPanel.transform);
            var btnClose = CreateButton("BtnClose", rootPanel.transform);
            var btnRefresh = CreateButton("BtnRefresh", rootPanel.transform);
            var gBal = CreateTmp("GoldBal", rootPanel.transform);
            var oBal = CreateTmp("OilBal", rootPanel.transform);
            var iBal = CreateTmp("IronBal", rootPanel.transform);

            drawer.SetupComponentsForTest(rootPanel, title, btnClose, btnRefresh, gBal, oBal, iBal);

            var t = _service.RefreshSnapshotAsync();
            while (!t.IsCompleted) yield return null;

            drawer.Bind(_service, _locService);
            drawer.Close();
            yield return null;
            Assert.IsFalse(drawer.IsVisible);

            drawer.Open();
            yield return null;
            Assert.IsTrue(drawer.IsVisible);
            Assert.AreEqual("125,000", gBal.text);

            drawer.BtnClose.onClick.Invoke();
            yield return null;
            Assert.IsFalse(drawer.IsVisible);
        }

        [UnityTest]
        public IEnumerator BuildingDetailDrawer_WithEconomyService_ChecksAffordability()
        {
            // Low balance economy (e.g. 100 gold, 100 oil, 100 iron)
            var poorRepo = new LocalMockEconomyRepository(
                initialGold: 100,
                initialOil: 100,
                initialIron: 100,
                storageCaps: new StorageCapacity(50000, 50000, 50000),
                productionRates: new ResourceYieldRate(0, 0, 0))
            {
                SimulatedLatencyMs = 0
            };
            var poorService = new EconomyService(poorRepo);
            var t = poorService.RefreshSnapshotAsync();
            while (!t.IsCompleted) yield return null;

            var baseConfig = ScriptableObject.CreateInstance<BaseConfigSO>();
            baseConfig.InitializeFromDefaults();
            var baseRepo = new InMemoryBaseStateRepository(baseConfig);
            var baseService = new PlayerBaseService(baseConfig, baseRepo);

            var drawerGo = new GameObject("BuildingDrawer");
            drawerGo.transform.SetParent(_canvasGo.transform, false);
            var drawer = drawerGo.AddComponent<BuildingDetailDrawer>();

            var panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(drawerGo.transform, false);

            var nameText = CreateTmp("Name", panelRoot.transform);
            var levelBadge = CreateTmp("Level", panelRoot.transform);
            var descText = CreateTmp("Desc", panelRoot.transform);
            var curPerk = CreateTmp("CurPerk", panelRoot.transform);
            var nextPerk = CreateTmp("NextPerk", panelRoot.transform);
            var reqRoot = new GameObject("ReqRoot");
            reqRoot.transform.SetParent(panelRoot.transform, false);
            var costG = CreateTmp("CostG", reqRoot.transform);
            var costO = CreateTmp("CostO", reqRoot.transform);
            var costI = CreateTmp("CostI", reqRoot.transform);
            var dur = CreateTmp("Dur", reqRoot.transform);
            var prereqHQ = CreateTmp("PrereqHQ", reqRoot.transform);
            var btnUpgrade = CreateButton("BtnUpgrade", panelRoot.transform);
            var btnUpgradeText = CreateTmp("BtnUpgradeText", btnUpgrade.transform);
            var btnCancel = CreateButton("BtnCancel", panelRoot.transform);
            var btnClose = CreateButton("BtnClose", panelRoot.transform);
            var upgRoot = new GameObject("UpgRoot");
            upgRoot.transform.SetParent(panelRoot.transform, false);
            var countdown = CreateTmp("Countdown", upgRoot.transform);
            var progressFill = panelRoot.AddComponent<Image>();

            drawer.SetupComponentsForTest(
                panelRoot, nameText, levelBadge, descText, curPerk, nextPerk,
                reqRoot, costG, costO, costI, dur, prereqHQ,
                btnUpgrade, btnUpgradeText, btnCancel, btnClose,
                upgRoot, countdown, progressFill);

            drawer.Bind(baseService, _locService, null, poorService);

            // Select HQ (Level 1 requires 500 gold, player only has 100)
            drawer.DisplayBuilding(new BuildingId("bld_hq"));
            yield return null;

            // Verified: Cannot afford -> Upgrade button is disabled!
            Assert.IsFalse(btnUpgrade.interactable, "Upgrade button must be disabled when player cannot afford resource cost");
            // Cost text is colored red
            Assert.Greater(costG.color.r, 0.8f);
            Assert.Less(costG.color.g, 0.4f);
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
