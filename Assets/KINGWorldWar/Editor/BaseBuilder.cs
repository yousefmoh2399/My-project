using System.IO;
using KingWorldWar.App;
using KingWorldWar.Core.Base;
using KingWorldWar.Features.Base;
using KingWorldWar.Features.Camera;
using KingWorldWar.Infrastructure.Base;
using KingWorldWar.UI.Base;
using KingWorldWar.UI.Common;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.Editor
{
    /// <summary>
    /// Editor builder for the Player Base scene integration (Phase 5).
    /// Instantiates and wires PlayerBaseView, BaseGridView, BuildingDetailDrawer,
    /// and BaseConfig.asset into MainMenu.unity.
    /// </summary>
    public static class BaseBuilder
    {
        private const string ScenePath = "Assets/KINGWorldWar/Scenes/MainMenu.unity";
        private const string ConfigAssetPath = "Assets/KINGWorldWar/Art/Settings/BaseConfig.asset";

        private const string HQPrefabPath = "Assets/KINGWorldWar/Art/Prefabs/Buildings/HQ_CommandCenter.prefab";
        private const string BarracksPrefabPath = "Assets/KINGWorldWar/Art/Prefabs/Buildings/Barracks.prefab";
        private const string DepotPrefabPath = "Assets/KINGWorldWar/Art/Prefabs/Buildings/SupplyDepot.prefab";
        private const string RadarPrefabPath = "Assets/KINGWorldWar/Art/Prefabs/Buildings/RadarStation.prefab";
        private const string FactoryPrefabPath = "Assets/KINGWorldWar/Art/Prefabs/Buildings/VehicleFactory.prefab";

        private static readonly Color Charcoal = new Color32(0x1F, 0x24, 0x21, 0xFF);
        private static readonly Color SteelGray = new Color32(0x4A, 0x55, 0x68, 0xFF);
        private static readonly Color MilitaryGreen = new Color32(0x3E, 0x4E, 0x3D, 0xFF);
        private static readonly Color Gold = new Color32(0xD4, 0xAF, 0x37, 0xFF);

        [MenuItem("KingWorldWar/Build Player Base Scene (Phase 5)")]
        public static void BuildPlayerBaseIntegration()
        {
            // 1. Ensure BaseConfig asset exists
            var config = AssetDatabase.LoadAssetAtPath<BaseConfigSO>(ConfigAssetPath);
            if (config == null)
            {
                var dir = Path.GetDirectoryName(ConfigAssetPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                config = ScriptableObject.CreateInstance<BaseConfigSO>();
                config.InitializeFromDefaults();
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[BaseBuilder] Created BaseConfig asset at {ConfigAssetPath}");
            }

            // 2. Open MainMenu scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var scope = Object.FindAnyObjectByType<MainMenuScope>();
            if (scope == null)
            {
                Debug.LogError("[BaseBuilder] MainMenuScope not found in MainMenu.unity!");
                return;
            }

            var mainCamera = UnityEngine.Camera.main;
            var cameraRig = Object.FindAnyObjectByType<StrategyCameraRig>();

            // 3. Player Base Root & View
            var baseRootGo = GameObject.Find("PlayerBaseRoot");
            if (baseRootGo == null)
            {
                baseRootGo = new GameObject("PlayerBaseRoot");
            }

            var baseView = baseRootGo.GetComponent<PlayerBaseView>() ?? baseRootGo.AddComponent<PlayerBaseView>();
            var gridView = baseRootGo.GetComponent<BaseGridView>() ?? baseRootGo.AddComponent<BaseGridView>();

            var containerGo = GameObject.Find("BaseBuildingsContainer");
            if (containerGo == null)
            {
                containerGo = new GameObject("BaseBuildingsContainer");
                containerGo.transform.SetParent(baseRootGo.transform, false);
            }

            // Load building prefabs
            var hqPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HQPrefabPath);
            var barracksPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BarracksPrefabPath);
            var depotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DepotPrefabPath);
            var radarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RadarPrefabPath);
            var factoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FactoryPrefabPath);

            var baseViewSo = new SerializedObject(baseView);
            baseViewSo.FindProperty("_worldCamera").objectReferenceValue = mainCamera;
            baseViewSo.FindProperty("_cameraRig").objectReferenceValue = cameraRig;
            baseViewSo.FindProperty("_gridView").objectReferenceValue = gridView;
            baseViewSo.FindProperty("_buildingsContainer").objectReferenceValue = containerGo.transform;
            baseViewSo.FindProperty("_hqPrefab").objectReferenceValue = hqPrefab;
            baseViewSo.FindProperty("_barracksPrefab").objectReferenceValue = barracksPrefab;
            baseViewSo.FindProperty("_depotPrefab").objectReferenceValue = depotPrefab;
            baseViewSo.FindProperty("_radarPrefab").objectReferenceValue = radarPrefab;
            baseViewSo.FindProperty("_factoryPrefab").objectReferenceValue = factoryPrefab;
            baseViewSo.ApplyModifiedPropertiesWithoutUndo();

            // 4. Building Detail Drawer UI
            var safeAreaGo = GameObject.Find("SafeAreaContainer");
            var drawerGo = GameObject.Find("BuildingDetailDrawer");

            if (drawerGo == null && safeAreaGo != null)
            {
                drawerGo = BuildDrawerUI(safeAreaGo.transform);
            }

            var drawer = drawerGo != null ? drawerGo.GetComponent<BuildingDetailDrawer>() : null;

            // 5. Make Screen_World and Screen_Base transparent so 3D scene is visible and interactable
            ConfigureScreenTransparency("Screen_World");
            ConfigureScreenTransparency("Screen_Base");

            // 6. Wire MainMenuScope
            var scopeSo = new SerializedObject(scope);
            scopeSo.FindProperty("_playerBaseView").objectReferenceValue = baseView;
            scopeSo.FindProperty("_buildingDetailDrawer").objectReferenceValue = drawer;
            scopeSo.ApplyModifiedPropertiesWithoutUndo();

            // 7. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[BaseBuilder] MainMenu.unity updated with Player Base features and saved successfully!");
        }

        private static void ConfigureScreenTransparency(string screenName)
        {
            var screenGo = GameObject.Find(screenName);
            if (screenGo == null) return;

            var img = screenGo.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0, 0, 0, 0);
                img.raycastTarget = false;
            }

            // Hide placeholder text children for World and Base
            for (int i = 0; i < screenGo.transform.childCount; i++)
            {
                screenGo.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private static GameObject BuildDrawerUI(Transform parent)
        {
            var root = new GameObject("BuildingDetailDrawer");
            root.transform.SetParent(parent, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var drawer = root.AddComponent<BuildingDetailDrawer>();

            // Panel Root (Anchored to bottom sheet)
            var panel = new GameObject("Panel_BuildingSheet");
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.48f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.offsetMin = new Vector2(16f, 150f); // above bottom nav bar
            panelRect.offsetMax = new Vector2(-16f, 0f);

            var panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.12f, 0.14f, 0.13f, 0.98f);

            var vLayout = panel.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(24, 24, 18, 18);
            vLayout.spacing = 8;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;

            // --- Header Row (Title & Close Button) ---
            var headerRow = new GameObject("HeaderRow");
            headerRow.transform.SetParent(panel.transform, false);
            var hLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            var hRect = headerRow.GetComponent<RectTransform>();
            hRect.sizeDelta = new Vector2(0, 40);

            var titleTmp = CreateText(headerRow.transform, "BuildingNameText", "Command Center", 24, FontStyles.Bold, Color.white);
            titleTmp.rectTransform.sizeDelta = new Vector2(400, 40);

            var levelBadge = CreateText(headerRow.transform, "LevelBadgeText", "Level 1", 20, FontStyles.Bold, Gold);
            levelBadge.rectTransform.sizeDelta = new Vector2(180, 40);

            var closeBtnGo = new GameObject("BtnClose");
            closeBtnGo.transform.SetParent(headerRow.transform, false);
            var closeBtnRect = closeBtnGo.AddComponent<RectTransform>();
            closeBtnRect.sizeDelta = new Vector2(48, 48);
            var closeImg = closeBtnGo.AddComponent<Image>();
            closeImg.color = new Color(0.3f, 0.35f, 0.3f, 0.8f);
            var closeBtn = closeBtnGo.AddComponent<Button>();
            var closeTxt = CreateText(closeBtnGo.transform, "Txt", "X", 22, FontStyles.Bold, Color.white);
            closeTxt.alignment = TextAlignmentOptions.Center;

            // Description
            var descTmp = CreateText(panel.transform, "DescriptionText", "Base operations headquarters.", 16, FontStyles.Normal, SteelGray);
            descTmp.rectTransform.sizeDelta = new Vector2(0, 32);

            // --- Stats Section ---
            var statsRow = new GameObject("StatsRow");
            statsRow.transform.SetParent(panel.transform, false);
            var sLayout = statsRow.AddComponent<HorizontalLayoutGroup>();
            sLayout.spacing = 15;
            sLayout.childControlWidth = true;
            sLayout.childForceExpandWidth = true;
            var sRect = statsRow.GetComponent<RectTransform>();
            sRect.sizeDelta = new Vector2(0, 36);

            var curPerk = CreateText(statsRow.transform, "CurrentPerkText", "Max Level: 1", 16, FontStyles.Bold, Color.white);
            var nextPerk = CreateText(statsRow.transform, "NextPerkText", "Next: Max Level: 2", 16, FontStyles.Bold, Gold);

            // --- Upgrade Requirements Root ---
            var reqRoot = new GameObject("UpgradeRequirements");
            reqRoot.transform.SetParent(panel.transform, false);
            var rLayout = reqRoot.AddComponent<VerticalLayoutGroup>();
            rLayout.spacing = 6;
            rLayout.childControlWidth = true;
            rLayout.childForceExpandWidth = true;

            var costsRow = new GameObject("CostsRow");
            costsRow.transform.SetParent(reqRoot.transform, false);
            var cLayout = costsRow.AddComponent<HorizontalLayoutGroup>();
            cLayout.spacing = 15;
            cLayout.childControlWidth = true;
            cLayout.childForceExpandWidth = true;
            var cRect = costsRow.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(0, 30);

            var goldTxt = CreateText(costsRow.transform, "CostGoldText", "Gold: 500", 16, FontStyles.Normal, new Color(1f, 0.85f, 0.2f));
            var oilTxt = CreateText(costsRow.transform, "CostOilText", "Oil: 200", 16, FontStyles.Normal, new Color(0.2f, 0.8f, 0.9f));
            var ironTxt = CreateText(costsRow.transform, "CostIronText", "Iron: 100", 16, FontStyles.Normal, new Color(0.9f, 0.5f, 0.2f));
            var timeTxt = CreateText(costsRow.transform, "DurationText", "00:30", 16, FontStyles.Bold, Color.white);

            var reqHqTxt = CreateText(reqRoot.transform, "PrerequisiteHQText", "", 15, FontStyles.Italic, Color.yellow);
            reqHqTxt.rectTransform.sizeDelta = new Vector2(0, 24);

            // --- Upgrading Status Root ---
            var upgRoot = new GameObject("UpgradingStatus");
            upgRoot.transform.SetParent(panel.transform, false);
            var uLayout = upgRoot.AddComponent<VerticalLayoutGroup>();
            uLayout.spacing = 6;
            uLayout.childControlWidth = true;
            uLayout.childForceExpandWidth = true;

            var cdTmp = CreateText(upgRoot.transform, "CountdownText", "Time Remaining: 00:25", 18, FontStyles.Bold, new Color(0.2f, 0.9f, 0.3f));
            cdTmp.alignment = TextAlignmentOptions.Center;

            var pbBg = new GameObject("ProgressBarBg");
            pbBg.transform.SetParent(upgRoot.transform, false);
            var pbBgRect = pbBg.AddComponent<RectTransform>();
            pbBgRect.sizeDelta = new Vector2(0, 16);
            var pbBgImg = pbBg.AddComponent<Image>();
            pbBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var pbFill = new GameObject("ProgressBarFill");
            pbFill.transform.SetParent(pbBg.transform, false);
            var pbFillRect = pbFill.AddComponent<RectTransform>();
            pbFillRect.anchorMin = Vector2.zero;
            pbFillRect.anchorMax = Vector2.one;
            pbFillRect.offsetMin = Vector2.zero;
            pbFillRect.offsetMax = Vector2.zero;
            var pbFillImg = pbFill.AddComponent<Image>();
            pbFillImg.color = new Color(0.2f, 0.9f, 0.3f, 1f);
            pbFillImg.type = Image.Type.Filled;
            pbFillImg.fillMethod = Image.FillMethod.Horizontal;

            // --- Action Buttons ---
            var actionRow = new GameObject("ActionRow");
            actionRow.transform.SetParent(panel.transform, false);
            var aLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            aLayout.spacing = 15;
            aLayout.childControlWidth = true;
            aLayout.childForceExpandWidth = true;
            var aRect = actionRow.GetComponent<RectTransform>();
            aRect.sizeDelta = new Vector2(0, 50);

            var upgradeBtnGo = new GameObject("BtnUpgrade");
            upgradeBtnGo.transform.SetParent(actionRow.transform, false);
            var upBtnImg = upgradeBtnGo.AddComponent<Image>();
            upBtnImg.color = MilitaryGreen;
            var upBtn = upgradeBtnGo.AddComponent<Button>();
            var upBtnTxt = CreateText(upgradeBtnGo.transform, "Txt", "Upgrade", 20, FontStyles.Bold, Color.white);
            upBtnTxt.alignment = TextAlignmentOptions.Center;

            var cancelBtnGo = new GameObject("BtnCancelUpgrade");
            cancelBtnGo.transform.SetParent(actionRow.transform, false);
            var cancelBtnImg = cancelBtnGo.AddComponent<Image>();
            cancelBtnImg.color = new Color(0.6f, 0.2f, 0.2f);
            var cancelBtn = cancelBtnGo.AddComponent<Button>();
            var cancelBtnTxt = CreateText(cancelBtnGo.transform, "Txt", "Cancel", 20, FontStyles.Bold, Color.white);
            cancelBtnTxt.alignment = TextAlignmentOptions.Center;

            drawer.SetupComponentsForTest(
                panel,
                titleTmp,
                levelBadge,
                descTmp,
                curPerk,
                nextPerk,
                reqRoot,
                goldTxt,
                oilTxt,
                ironTxt,
                timeTxt,
                reqHqTxt,
                upBtn,
                upBtnTxt,
                cancelBtn,
                closeBtn,
                upgRoot,
                cdTmp,
                pbFillImg
            );

            panel.SetActive(false);
            upgRoot.SetActive(false);
            return root;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, FontStyles style, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            return tmp;
        }
    }
}
