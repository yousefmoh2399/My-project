using System.IO;
using KingWorldWar.App;
using KingWorldWar.Features.Army;
using KingWorldWar.Features.Base;
using KingWorldWar.UI.Army;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.Editor
{
    /// <summary>
    /// Builds authentic, textured 3D prefabs for KING WORLD WAR from CC0/Public Domain 3D models.
    /// Replaces all primitive placeholder cubes with real military 3D assets.
    /// Wires the Army Screen, Unit Detail 3D Rotating Preview Stage, and Base Screen into MainMenu.unity.
    /// </summary>
    public static class RealAssetPrefabBuilder
    {
        private const string UnitsPrefabDir = "Assets/KINGWorldWar/Art/Prefabs/Units";
        private const string UnitsResourcesDir = "Assets/KINGWorldWar/Resources/Units";
        private const string BuildingsPrefabDir = "Assets/KINGWorldWar/Art/Prefabs/Buildings";
        private const string MaterialsDir = "Assets/KINGWorldWar/Art/Materials";
        private const string ModelsDir = "Assets/KINGWorldWar/Art/Models";
        private const string ScenePath = "Assets/KINGWorldWar/Scenes/MainMenu.unity";

        private static readonly Color Charcoal = new Color32(0x14, 0x18, 0x16, 0xEE);
        private static readonly Color SteelGray = new Color32(0x71, 0x80, 0x96, 0xFF);
        private static readonly Color MilitaryGreen = new Color32(0x2E, 0x5A, 0x36, 0xFF);
        private static readonly Color Gold = new Color32(0xD4, 0xAF, 0x37, 0xFF);
        private static readonly Color DarkPanel = new Color(0.08f, 0.10f, 0.09f, 0.92f);

        [MenuItem("KingWorldWar/Build Real 3D Prefabs and Wire Scene")]
        public static void BuildAllRealPrefabsAndWireScene()
        {
            EnsureDirectories();

            // Load Materials
            var matOlive = LoadMaterial("M_Military_Olive");
            var matSteel = LoadMaterial("M_Armor_Steel");
            var matDesert = LoadMaterial("M_Desert_Tan");
            var matCharcoal = LoadMaterial("M_Charcoal_Matte");
            var matBuildingCharcoal = LoadMaterial("M_Building_Charcoal");
            var matBuildingSteel = LoadMaterial("M_Building_Steel");

            // Build Unit Prefabs
            var tankPrefab = BuildModelPrefab("Tank_Medium", $"{ModelsDir}/Units/Tank_Medium.obj", matOlive, Vector3.one, UnitsPrefabDir, "Unit_Tank_Placeholder");
            var apcPrefab = BuildModelPrefab("Armored_Vehicle", $"{ModelsDir}/Units/Armored_Vehicle.obj", matDesert, Vector3.one, UnitsPrefabDir, "Unit_ArmoredVehicle_Placeholder");
            var artPrefab = BuildModelPrefab("Artillery", $"{ModelsDir}/Units/Artillery.obj", matSteel, new Vector3(1.2f, 1.2f, 1.2f), UnitsPrefabDir, "Unit_Artillery_Placeholder");
            var jetPrefab = BuildModelPrefab("Fighter_Jet", $"{ModelsDir}/Units/Fighter_Jet.obj", matCharcoal, new Vector3(0.6f, 0.6f, 0.6f), UnitsPrefabDir, "Unit_FighterAircraft_Placeholder");
            var reconPrefab = BuildModelPrefab("Recon_Plane", $"{ModelsDir}/Units/Recon_Plane.obj", matSteel, new Vector3(0.5f, 0.5f, 0.5f), UnitsPrefabDir, "Unit_ReconAircraft_Placeholder");

            // Copy/Save to Resources for runtime loading
            CopyToResources(tankPrefab, "Unit_Tank_Placeholder");
            CopyToResources(apcPrefab, "Unit_ArmoredVehicle_Placeholder");
            CopyToResources(artPrefab, "Unit_Artillery_Placeholder");
            CopyToResources(jetPrefab, "Unit_FighterAircraft_Placeholder");
            CopyToResources(reconPrefab, "Unit_ReconAircraft_Placeholder");

            // Build Building Prefabs
            var hqPrefab = BuildModelPrefab("HQ_CommandCenter", $"{ModelsDir}/Buildings/HQ_Tower.obj", matBuildingCharcoal, new Vector3(0.35f, 0.35f, 0.35f), BuildingsPrefabDir, "HQ_CommandCenter");
            var barracksPrefab = BuildModelPrefab("Barracks", $"{ModelsDir}/Buildings/Barracks.obj", matBuildingSteel, new Vector3(0.8f, 0.8f, 0.8f), BuildingsPrefabDir, "Barracks");
            var depotPrefab = BuildModelPrefab("SupplyDepot", $"{ModelsDir}/Buildings/Supply_Depot.obj", matBuildingCharcoal, new Vector3(1.2f, 1.2f, 1.2f), BuildingsPrefabDir, "SupplyDepot");
            var radarPrefab = BuildModelPrefab("RadarStation", $"{ModelsDir}/Buildings/Radar_Station.obj", matBuildingSteel, new Vector3(0.8f, 0.8f, 0.8f), BuildingsPrefabDir, "RadarStation");
            var factoryPrefab = BuildModelPrefab("VehicleFactory", $"{ModelsDir}/Buildings/Vehicle_Factory.obj", matOlive, new Vector3(0.22f, 0.22f, 0.22f), BuildingsPrefabDir, "VehicleFactory");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Wire into Scene
            WireScene(tankPrefab, apcPrefab, artPrefab, jetPrefab, reconPrefab,
                      hqPrefab, barracksPrefab, depotPrefab, radarPrefab, factoryPrefab);

            Debug.Log("[RealAssetPrefabBuilder] All real 3D prefabs built and MainMenu scene wired successfully!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(UnitsPrefabDir)) Directory.CreateDirectory(UnitsPrefabDir);
            if (!Directory.Exists(UnitsResourcesDir)) Directory.CreateDirectory(UnitsResourcesDir);
            if (!Directory.Exists(BuildingsPrefabDir)) Directory.CreateDirectory(BuildingsPrefabDir);
        }

        private static Material LoadMaterial(string name)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/{name}.mat");
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, $"{MaterialsDir}/{name}.mat");
            }
            return mat;
        }

        private static GameObject BuildModelPrefab(string name, string modelPath, Material mat, Vector3 localScale, string outDir, string prefabName)
        {
            var sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (sourceModel == null)
            {
                Debug.LogError($"[RealAssetPrefabBuilder] Source model not found at {modelPath}");
                return null;
            }

            var root = new GameObject(prefabName);
            var meshFilters = sourceModel.GetComponentsInChildren<MeshFilter>(true);

            if (meshFilters.Length == 1)
            {
                var mf = root.AddComponent<MeshFilter>();
                mf.sharedMesh = meshFilters[0].sharedMesh;
                var mr = root.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
            }
            else
            {
                foreach (var childMf in meshFilters)
                {
                    var childGo = new GameObject(childMf.gameObject.name);
                    childGo.transform.SetParent(root.transform, false);
                    childGo.transform.localPosition = childMf.transform.localPosition;
                    childGo.transform.localRotation = childMf.transform.localRotation;
                    childGo.transform.localScale = childMf.transform.localScale;

                    var mf = childGo.AddComponent<MeshFilter>();
                    mf.sharedMesh = childMf.sharedMesh;
                    var mr = childGo.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = mat;
                }
            }

            root.transform.localScale = localScale;

            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                var col = root.AddComponent<BoxCollider>();
                col.center = root.transform.InverseTransformPoint(bounds.center);
                col.size = new Vector3(
                    bounds.size.x / Mathf.Max(localScale.x, 0.001f),
                    bounds.size.y / Mathf.Max(localScale.y, 0.001f),
                    bounds.size.z / Mathf.Max(localScale.z, 0.001f)
                );
            }

            string outPath = $"{outDir}/{prefabName}.prefab";
            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, outPath);
            Object.DestroyImmediate(root);
            return savedPrefab;
        }

        private static void CopyToResources(GameObject prefab, string prefabName)
        {
            if (prefab == null) return;
            string destPath = $"{UnitsResourcesDir}/{prefabName}.prefab";
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(prefab), destPath);
        }

        private static void WireScene(
            GameObject tankPrefab, GameObject apcPrefab, GameObject artPrefab, GameObject jetPrefab, GameObject reconPrefab,
            GameObject hqPrefab, GameObject barracksPrefab, GameObject depotPrefab, GameObject radarPrefab, GameObject factoryPrefab)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var scope = Object.FindAnyObjectByType<MainMenuScope>();

            // 1. Wire PlayerArmyView
            var armyView = Object.FindAnyObjectByType<PlayerArmyView>();
            if (armyView != null)
            {
                var armySo = new SerializedObject(armyView);
                SetObjectProperty(armySo, "_tankPrefab", tankPrefab);
                SetObjectProperty(armySo, "_armoredVehiclePrefab", apcPrefab);
                SetObjectProperty(armySo, "_artilleryPrefab", artPrefab);
                SetObjectProperty(armySo, "_fighterAircraftPrefab", jetPrefab);
                SetObjectProperty(armySo, "_reconAircraftPrefab", reconPrefab);
                armySo.ApplyModifiedPropertiesWithoutUndo();
            }

            // 2. Wire PlayerBaseView
            var baseView = Object.FindAnyObjectByType<PlayerBaseView>();
            if (baseView != null)
            {
                var baseSo = new SerializedObject(baseView);
                SetObjectProperty(baseSo, "_hqPrefab", hqPrefab);
                SetObjectProperty(baseSo, "_barracksPrefab", barracksPrefab);
                SetObjectProperty(baseSo, "_depotPrefab", depotPrefab);
                SetObjectProperty(baseSo, "_radarPrefab", radarPrefab);
                SetObjectProperty(baseSo, "_factoryPrefab", factoryPrefab);
                baseSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // 3. Create 3D Rotating Preview Stage Anchor for UnitDetailView
            var previewStageAnchor = EnsurePreviewStageAnchor();

            // 4. Build & Wire UnitDetailView UI
            var unitDetailView = EnsureUnitDetailViewUI(previewStageAnchor);

            // 5. Connect UnitDetailView to ArmyScreenView & MainMenuScope
            var armyScreenView = Object.FindAnyObjectByType<ArmyScreenView>();
            if (armyScreenView != null && unitDetailView != null)
            {
                var armyScreenSo = new SerializedObject(armyScreenView);
                var detailProp = armyScreenSo.FindProperty("_unitDetailView");
                if (detailProp != null) detailProp.objectReferenceValue = unitDetailView;
                armyScreenSo.ApplyModifiedPropertiesWithoutUndo();
            }

            if (scope != null && unitDetailView != null)
            {
                var scopeSo = new SerializedObject(scope);
                var scopeDetailProp = scopeSo.FindProperty("_unitDetailView");
                if (scopeDetailProp != null) scopeDetailProp.objectReferenceValue = unitDetailView;
                scopeSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // 6. Add 3D Battlefield Terrain Ground
            Ensure3DTerrainEnvironment();

            // 7. Lighting & Atmosphere
            var sun = GameObject.Find("Directional Light")?.GetComponent<Light>();
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                sun.color = new Color(1f, 0.96f, 0.88f);
                sun.intensity = 1.25f;
                sun.shadows = LightShadows.Soft;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform EnsurePreviewStageAnchor()
        {
            var armyRoot = FindInScene("PlayerArmyRoot");
            if (armyRoot == null) return null;

            var existingAnchor = armyRoot.transform.Find("PreviewStageAnchor");
            if (existingAnchor != null) return existingAnchor;

            var anchorGo = new GameObject("PreviewStageAnchor");
            anchorGo.transform.SetParent(armyRoot.transform, false);
            anchorGo.transform.position = new Vector3(400f, 1.2f, 0f);
            anchorGo.transform.rotation = Quaternion.identity;

            // Rotating stage pedestal disc
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(anchorGo.transform, false);
            pedestal.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            pedestal.transform.localScale = new Vector3(6f, 0.05f, 6f);
            var pedMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/M_Building_Steel.mat");
            if (pedMat != null) pedestal.GetComponent<Renderer>().sharedMaterial = pedMat;
            var col = pedestal.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            return anchorGo.transform;
        }

        private static UnitDetailView EnsureUnitDetailViewUI(Transform previewAnchor)
        {
            var screenArmy = FindInScene("Screen_Army");
            if (screenArmy == null) return null;

            var existingView = screenArmy.GetComponentInChildren<UnitDetailView>(true);
            if (existingView != null) return existingView;

            // Root Drawer Container
            var detailRootGo = new GameObject("UnitDetailDrawer");
            detailRootGo.transform.SetParent(screenArmy.transform, false);
            var rootRect = detailRootGo.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.55f, 0.05f);
            rootRect.anchorMax = new Vector2(0.98f, 0.95f);
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;

            var bgImg = detailRootGo.AddComponent<Image>();
            bgImg.color = DarkPanel;

            var outline = detailRootGo.AddComponent<Outline>();
            outline.effectColor = Gold;
            outline.effectDistance = new Vector2(2, -2);

            var view = detailRootGo.AddComponent<UnitDetailView>();

            // Header Section
            var headerGo = CreateSubPanel(detailRootGo.transform, "Header", new Vector2(0f, 0.85f), new Vector2(1f, 1f));
            var titleTxt = CreateText(headerGo.transform, "Title", "UNIT SPECIFICATIONS", 22f, Gold, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            titleTxt.rectTransform.anchorMin = new Vector2(0.04f, 0.5f);
            titleTxt.rectTransform.anchorMax = new Vector2(0.85f, 0.95f);
            titleTxt.rectTransform.offsetMin = titleTxt.rectTransform.offsetMax = Vector2.zero;

            var nameTxt = CreateText(headerGo.transform, "UnitName", "MAIN BATTLE TANK", 26f, Color.white, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            nameTxt.rectTransform.anchorMin = new Vector2(0.04f, 0.05f);
            nameTxt.rectTransform.anchorMax = new Vector2(0.85f, 0.5f);
            nameTxt.rectTransform.offsetMin = nameTxt.rectTransform.offsetMax = Vector2.zero;

            // Close button
            var closeGo = new GameObject("BtnClose");
            closeGo.transform.SetParent(headerGo.transform, false);
            var closeRect = closeGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.88f, 0.5f);
            closeRect.anchorMax = new Vector2(0.98f, 0.92f);
            closeRect.offsetMin = closeRect.offsetMax = Vector2.zero;
            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.35f, 0.15f, 0.15f, 0.9f);
            var closeBtn = closeGo.AddComponent<Button>();
            var closeTxt = CreateText(closeGo.transform, "X", "X", 18f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            closeTxt.rectTransform.anchorMin = Vector2.zero;
            closeTxt.rectTransform.anchorMax = Vector2.one;
            closeTxt.rectTransform.offsetMin = closeTxt.rectTransform.offsetMax = Vector2.zero;

            // Archetype / Tier tags
            var tagsGo = CreateSubPanel(detailRootGo.transform, "Tags", new Vector2(0.04f, 0.77f), new Vector2(0.96f, 0.84f));
            var catTxt = CreateText(tagsGo.transform, "Category", "Category: Ground", 14f, SteelGray, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            catTxt.rectTransform.anchorMin = new Vector2(0f, 0f);
            catTxt.rectTransform.anchorMax = new Vector2(0.48f, 1f);
            catTxt.rectTransform.offsetMin = catTxt.rectTransform.offsetMax = Vector2.zero;

            var tierTxt = CreateText(tagsGo.transform, "Tier", "Tier 1 • Level 1", 14f, Gold, FontStyles.Bold, TextAlignmentOptions.MidlineRight);
            tierTxt.rectTransform.anchorMin = new Vector2(0.52f, 0f);
            tierTxt.rectTransform.anchorMax = new Vector2(1f, 1f);
            tierTxt.rectTransform.offsetMin = tierTxt.rectTransform.offsetMax = Vector2.zero;

            // Combat Stats Grid (HP, Attack, Defense, Speed, Range, Capacity, Power)
            var statsGo = CreateSubPanel(detailRootGo.transform, "StatsGrid", new Vector2(0.04f, 0.40f), new Vector2(0.96f, 0.75f));
            var statsBg = statsGo.AddComponent<Image>();
            statsBg.color = new Color(0.12f, 0.14f, 0.13f, 0.75f);

            var hpTxt = CreateText(statsGo.transform, "StatHP", "HP: 850", 15f, Color.white, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            hpTxt.rectTransform.anchorMin = new Vector2(0.05f, 0.72f);
            hpTxt.rectTransform.anchorMax = new Vector2(0.48f, 0.95f);
            hpTxt.rectTransform.offsetMin = hpTxt.rectTransform.offsetMax = Vector2.zero;

            var atkTxt = CreateText(statsGo.transform, "StatATK", "ATK: 160", 15f, new Color(1f, 0.45f, 0.45f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            atkTxt.rectTransform.anchorMin = new Vector2(0.52f, 0.72f);
            atkTxt.rectTransform.anchorMax = new Vector2(0.95f, 0.95f);
            atkTxt.rectTransform.offsetMin = atkTxt.rectTransform.offsetMax = Vector2.zero;

            var defTxt = CreateText(statsGo.transform, "StatDEF", "DEF: 120", 15f, new Color(0.45f, 0.75f, 1f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            defTxt.rectTransform.anchorMin = new Vector2(0.05f, 0.48f);
            defTxt.rectTransform.anchorMax = new Vector2(0.48f, 0.71f);
            defTxt.rectTransform.offsetMin = defTxt.rectTransform.offsetMax = Vector2.zero;

            var spdTxt = CreateText(statsGo.transform, "StatSPD", "SPD: 7.0", 15f, Color.white, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            spdTxt.rectTransform.anchorMin = new Vector2(0.52f, 0.48f);
            spdTxt.rectTransform.anchorMax = new Vector2(0.95f, 0.71f);
            spdTxt.rectTransform.offsetMin = spdTxt.rectTransform.offsetMax = Vector2.zero;

            var rngTxt = CreateText(statsGo.transform, "StatRNG", "RNG: 30m", 15f, Color.white, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            rngTxt.rectTransform.anchorMin = new Vector2(0.05f, 0.24f);
            rngTxt.rectTransform.anchorMax = new Vector2(0.48f, 0.47f);
            rngTxt.rectTransform.offsetMin = rngTxt.rectTransform.offsetMax = Vector2.zero;

            var capTxt = CreateText(statsGo.transform, "StatCAP", "CAP: 3", 15f, SteelGray, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            capTxt.rectTransform.anchorMin = new Vector2(0.52f, 0.24f);
            capTxt.rectTransform.anchorMax = new Vector2(0.95f, 0.47f);
            capTxt.rectTransform.offsetMin = capTxt.rectTransform.offsetMax = Vector2.zero;

            var pwrTxt = CreateText(statsGo.transform, "StatPWR", "Combat Power: 320", 16f, Gold, FontStyles.Bold, TextAlignmentOptions.Center);
            pwrTxt.rectTransform.anchorMin = new Vector2(0.05f, 0.02f);
            pwrTxt.rectTransform.anchorMax = new Vector2(0.95f, 0.23f);
            pwrTxt.rectTransform.offsetMin = pwrTxt.rectTransform.offsetMax = Vector2.zero;

            // Upgrade Section
            var upgGo = CreateSubPanel(detailRootGo.transform, "UpgradeSection", new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.38f));
            var upgPerksTxt = CreateText(upgGo.transform, "Perks", "+15% Combat Efficiency", 14f, Gold, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            upgPerksTxt.rectTransform.anchorMin = new Vector2(0f, 0.6f);
            upgPerksTxt.rectTransform.anchorMax = new Vector2(1f, 1f);
            upgPerksTxt.rectTransform.offsetMin = upgPerksTxt.rectTransform.offsetMax = Vector2.zero;

            var upgCostTxt = CreateText(upgGo.transform, "Cost", "G: 5000 | O: 3000 | I: 3000", 14f, SteelGray, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            upgCostTxt.rectTransform.anchorMin = new Vector2(0f, 0.25f);
            upgCostTxt.rectTransform.anchorMax = new Vector2(1f, 0.6f);
            upgCostTxt.rectTransform.offsetMin = upgCostTxt.rectTransform.offsetMax = Vector2.zero;

            var upgDurTxt = CreateText(upgGo.transform, "Duration", "Research: 03:00", 13f, SteelGray, FontStyles.Italic, TextAlignmentOptions.BottomLeft);
            upgDurTxt.rectTransform.anchorMin = new Vector2(0f, 0f);
            upgDurTxt.rectTransform.anchorMax = new Vector2(1f, 0.25f);
            upgDurTxt.rectTransform.offsetMin = upgDurTxt.rectTransform.offsetMax = Vector2.zero;

            // Action Buttons Section
            var actionsGo = CreateSubPanel(detailRootGo.transform, "ActionRow", new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.13f));
            var trainGo = new GameObject("BtnTrain");
            trainGo.transform.SetParent(actionsGo.transform, false);
            var trainRect = trainGo.AddComponent<RectTransform>();
            trainRect.anchorMin = new Vector2(0f, 0f);
            trainRect.anchorMax = new Vector2(1f, 1f);
            trainRect.offsetMin = trainRect.offsetMax = Vector2.zero;
            var trainImg = trainGo.AddComponent<Image>();
            trainImg.color = MilitaryGreen;
            var trainBtn = trainGo.AddComponent<Button>();
            var trainTxt = CreateText(trainGo.transform, "Txt", "MOBILIZE UNITS", 18f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            trainTxt.rectTransform.anchorMin = Vector2.zero;
            trainTxt.rectTransform.anchorMax = Vector2.one;
            trainTxt.rectTransform.offsetMin = trainTxt.rectTransform.offsetMax = Vector2.zero;

            // Wire Serialized Properties to UnitDetailView
            var so = new SerializedObject(view);
            so.FindProperty("_panelRoot").objectReferenceValue = detailRootGo;
            so.FindProperty("_titleText").objectReferenceValue = titleTxt;
            so.FindProperty("_unitNameText").objectReferenceValue = nameTxt;
            so.FindProperty("_categoryText").objectReferenceValue = catTxt;
            so.FindProperty("_tierLevelText").objectReferenceValue = tierTxt;
            so.FindProperty("_hpStatText").objectReferenceValue = hpTxt;
            so.FindProperty("_attackStatText").objectReferenceValue = atkTxt;
            so.FindProperty("_defenseStatText").objectReferenceValue = defTxt;
            so.FindProperty("_speedStatText").objectReferenceValue = spdTxt;
            so.FindProperty("_rangeStatText").objectReferenceValue = rngTxt;
            so.FindProperty("_capacityStatText").objectReferenceValue = capTxt;
            so.FindProperty("_powerStatText").objectReferenceValue = pwrTxt;
            so.FindProperty("_upgradeSectionRoot").objectReferenceValue = upgGo;
            so.FindProperty("_nextLevelPerksText").objectReferenceValue = upgPerksTxt;
            so.FindProperty("_upgradeCostText").objectReferenceValue = upgCostTxt;
            so.FindProperty("_upgradeDurationText").objectReferenceValue = upgDurTxt;
            so.FindProperty("_btnTrainUnits").objectReferenceValue = trainBtn;
            so.FindProperty("_btnTrainUnitsText").objectReferenceValue = trainTxt;
            so.FindProperty("_btnClose").objectReferenceValue = closeBtn;
            so.FindProperty("_previewStageAnchor").objectReferenceValue = previewAnchor;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Initially inactive
            detailRootGo.SetActive(false);

            return view;
        }

        private static GameObject CreateSubPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, Color color, FontStyles style, TextAlignmentOptions align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            return tmp;
        }

        private static void Ensure3DTerrainEnvironment()
        {
            var terrainModel = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelsDir}/Terrain/Terrain_WorldWar.obj");
            if (terrainModel == null) return;

            var matTerrain = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/M_Terrain_Ground.mat");

            var baseRoot = FindInScene("PlayerBaseRoot");
            if (baseRoot != null)
            {
                var existingTerrain = baseRoot.transform.Find("Terrain_3D_Base");
                if (existingTerrain == null)
                {
                    var terrGo = new GameObject("Terrain_3D_Base");
                    terrGo.transform.SetParent(baseRoot.transform, false);
                    terrGo.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                    terrGo.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);

                    var mf = terrGo.AddComponent<MeshFilter>();
                    var srcMf = terrainModel.GetComponentInChildren<MeshFilter>();
                    if (srcMf != null) mf.sharedMesh = srcMf.sharedMesh;

                    var mr = terrGo.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = matTerrain;
                }
            }

            var armyRoot = FindInScene("PlayerArmyRoot");
            if (armyRoot != null)
            {
                var existingTerrain = armyRoot.transform.Find("Terrain_3D_Army");
                if (existingTerrain == null)
                {
                    var terrGo = new GameObject("Terrain_3D_Army");
                    terrGo.transform.SetParent(armyRoot.transform, false);
                    terrGo.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                    terrGo.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);

                    var mf = terrGo.AddComponent<MeshFilter>();
                    var srcMf = terrainModel.GetComponentInChildren<MeshFilter>();
                    if (srcMf != null) mf.sharedMesh = srcMf.sharedMesh;

                    var mr = terrGo.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = matTerrain;
                }
            }
        }

        private static GameObject FindInScene(string name)
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root;
                var found = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in found)
                {
                    if (t.name == name) return t.gameObject;
                }
            }
            return null;
        }

        private static void SetObjectProperty(SerializedObject so, string propertyName, Object target)
        {
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = target;
            }
        }
    }
}
