using System;
using System.Collections;
using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Infrastructure.Army;
using KingWorldWar.Infrastructure.Base;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.UI;
using KingWorldWar.UI.Army;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class ArmyRosterAndTrainingUITests
    {
        private GameObject _holderGo;
        private ArmyScreenView _screenView;
        private UnitDetailView _detailView;
        private TrainingQueueItemView _queueItemView;

        private ArmyConfigSO _armyConfig;
        private LocalMockArmyRepository _armyRepo;
        private ArmyService _armyService;
        private LocalizationService _locService;
        private ModalService _modalService;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _holderGo = new GameObject("Test_ArmyRosterAndTrainingUI");

            // Config & Services
            _armyConfig = ScriptableObject.CreateInstance<ArmyConfigSO>();
            _armyConfig.InitializeFromDefaults();

            _armyRepo = new LocalMockArmyRepository(_armyConfig)
            {
                SimulatedLatencyMs = 0
            };

            _locService = new LocalizationService(GameLog.Default);
            _locService.SetLanguage(Language.English, persist: false);
            _modalService = new ModalService(GameLog.Default);

            _armyService = new ArmyService(_armyConfig, _armyRepo, null, null, GameLog.Default);

            // Screen View
            var screenGo = new GameObject("ScreenView");
            screenGo.transform.SetParent(_holderGo.transform);
            _screenView = screenGo.AddComponent<ArmyScreenView>();

            var titleTxt = CreateTmp("Title", screenGo.transform);
            var pwrVal = CreateTmp("PwrVal", screenGo.transform);
            var trpsVal = CreateTmp("TrpsVal", screenGo.transform);
            var qVal = CreateTmp("QVal", screenGo.transform);

            var tankCard = CreateCard("Card_Tank", screenGo.transform, UnitType.Tank);
            var avCard = CreateCard("Card_AV", screenGo.transform, UnitType.ArmoredVehicle);
            var artCard = CreateCard("Card_Art", screenGo.transform, UnitType.Artillery);
            var figCard = CreateCard("Card_Fig", screenGo.transform, UnitType.FighterAircraft);
            var recAirCard = CreateCard("Card_RecAir", screenGo.transform, UnitType.ReconAircraft);
            var infCard = CreateCard("Card_Inf", screenGo.transform, UnitType.Infantry);
            var recCard = CreateCard("Card_Rec", screenGo.transform, UnitType.ReconVehicle);

            // Detail View
            var detailGo = new GameObject("DetailView");
            detailGo.transform.SetParent(_holderGo.transform);
            _detailView = detailGo.AddComponent<UnitDetailView>();

            var detailPanel = new GameObject("DetailPanel");
            detailPanel.transform.SetParent(detailGo.transform);

            var dName = CreateTmp("DName", detailPanel.transform);
            var dCat = CreateTmp("DCat", detailPanel.transform);
            var dRole = CreateTmp("DRole", detailPanel.transform);
            var dHp = CreateTmp("DHp", detailPanel.transform);
            var dPwr = CreateTmp("DPwr", detailPanel.transform);
            var dPerk = CreateTmp("DPerk", detailPanel.transform);

            var btnTrain = CreateBtn("BtnTrain", detailPanel.transform);
            var btnUpgrade = CreateBtn("BtnUpgrade", detailPanel.transform);
            var btnClose = CreateBtn("BtnClose", detailPanel.transform);

            var stageAnchor = new GameObject("PreviewStageAnchor").transform;
            stageAnchor.SetParent(detailPanel.transform);

            _detailView.SetupForTest(
                detailPanel, dName, dCat, dRole, dHp, dPwr, dPerk,
                btnTrain, btnUpgrade, btnClose, stageAnchor);
            _detailView.Initialize(_armyService, _locService);

            // Setup Screen View
            _screenView.SetupComponentsForTest(
                titleTxt, pwrVal, trpsVal, qVal,
                null,
                infCard, recCard, tankCard, artCard,
                avCard, figCard, recAirCard,
                _detailView);
            _screenView.Initialize(_armyService, _locService, null, _detailView);

            // Training Queue Item View
            var qItemGo = new GameObject("QueueItemView");
            qItemGo.transform.SetParent(_holderGo.transform);
            _queueItemView = qItemGo.AddComponent<TrainingQueueItemView>();

            var qName = CreateTmp("QName", qItemGo.transform);
            var qQty = CreateTmp("QQty", qItemGo.transform);
            var qBadge = CreateTmp("QBadge", qItemGo.transform);
            var qCd = CreateTmp("QCd", qItemGo.transform);
            var qCancelBtn = CreateBtn("QCancel", qItemGo.transform);

            _queueItemView.SetupForTest(qName, qQty, qBadge, qCd, qCancelBtn);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_holderGo != null) UnityEngine.Object.DestroyImmediate(_holderGo);
            if (_armyConfig != null) UnityEngine.Object.DestroyImmediate(_armyConfig);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ArmyScreenView_CategoryFilter_HidesUnmatchedUnits()
        {
            // Initial filter is null (All)
            _screenView.FilterCategory(null);
            yield return null;

            Assert.That(_screenView.TankCard.Root.activeSelf, Is.True);
            Assert.That(_screenView.FighterAircraftCard.Root.activeSelf, Is.True);

            // Filter Ground only
            _screenView.FilterCategory(UnitCategory.Ground);
            yield return null;

            Assert.That(_screenView.TankCard.Root.activeSelf, Is.True);
            Assert.That(_screenView.ArmoredVehicleCard.Root.activeSelf, Is.True);
            Assert.That(_screenView.ArtilleryCard.Root.activeSelf, Is.True);
            Assert.That(_screenView.FighterAircraftCard.Root.activeSelf, Is.False);
            Assert.That(_screenView.ReconAircraftCard.Root.activeSelf, Is.False);

            // Filter Air only
            _screenView.FilterCategory(UnitCategory.Air);
            yield return null;

            Assert.That(_screenView.TankCard.Root.activeSelf, Is.False);
            Assert.That(_screenView.FighterAircraftCard.Root.activeSelf, Is.True);
            Assert.That(_screenView.ReconAircraftCard.Root.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator ArmyScreenView_StatusBreakdown_DisplaysAuthoritativeCounts()
        {
            _armyRepo.SetUnitCountForTest(UnitType.Tank, owned: 25, inTraining: 5, deployed: 10);
            var task = _armyService.RefreshRosterAsync();
            while (!task.IsCompleted) yield return null;

            _screenView.Refresh();
            yield return null;

            var breakdown = _screenView.TankCard.StatusBreakdownText.text;
            Assert.That(breakdown.Contains("10"), Is.True, "Available count should be 25 - 5 - 10 = 10");
            Assert.That(breakdown.Contains("5"), Is.True, "InTraining count should be 5");
            Assert.That(breakdown.Contains("10"), Is.True, "Deployed count should be 10");
        }

        [UnityTest]
        public IEnumerator UnitDetailView_OpenAndRotate_PresentsSpecsAndPerks()
        {
            var def = _armyConfig.GetDefinition(UnitType.Tank);
            _detailView.Open(def);
            yield return null;

            Assert.That(_detailView.IsVisible, Is.True);
            Assert.That(_detailView.UnitNameText.text, Is.EqualTo("Main Battle Tank"));
            Assert.That(_detailView.HpStatText.text.Contains("850"), Is.True);
            Assert.That(_detailView.NextLevelPerksText.text.Contains("Frontal Armor"), Is.True);

            // Check rotating preview stage rotation over frames
            float initialY = _detailView.PreviewStageAnchor.eulerAngles.y;
            yield return new WaitForSeconds(0.1f);
            float rotatedY = _detailView.PreviewStageAnchor.eulerAngles.y;
            Assert.That(rotatedY, Is.Not.EqualTo(initialY));

            _detailView.Close();
            yield return null;
            Assert.That(_detailView.IsVisible, Is.False);
        }

        [UnityTest]
        public IEnumerator TrainingQueueItemView_InProgressState_DisplaysCountdown()
        {
            var now = DateTime.UtcNow;
            var item = new RecruitmentQueueItem(
                "q_item_test",
                UnitType.Tank,
                3,
                now,
                now.AddSeconds(60),
                new UnitCost(100, 100, 100, 60),
                TrainingQueueState.InProgress);

            _queueItemView.Initialize(item, _armyService, _locService, _modalService);
            yield return null;

            Assert.That(_queueItemView.QuantityText.text, Is.EqualTo("x3"));
            Assert.That(_queueItemView.StateBadgeText.text, Is.EqualTo("In Progress"));
            Assert.That(_queueItemView.CountdownText.text == "01:00" || _queueItemView.CountdownText.text == "00:59", Is.True);
            Assert.That(_queueItemView.CancelButton.gameObject.activeSelf, Is.True);
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
            return go.AddComponent<Button>();
        }

        private ArmyScreenView.UnitCardUI CreateCard(string name, Transform parent, UnitType type)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var recruitBtn = CreateBtn("RecruitBtn", go.transform);
            var inspectBtn = CreateBtn("InspectBtn", go.transform);

            return new ArmyScreenView.UnitCardUI
            {
                Root = go,
                Type = type,
                NameText = CreateTmp("Name", go.transform),
                RoleText = CreateTmp("Role", go.transform),
                CountText = CreateTmp("Count", go.transform),
                PowerText = CreateTmp("Power", go.transform),
                PrereqText = CreateTmp("Prereq", go.transform),
                StatusBreakdownText = CreateTmp("StatusBreakdown", go.transform),
                RecruitButton = recruitBtn,
                RecruitButtonText = CreateTmp("RecruitText", recruitBtn.transform),
                InspectButton = inspectBtn,
                TrainingBadge = new GameObject("Badge"),
                TrainingCountdown = CreateTmp("Cd", go.transform)
            };
        }
    }
}
