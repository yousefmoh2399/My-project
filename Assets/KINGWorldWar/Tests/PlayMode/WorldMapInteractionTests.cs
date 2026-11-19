using System.Collections;
using System.Linq;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.WorldMap;
using KingWorldWar.Features.Camera;
using KingWorldWar.Features.WorldMap;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.WorldMap;
using KingWorldWar.UI.WorldMap;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KingWorldWar.Tests.PlayMode
{
    [TestFixture]
    public class WorldMapInteractionTests
    {
        private GameObject _holderGo;
        private UnityEngine.Camera _camera;
        private StrategyCameraRig _cameraRig;
        private WorldMapView _mapView;
        private TerritoryDetailDrawer _drawer;

        private WorldMapConfigSO _dataSource;
        private InMemoryTerritoryStateRepository _stateRepo;
        private WorldMapService _mapService;
        private LocalizationService _locService;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _holderGo = new GameObject("Test_WorldMapInteraction");

            // Setup camera
            var camGo = new GameObject("Test_Camera");
            camGo.transform.SetParent(_holderGo.transform);
            camGo.transform.position = new Vector3(0, 50, -50);
            camGo.transform.rotation = Quaternion.Euler(45, 0, 0);
            _camera = camGo.AddComponent<UnityEngine.Camera>();
            _camera.tag = "MainCamera";

            var rigGo = new GameObject("Test_CameraRig");
            rigGo.transform.SetParent(_holderGo.transform);
            _cameraRig = rigGo.AddComponent<StrategyCameraRig>();
            _cameraRig.Config = CameraConfigSO.CreateDefaultInstance();

            // Setup World Map View
            var mapGo = new GameObject("Test_WorldMapView");
            mapGo.transform.SetParent(_holderGo.transform);
            _mapView = mapGo.AddComponent<WorldMapView>();

            // Setup TerritoryDetailDrawer with required UI components
            var drawerGo = new GameObject("Test_TerritoryDetailDrawer");
            drawerGo.transform.SetParent(_holderGo.transform);
            _drawer = drawerGo.AddComponent<TerritoryDetailDrawer>();

            var panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(drawerGo.transform);

            var tName = CreateTmp("TerritoryName", panelRoot.transform);
            var rBadge = CreateTmp("RegionBadge", panelRoot.transform);
            var fName = CreateTmp("FactionName", panelRoot.transform);
            var fSymbol = CreateTmp("FactionSymbol", panelRoot.transform);
            var oPlayer = CreateTmp("OwnerPlayer", panelRoot.transform);
            var yGold = CreateTmp("YieldGold", panelRoot.transform);
            var yOil = CreateTmp("YieldOil", panelRoot.transform);
            var yIron = CreateTmp("YieldIron", panelRoot.transform);
            var gDefense = CreateTmp("GarrisonDefense", panelRoot.transform);
            var aFrontiers = CreateTmp("AdjacentFrontiers", panelRoot.transform);
            var noteTmp = CreateTmp("Phase9Note", panelRoot.transform);

            var btnAttack = CreateButton("Btn_Attack", panelRoot.transform);
            var btnScout = CreateButton("Btn_Scout", panelRoot.transform);
            var btnClose = CreateButton("Btn_Close", panelRoot.transform);

            var legendPanel = new GameObject("LegendPanelRoot");
            legendPanel.transform.SetParent(drawerGo.transform);
            var btnToggleLegend = CreateButton("Btn_ToggleLegend", panelRoot.transform);

            _drawer.SetupComponentsForTest(
                panelRoot, tName, rBadge, fName, fSymbol, oPlayer,
                yGold, yOil, yIron, gDefense, aFrontiers,
                btnAttack, btnScout, noteTmp, btnClose,
                legendPanel, btnToggleLegend
            );

            // Data & Services
            _dataSource = ScriptableObject.CreateInstance<WorldMapConfigSO>();
            _dataSource.InitializeFromDefaults();
            _stateRepo = new InMemoryTerritoryStateRepository(_dataSource);
            _locService = new LocalizationService(GameLog.Default);
            _locService.SetLanguage(Language.English);
            _mapService = new WorldMapService(_dataSource, _stateRepo, GameLog.Default);

            _mapView.Bind(_mapService, _locService, _cameraRig, _camera);
            _mapView.SetWorldCamera(_camera);
            _drawer.Bind(_mapService, _locService);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_holderGo != null)
            {
                Object.DestroyImmediate(_holderGo);
            }
            if (_dataSource != null)
            {
                Object.DestroyImmediate(_dataSource);
            }
            yield return null;
        }

        private TextMeshProUGUI CreateTmp(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            return go.AddComponent<TextMeshProUGUI>();
        }

        private Button CreateButton(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.AddComponent<Image>();
            return go.AddComponent<Button>();
        }

        [Test]
        public void WorldMapView_SpawnsAll25Territories()
        {
            Assert.AreEqual(25, _mapView.TerritoryViews.Count, "Must spawn exactly 25 territory views");

            foreach (var kvp in _mapView.TerritoryViews)
            {
                var view = kvp.Value;
                Assert.IsNotNull(view);
                Assert.AreEqual(view.Data.MapPosition, view.CenterPosition, "Territory position must match MapPosition");
            }
        }

        [Test]
        public void WorldMapView_SelectTerritory_UpdatesSelectedView()
        {
            var t1 = new TerritoryId("ter_nrd_001");
            _mapService.SelectTerritory(t1);

            Assert.IsNotNull(_mapView.SelectedView);
            Assert.AreEqual(t1, _mapView.SelectedView.TerritoryId);
            Assert.IsTrue(_mapView.SelectedView.IsSelected);

            // Select another
            var t2 = new TerritoryId("ter_cst_001");
            _mapService.SelectTerritory(t2);

            Assert.IsNotNull(_mapView.SelectedView);
            Assert.AreEqual(t2, _mapView.SelectedView.TerritoryId);
            Assert.IsTrue(_mapView.SelectedView.IsSelected);

            var oldView = _mapView.TerritoryViews[t1];
            Assert.IsFalse(oldView.IsSelected, "Previous territory must be deselected");
        }

        [Test]
        public void WorldMapView_Deselect_ClearsSelectedView()
        {
            var t1 = new TerritoryId("ter_nrd_001");
            _mapService.SelectTerritory(t1);
            Assert.IsNotNull(_mapView.SelectedView);

            _mapService.SelectTerritory(null);
            Assert.IsNull(_mapView.SelectedView);
        }

        [Test]
        public void WorldMapView_RaycastSelection_SelectsClickedTerritory()
        {
            var targetId = new TerritoryId("ter_val_001");
            var targetView = _mapView.TerritoryViews[targetId];

            // Point camera directly at the territory
            _camera.transform.position = targetView.CenterPosition + new Vector3(0, 30, -20);
            _camera.transform.LookAt(targetView.CenterPosition);

            Vector2 screenPoint = _camera.WorldToScreenPoint(targetView.CenterPosition);
            _mapView.PerformRaycastSelection(screenPoint);

            Assert.AreEqual(targetId, _mapService.SelectedTerritoryId);
            Assert.AreEqual(targetId, _mapView.SelectedView.TerritoryId);
        }

        [Test]
        public void WorldMapView_RaycastSelection_EmptySpaceDeselects()
        {
            var targetId = new TerritoryId("ter_val_001");
            _mapService.SelectTerritory(targetId);
            Assert.AreEqual(targetId, _mapService.SelectedTerritoryId);

            // Raycast into empty void
            _mapView.PerformRaycastSelection(new Vector2(0, 0));

            Assert.IsNull(_mapService.SelectedTerritoryId);
            Assert.IsNull(_mapView.SelectedView);
        }

        [Test]
        public void TerritoryDetailDrawer_DisplaysCorrectDetails()
        {
            var targetId = new TerritoryId("ter_val_001"); // Player Allied capital
            _mapService.SelectTerritory(targetId);

            Assert.IsTrue(_drawer.IsVisible, "Drawer must be visible when territory is selected");
            Assert.AreEqual(targetId, _drawer.CurrentTerritoryId);

            StringAssert.Contains("Emerald Nexus", _drawer.TerritoryNameText.text);
            StringAssert.Contains("VERDANT VALLEY", _drawer.RegionBadgeText.text);
            Assert.AreEqual("★", _drawer.FactionSymbolText.text);
            StringAssert.Contains("Commander", _drawer.OwnerPlayerText.text);
            StringAssert.Contains("+2,000/h", _drawer.YieldGoldText.text);
            StringAssert.Contains("5,000", _drawer.GarrisonDefenseText.text);
        }

        [Test]
        public void TerritoryDetailDrawer_ActionButtons_AreDisabled()
        {
            _mapService.SelectTerritory(new TerritoryId("ter_val_001"));

            Assert.IsFalse(_drawer.BtnAttack.interactable, "Attack button must be disabled in Phase 4");
            Assert.IsFalse(_drawer.BtnScout.interactable, "Scout button must be disabled in Phase 4");
        }

        [Test]
        public void TerritoryDetailDrawer_LegendToggle_TogglesVisibility()
        {
            _mapService.SelectTerritory(new TerritoryId("ter_val_001"));

            Assert.IsFalse(_drawer.LegendPanelRoot.activeSelf, "Legend must start hidden");

            _drawer.BtnToggleLegend.onClick.Invoke();
            Assert.IsTrue(_drawer.LegendPanelRoot.activeSelf, "Legend must open on first toggle");

            _drawer.BtnToggleLegend.onClick.Invoke();
            Assert.IsFalse(_drawer.LegendPanelRoot.activeSelf, "Legend must close on second toggle");
        }

        [Test]
        public void TerritoryDetailDrawer_CloseButton_DeselectsAndHides()
        {
            _mapService.SelectTerritory(new TerritoryId("ter_val_001"));
            Assert.IsTrue(_drawer.IsVisible);

            _drawer.BtnClose.onClick.Invoke();

            Assert.IsFalse(_drawer.IsVisible);
            Assert.IsNull(_mapService.SelectedTerritoryId);
        }

        [Test]
        public void TerritoryDetailDrawer_LanguageChange_UpdatesLocalizationInRealtime()
        {
            var targetId = new TerritoryId("ter_val_001");
            _mapService.SelectTerritory(targetId);

            StringAssert.Contains("Emerald Nexus", _drawer.TerritoryNameText.text);

            _locService.SetLanguage(Language.Arabic);

            // Shaper produces connected Arabic
            var shapedExpected = ArabicTextShaper.Shape("ملتقى الزمرد المركزي");
            StringAssert.Contains(shapedExpected, _drawer.TerritoryNameText.text);
        }
    }
}
