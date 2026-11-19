using System.IO;
using KingWorldWar.App;
using KingWorldWar.Core.WorldMap;
using KingWorldWar.Features.Camera;
using KingWorldWar.Features.WorldMap;
using KingWorldWar.Infrastructure.WorldMap;
using KingWorldWar.UI.Common;
using KingWorldWar.UI.WorldMap;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.Editor
{
    /// <summary>
    /// Editor builder for the World Map scene integration (Phase 4).
    /// Instantiates and wires WorldMapView, Camera Rig, and TerritoryDetailDrawer into MainMenu.unity.
    /// </summary>
    public static class WorldMapBuilder
    {
        private const string ScenePath = "Assets/KINGWorldWar/Scenes/MainMenu.unity";
        private const string ConfigAssetPath = "Assets/KINGWorldWar/Art/Settings/WorldMapConfig.asset";

        private static readonly Color Charcoal = new Color32(0x1F, 0x24, 0x21, 0xFF);
        private static readonly Color SteelGray = new Color32(0x4A, 0x55, 0x68, 0xFF);
        private static readonly Color MilitaryGreen = new Color32(0x3E, 0x4E, 0x3D, 0xFF);
        private static readonly Color Gold = new Color32(0xD4, 0xAF, 0x37, 0xFF);

        [MenuItem("KingWorldWar/Build World Map Scene (Phase 4)")]
        public static void BuildWorldMapIntegration()
        {
            // 1. Ensure WorldMapConfig asset exists
            var config = AssetDatabase.LoadAssetAtPath<WorldMapConfigSO>(ConfigAssetPath);
            if (config == null)
            {
                var dir = Path.GetDirectoryName(ConfigAssetPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                config = ScriptableObject.CreateInstance<WorldMapConfigSO>();
                config.InitializeFromDefaults();
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[WorldMapBuilder] Created WorldMapConfig asset at {ConfigAssetPath}");
            }

            // 2. Open MainMenu scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var scope = Object.FindAnyObjectByType<MainMenuScope>();
            if (scope == null)
            {
                Debug.LogError("[WorldMapBuilder] MainMenuScope not found in MainMenu.unity!");
                return;
            }

            var mainCamera = UnityEngine.Camera.main;

            // 3. Strategy Camera Rig
            var rigGo = GameObject.Find("StrategyCameraRig");
            if (rigGo == null)
            {
                rigGo = new GameObject("StrategyCameraRig");
                if (mainCamera != null)
                {
                    rigGo.transform.position = new Vector3(0, 80, -60);
                    mainCamera.transform.SetParent(rigGo.transform, false);
                    mainCamera.transform.localPosition = Vector3.zero;
                    mainCamera.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
                }
            }

            var cameraRig = rigGo.GetComponent<StrategyCameraRig>();
            if (cameraRig == null) cameraRig = rigGo.AddComponent<StrategyCameraRig>();

            var camConfig = AssetDatabase.LoadAssetAtPath<CameraConfigSO>("Assets/KINGWorldWar/Art/Settings/StrategyCameraConfig.asset");
            if (camConfig == null)
            {
                camConfig = CameraConfigSO.CreateDefaultInstance();
                AssetDatabase.CreateAsset(camConfig, "Assets/KINGWorldWar/Art/Settings/StrategyCameraConfig.asset");
            }
            cameraRig.Config = camConfig;

            // 4. World Map Root & View
            var mapRootGo = GameObject.Find("WorldMapRoot");
            if (mapRootGo == null)
            {
                mapRootGo = new GameObject("WorldMapRoot");
            }

            var mapView = mapRootGo.GetComponent<WorldMapView>();
            if (mapView == null) mapView = mapRootGo.AddComponent<WorldMapView>();

            var containerGo = GameObject.Find("TerritoriesContainer");
            if (containerGo == null)
            {
                containerGo = new GameObject("TerritoriesContainer");
                containerGo.transform.SetParent(mapRootGo.transform, false);
            }

            // Wire WorldMapView serialized fields
            var mapViewSo = new SerializedObject(mapView);
            mapViewSo.FindProperty("_worldCamera").objectReferenceValue = mainCamera;
            mapViewSo.FindProperty("_cameraRig").objectReferenceValue = cameraRig;
            mapViewSo.FindProperty("_territoriesContainer").objectReferenceValue = containerGo.transform;
            mapViewSo.ApplyModifiedPropertiesWithoutUndo();

            // 5. Territory Detail Drawer UI
            var canvasGo = GameObject.Find("MainMenuCanvas");
            var safeAreaGo = GameObject.Find("SafeAreaContainer");
            var drawerGo = GameObject.Find("TerritoryDetailDrawer");

            if (drawerGo == null && safeAreaGo != null)
            {
                drawerGo = BuildDrawerUI(safeAreaGo.transform);
            }

            var drawer = drawerGo != null ? drawerGo.GetComponent<TerritoryDetailDrawer>() : null;

            // 6. Wire MainMenuScope
            var scopeSo = new SerializedObject(scope);
            scopeSo.FindProperty("_worldMapView").objectReferenceValue = mapView;
            scopeSo.FindProperty("_territoryDetailDrawer").objectReferenceValue = drawer;
            scopeSo.FindProperty("_cameraRig").objectReferenceValue = cameraRig;
            scopeSo.ApplyModifiedPropertiesWithoutUndo();

            // 7. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[WorldMapBuilder] MainMenu.unity updated with World Map features and saved successfully!");
        }

        private static GameObject BuildDrawerUI(Transform parent)
        {
            var root = new GameObject("TerritoryDetailDrawer");
            root.transform.SetParent(parent, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var drawer = root.AddComponent<TerritoryDetailDrawer>();

            // Panel Root (Anchored to bottom sheet)
            var panel = new GameObject("Panel_BottomSheet");
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.45f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.offsetMin = new Vector2(16f, 150f); // above bottom nav bar
            panelRect.offsetMax = new Vector2(-16f, 0f);

            var panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.12f, 0.14f, 0.13f, 0.98f);

            var vLayout = panel.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(24, 24, 20, 20);
            vLayout.spacing = 10;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;

            // Header Row (Territory Name + Region Badge + Close)
            var headerRow = CreateRow(panel.transform, "HeaderRow", 60f);
            var tName = CreateText(headerRow.transform, "TerritoryName", "Territory Name", 28, FontStyles.Bold, Color.white);
            var rBadge = CreateText(headerRow.transform, "RegionBadge", "[REGION]", 20, FontStyles.Bold, Gold);
            var btnClose = CreateButton(headerRow.transform, "Btn_Close", "✕", 50f, ButtonStyleType.Secondary);

            // Faction Row (Symbol + Name + Owner)
            var factionRow = CreateRow(panel.transform, "FactionRow", 44f);
            var fSymbol = CreateText(factionRow.transform, "FactionSymbol", "★", 26, FontStyles.Bold, Gold);
            var fName = CreateText(factionRow.transform, "FactionName", "Player Allied", 22, FontStyles.Normal, Color.white);
            var oPlayer = CreateText(factionRow.transform, "OwnerPlayer", "[VGD] Commander", 20, FontStyles.Italic, SteelGray);

            // Yields Row (Gold, Oil, Iron)
            var yieldsRow = CreateRow(panel.transform, "YieldsRow", 40f);
            var yGold = CreateText(yieldsRow.transform, "YieldGold", "+1,000/h", 20, FontStyles.Bold, Gold);
            var yOil = CreateText(yieldsRow.transform, "YieldOil", "+1,000/h", 20, FontStyles.Bold, SteelGray);
            var yIron = CreateText(yieldsRow.transform, "YieldIron", "+1,000/h", 20, FontStyles.Bold, new Color(0.7f, 0.7f, 0.8f));

            // Defense Row
            var defenseRow = CreateRow(panel.transform, "DefenseRow", 36f);
            var gDefenseLbl = CreateText(defenseRow.transform, "DefenseLabel", "DEFENSE POWER:", 18, FontStyles.Normal, SteelGray);
            var gDefense = CreateText(defenseRow.transform, "GarrisonDefense", "10,000", 22, FontStyles.Bold, Color.white);

            // Frontiers Row
            var frontiersRow = CreateRow(panel.transform, "FrontiersRow", 34f);
            var aFrontiers = CreateText(frontiersRow.transform, "AdjacentFrontiers", "Connected Borders", 18, FontStyles.Normal, SteelGray);

            // Action Buttons Row (Disabled in Phase 4)
            var actionsRow = CreateRow(panel.transform, "ActionsRow", 64f);
            var btnAttack = CreateButton(actionsRow.transform, "Btn_Attack", "ATTACK", 140f, ButtonStyleType.Primary);
            var btnScout = CreateButton(actionsRow.transform, "Btn_Scout", "SCOUT", 140f, ButtonStyleType.Secondary);
            var btnLegend = CreateButton(actionsRow.transform, "Btn_Legend", "LEGEND", 100f, ButtonStyleType.Secondary);

            // Phase 9 Note
            var noteTmp = CreateText(panel.transform, "Phase9Note", "[Attack & Scout available in Phase 9]", 16, FontStyles.Italic, SteelGray);
            noteTmp.alignment = TextAlignmentOptions.Center;

            // Legend Panel (Hidden by default)
            var legendPanel = new GameObject("LegendPanelRoot");
            legendPanel.transform.SetParent(root.transform, false);
            var legendRect = legendPanel.AddComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0.2f, 0.5f);
            legendRect.anchorMax = new Vector2(0.8f, 0.8f);
            legendRect.offsetMin = Vector2.zero;
            legendRect.offsetMax = Vector2.zero;
            var legendBg = legendPanel.AddComponent<Image>();
            legendBg.color = new Color(0.1f, 0.12f, 0.11f, 0.98f);

            var legLayout = legendPanel.AddComponent<VerticalLayoutGroup>();
            legLayout.padding = new RectOffset(20, 20, 20, 20);
            legLayout.spacing = 10;
            CreateText(legendPanel.transform, "LegTitle", "TACTICAL MAP SYMBOLS", 22, FontStyles.Bold, Gold);
            CreateText(legendPanel.transform, "LegPlayer", "★ Player Allied Territory", 18, FontStyles.Normal, Color.white);
            CreateText(legendPanel.transform, "LegHostile", "✖ Hostile Crimson Territory", 18, FontStyles.Normal, Color.white);
            CreateText(legendPanel.transform, "LegIron", "◆ Hostile Iron Pact Territory", 18, FontStyles.Normal, Color.white);
            CreateText(legendPanel.transform, "LegNeutral", "▲ Neutral Territory", 18, FontStyles.Normal, Color.white);
            legendPanel.SetActive(false);

            // Setup serialized fields on drawer
            var drawerSo = new SerializedObject(drawer);
            drawerSo.FindProperty("_panelRoot").objectReferenceValue = panel;
            drawerSo.FindProperty("_territoryNameText").objectReferenceValue = tName;
            drawerSo.FindProperty("_regionBadgeText").objectReferenceValue = rBadge;
            drawerSo.FindProperty("_factionNameText").objectReferenceValue = fName;
            drawerSo.FindProperty("_factionSymbolText").objectReferenceValue = fSymbol;
            drawerSo.FindProperty("_ownerPlayerText").objectReferenceValue = oPlayer;
            drawerSo.FindProperty("_yieldGoldText").objectReferenceValue = yGold;
            drawerSo.FindProperty("_yieldOilText").objectReferenceValue = yOil;
            drawerSo.FindProperty("_yieldIronText").objectReferenceValue = yIron;
            drawerSo.FindProperty("_garrisonDefenseText").objectReferenceValue = gDefense;
            drawerSo.FindProperty("_adjacentFrontiersText").objectReferenceValue = aFrontiers;
            drawerSo.FindProperty("_btnAttack").objectReferenceValue = btnAttack;
            drawerSo.FindProperty("_btnScout").objectReferenceValue = btnScout;
            drawerSo.FindProperty("_phase9NoteText").objectReferenceValue = noteTmp;
            drawerSo.FindProperty("_btnClose").objectReferenceValue = btnClose;
            drawerSo.FindProperty("_legendPanelRoot").objectReferenceValue = legendPanel;
            drawerSo.FindProperty("_btnToggleLegend").objectReferenceValue = btnLegend;
            drawerSo.ApplyModifiedPropertiesWithoutUndo();

            panel.SetActive(false); // start hidden until selected
            return root;
        }

        private static GameObject CreateRow(Transform parent, string name, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 16;
            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, FontStyles style, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, float width, ButtonStyleType style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 54f);

            var img = go.AddComponent<Image>();
            img.color = style == ButtonStyleType.Primary ? Gold : MilitaryGreen;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var kwwBtn = go.AddComponent<KwwButton>();

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform, false);
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = style == ButtonStyleType.Primary ? Charcoal : Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            var txtRect = txtGo.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            return btn;
        }
    }
}
