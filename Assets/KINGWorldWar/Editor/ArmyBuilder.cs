using System.IO;
using KingWorldWar.App;
using KingWorldWar.Core.Army;
using KingWorldWar.Features.Army;
using KingWorldWar.Features.Camera;
using KingWorldWar.Infrastructure.Army;
using KingWorldWar.UI.Army;
using KingWorldWar.UI.Common;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.Editor
{
    /// <summary>
    /// Editor builder for the Army & Military Units scene integration (Phase 6).
    /// Instantiates PlayerArmyView at (400, 0, 0), configures ArmyScreenView,
    /// builds RecruitmentDrawer, and wires them into MainMenu.unity.
    /// </summary>
    public static class ArmyBuilder
    {
        private const string ScenePath = "Assets/KINGWorldWar/Scenes/MainMenu.unity";
        private const string ConfigAssetPath = "Assets/KINGWorldWar/Art/Settings/ArmyConfig.asset";
        private const string TankPrefabPath = "Assets/KINGWorldWar/Art/Prefabs/Units/Unit_Tank_Placeholder.prefab";

        private static readonly Color Charcoal = new Color32(0x1F, 0x24, 0x21, 0xFF);
        private static readonly Color SteelGray = new Color32(0x4A, 0x55, 0x68, 0xFF);
        private static readonly Color MilitaryGreen = new Color32(0x3E, 0x4E, 0x3D, 0xFF);
        private static readonly Color Gold = new Color32(0xD4, 0xAF, 0x37, 0xFF);

        [MenuItem("KingWorldWar/Build Army Scene (Phase 6)")]
        public static void BuildArmyIntegration()
        {
            // 1. Ensure ArmyConfig asset exists
            var config = AssetDatabase.LoadAssetAtPath<ArmyConfigSO>(ConfigAssetPath);
            if (config == null)
            {
                var dir = Path.GetDirectoryName(ConfigAssetPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                config = ScriptableObject.CreateInstance<ArmyConfigSO>();
                config.InitializeFromDefaults();
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[ArmyBuilder] Created ArmyConfig asset at {ConfigAssetPath}");
            }

            // 2. Open MainMenu scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var scope = Object.FindAnyObjectByType<MainMenuScope>();
            if (scope == null)
            {
                Debug.LogError("[ArmyBuilder] MainMenuScope not found in MainMenu.unity!");
                return;
            }

            var mainCamera = UnityEngine.Camera.main;
            var cameraRig = Object.FindAnyObjectByType<StrategyCameraRig>();

            // 3. Player Army Root & 3D Staging Ground at (400, 0, 0)
            var armyRootGo = GameObject.Find("PlayerArmyRoot");
            if (armyRootGo == null)
            {
                armyRootGo = new GameObject("PlayerArmyRoot");
            }
            armyRootGo.transform.position = new Vector3(400f, 0f, 0f);

            var armyView = armyRootGo.GetComponent<PlayerArmyView>() ?? armyRootGo.AddComponent<PlayerArmyView>();

            var containerGo = armyRootGo.transform.Find("UnitsContainer");
            if (containerGo == null)
            {
                var cont = new GameObject("UnitsContainer");
                cont.transform.SetParent(armyRootGo.transform, false);
                containerGo = cont.transform;
            }

            var tankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TankPrefabPath);

            var armyViewSo = new SerializedObject(armyView);
            armyViewSo.FindProperty("_worldCamera").objectReferenceValue = mainCamera;
            armyViewSo.FindProperty("_cameraRig").objectReferenceValue = cameraRig;
            armyViewSo.FindProperty("_unitsContainer").objectReferenceValue = containerGo;
            armyViewSo.FindProperty("_tankPrefab").objectReferenceValue = tankPrefab;
            armyViewSo.ApplyModifiedPropertiesWithoutUndo();

            // 4. Configure Screen_Army UI
            var screenArmyGo = GameObject.Find("Screen_Army");
            if (screenArmyGo != null)
            {
                var screenImg = screenArmyGo.GetComponent<Image>();
                if (screenImg != null)
                {
                    screenImg.color = new Color(0, 0, 0, 0);
                    screenImg.raycastTarget = false;
                }

                // Hide old placeholder text if present
                var oldTitle = screenArmyGo.transform.Find("Title");
                if (oldTitle != null) oldTitle.gameObject.SetActive(false);
                var oldDesc = screenArmyGo.transform.Find("Description");
                if (oldDesc != null) oldDesc.gameObject.SetActive(false);
            }

            // 5. Build ArmyScreenView & RecruitmentDrawer
            var safeAreaGo = GameObject.Find("SafeAreaContainer");
            var drawerGo = GameObject.Find("RecruitmentDrawer");
            if (drawerGo == null && safeAreaGo != null)
            {
                drawerGo = BuildRecruitmentDrawerUI(safeAreaGo.transform);
            }
            var drawer = drawerGo != null ? drawerGo.GetComponent<RecruitmentDrawer>() : null;

            var armyScreenView = screenArmyGo != null ? screenArmyGo.GetComponent<ArmyScreenView>() : null;
            if (armyScreenView == null && screenArmyGo != null)
            {
                armyScreenView = BuildArmyScreenDashboard(screenArmyGo, drawer);
            }

            // 6. Wire MainMenuScope
            var scopeSo = new SerializedObject(scope);
            scopeSo.FindProperty("_playerArmyView").objectReferenceValue = armyView;
            scopeSo.FindProperty("_armyScreenView").objectReferenceValue = armyScreenView;
            scopeSo.FindProperty("_recruitmentDrawer").objectReferenceValue = drawer;
            scopeSo.ApplyModifiedPropertiesWithoutUndo();

            // 7. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ArmyBuilder] MainMenu.unity updated with Army & Military Units features and saved successfully!");
        }

        private static ArmyScreenView BuildArmyScreenDashboard(GameObject screenArmyGo, RecruitmentDrawer drawer)
        {
            var view = screenArmyGo.AddComponent<ArmyScreenView>();

            // Top Header Bar
            var headerGo = new GameObject("ArmyDashboardHeader");
            headerGo.transform.SetParent(screenArmyGo.transform, false);
            var headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.85f);
            headerRect.anchorMax = new Vector2(1f, 0.95f);
            headerRect.offsetMin = new Vector2(24f, 0f);
            headerRect.offsetMax = new Vector2(-24f, 0f);

            var headerBg = headerGo.AddComponent<Image>();
            headerBg.color = new Color(0.12f, 0.14f, 0.13f, 0.85f);

            // Title
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.02f, 0.5f);
            titleRect.anchorMax = new Vector2(0.35f, 0.95f);
            titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.fontSize = 28f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Gold;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            // Combat Power Stat
            var pwrGo = new GameObject("CombatPowerStat");
            pwrGo.transform.SetParent(headerGo.transform, false);
            var pwrRect = pwrGo.AddComponent<RectTransform>();
            pwrRect.anchorMin = new Vector2(0.38f, 0.1f);
            pwrRect.anchorMax = new Vector2(0.68f, 0.9f);
            pwrRect.offsetMin = pwrRect.offsetMax = Vector2.zero;

            var pwrLbl = CreateText(pwrGo.transform, "Label", "Combat Power", 14f, SteelGray, TextAlignmentOptions.TopLeft, new Vector2(0f, 0.5f), new Vector2(1f, 1f));
            var pwrVal = CreateText(pwrGo.transform, "Value", "0", 22f, Gold, TextAlignmentOptions.BottomLeft, new Vector2(0f, 0f), new Vector2(1f, 0.55f));

            // Total Troops Stat
            var trpsGo = new GameObject("TotalTroopsStat");
            trpsGo.transform.SetParent(headerGo.transform, false);
            var trpsRect = trpsGo.AddComponent<RectTransform>();
            trpsRect.anchorMin = new Vector2(0.70f, 0.1f);
            trpsRect.anchorMax = new Vector2(0.98f, 0.9f);
            trpsRect.offsetMin = trpsRect.offsetMax = Vector2.zero;

            var trpsLbl = CreateText(trpsGo.transform, "Label", "Forces", 14f, SteelGray, TextAlignmentOptions.TopLeft, new Vector2(0f, 0.5f), new Vector2(1f, 1f));
            var trpsVal = CreateText(trpsGo.transform, "Value", "0", 22f, Color.white, TextAlignmentOptions.BottomLeft, new Vector2(0f, 0f), new Vector2(1f, 0.55f));

            // Unit Cards Container (Grid / Horizontal shelf at bottom right)
            var shelfGo = new GameObject("UnitCardsShelf");
            shelfGo.transform.SetParent(screenArmyGo.transform, false);
            var shelfRect = shelfGo.AddComponent<RectTransform>();
            shelfRect.anchorMin = new Vector2(0.02f, 0.12f);
            shelfRect.anchorMax = new Vector2(0.98f, 0.28f);
            shelfRect.offsetMin = shelfRect.offsetMax = Vector2.zero;

            var shelfLayout = shelfGo.AddComponent<HorizontalLayoutGroup>();
            shelfLayout.spacing = 16f;
            shelfLayout.childControlWidth = true;
            shelfLayout.childControlHeight = true;
            shelfLayout.childForceExpandWidth = true;
            shelfLayout.childForceExpandHeight = true;

            var infCard = BuildUnitCard(shelfGo.transform, UnitType.Infantry);
            var reconCard = BuildUnitCard(shelfGo.transform, UnitType.ReconVehicle);
            var tankCard = BuildUnitCard(shelfGo.transform, UnitType.Tank);
            var artCard = BuildUnitCard(shelfGo.transform, UnitType.Artillery);

            var so = new SerializedObject(view);
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_combatPowerLabel").objectReferenceValue = pwrLbl;
            so.FindProperty("_combatPowerValue").objectReferenceValue = pwrVal;
            so.FindProperty("_totalTroopsLabel").objectReferenceValue = trpsLbl;
            so.FindProperty("_totalTroopsValue").objectReferenceValue = trpsVal;
            so.FindProperty("_recruitmentDrawer").objectReferenceValue = drawer;

            SetCardProperties(so.FindProperty("_infantryCard"), infCard);
            SetCardProperties(so.FindProperty("_reconCard"), reconCard);
            SetCardProperties(so.FindProperty("_tankCard"), tankCard);
            SetCardProperties(so.FindProperty("_artilleryCard"), artCard);

            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static ArmyScreenView.UnitCardUI BuildUnitCard(Transform parent, UnitType type)
        {
            var cardGo = new GameObject($"Card_{type}");
            cardGo.transform.SetParent(parent, false);

            var bg = cardGo.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.17f, 0.15f, 0.9f);

            var card = new ArmyScreenView.UnitCardUI { Type = type };

            var nameTxt = CreateText(cardGo.transform, "Name", type.ToString(), 16f, Color.white, TextAlignmentOptions.TopLeft, new Vector2(0.08f, 0.65f), new Vector2(0.92f, 0.92f));
            nameTxt.fontStyle = FontStyles.Bold;
            card.NameText = nameTxt;

            var countTxt = CreateText(cardGo.transform, "Count", "Mobilized: 0", 14f, Gold, TextAlignmentOptions.MidlineLeft, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.65f));
            card.CountText = countTxt;

            // Recruit Button
            var btnGo = new GameObject("BtnRecruit");
            btnGo.transform.SetParent(cardGo.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.08f, 0.08f);
            btnRect.anchorMax = new Vector2(0.92f, 0.35f);
            btnRect.offsetMin = btnRect.offsetMax = Vector2.zero;

            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = MilitaryGreen;
            var btn = btnGo.AddComponent<Button>();
            btnGo.AddComponent<KwwButton>();
            card.RecruitButton = btn;

            var btnTxt = CreateText(btnGo.transform, "Text", "Recruit", 14f, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            card.RecruitButtonText = btnTxt;

            return card;
        }

        private static void SetCardProperties(SerializedProperty prop, ArmyScreenView.UnitCardUI card)
        {
            if (prop == null || card == null) return;
            prop.FindPropertyRelative("Type").enumValueIndex = (int)card.Type - 1;
            prop.FindPropertyRelative("NameText").objectReferenceValue = card.NameText;
            prop.FindPropertyRelative("CountText").objectReferenceValue = card.CountText;
            prop.FindPropertyRelative("RecruitButton").objectReferenceValue = card.RecruitButton;
            prop.FindPropertyRelative("RecruitButtonText").objectReferenceValue = card.RecruitButtonText;
        }

        private static GameObject BuildRecruitmentDrawerUI(Transform parent)
        {
            var drawerRoot = new GameObject("RecruitmentDrawer");
            drawerRoot.transform.SetParent(parent, false);
            var drawer = drawerRoot.AddComponent<RecruitmentDrawer>();

            var drawerRect = drawerRoot.AddComponent<RectTransform>();
            drawerRect.anchorMin = new Vector2(0.60f, 0.12f);
            drawerRect.anchorMax = new Vector2(0.98f, 0.88f);
            drawerRect.offsetMin = drawerRect.offsetMax = Vector2.zero;

            var bgImg = drawerRoot.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.15f, 0.13f, 0.95f);

            // Title
            var titleText = CreateText(drawerRoot.transform, "TitleText", "Mobilization", 22f, Gold, TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.92f), new Vector2(0.85f, 0.98f));
            titleText.fontStyle = FontStyles.Bold;

            // Close button
            var closeGo = new GameObject("BtnClose");
            closeGo.transform.SetParent(drawerRoot.transform, false);
            var closeRect = closeGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.88f, 0.92f);
            closeRect.anchorMax = new Vector2(0.96f, 0.98f);
            closeRect.offsetMin = closeRect.offsetMax = Vector2.zero;
            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            var closeBtn = closeGo.AddComponent<Button>();
            closeGo.AddComponent<KwwButton>();
            CreateText(closeGo.transform, "X", "✕", 18f, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);

            // Role & Description
            var roleText = CreateText(drawerRoot.transform, "RoleText", "Frontline Combat", 16f, SteelGray, TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.92f));
            var descText = CreateText(drawerRoot.transform, "DescText", "Tactical frontline unit...", 13f, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.86f));

            // Stats grid (HP, ATK, DEF, PWR)
            var statsGo = new GameObject("StatsGrid");
            statsGo.transform.SetParent(drawerRoot.transform, false);
            var statsRect = statsGo.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.05f, 0.52f);
            statsRect.anchorMax = new Vector2(0.95f, 0.70f);
            statsRect.offsetMin = statsRect.offsetMax = Vector2.zero;

            var hpText = CreateText(statsGo.transform, "HP", "HP: 120", 14f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.5f), new Vector2(0.5f, 1f));
            var atkText = CreateText(statsGo.transform, "ATK", "ATK: 25", 14f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0.5f, 0.5f), new Vector2(1f, 1f));
            var defText = CreateText(statsGo.transform, "DEF", "DEF: 15", 14f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f));
            var pwrText = CreateText(statsGo.transform, "PWR", "PWR: 40", 14f, Gold, TextAlignmentOptions.MidlineLeft, new Vector2(0.5f, 0f), new Vector2(1f, 0.5f));

            // Quantity Stepper
            var stepperGo = new GameObject("QuantityStepper");
            stepperGo.transform.SetParent(drawerRoot.transform, false);
            var stepRect = stepperGo.AddComponent<RectTransform>();
            stepRect.anchorMin = new Vector2(0.05f, 0.42f);
            stepRect.anchorMax = new Vector2(0.95f, 0.50f);
            stepRect.offsetMin = stepRect.offsetMax = Vector2.zero;

            var btnDec = CreateButton(stepperGo.transform, "BtnDec", "-", new Vector2(0f, 0f), new Vector2(0.25f, 1f), SteelGray);
            var qtyTxt = CreateText(stepperGo.transform, "QtyText", "x10", 18f, Gold, TextAlignmentOptions.Center, new Vector2(0.28f, 0f), new Vector2(0.60f, 1f));
            var btnInc = CreateButton(stepperGo.transform, "BtnInc", "+", new Vector2(0.63f, 0f), new Vector2(0.80f, 1f), SteelGray);
            var btnMax = CreateButton(stepperGo.transform, "BtnMax", "MAX", new Vector2(0.83f, 0f), new Vector2(1f, 1f), SteelGray);

            // Costs Container
            var costsGo = new GameObject("CostsContainer");
            costsGo.transform.SetParent(drawerRoot.transform, false);
            var costsRect = costsGo.AddComponent<RectTransform>();
            costsRect.anchorMin = new Vector2(0.05f, 0.22f);
            costsRect.anchorMax = new Vector2(0.95f, 0.40f);
            costsRect.offsetMin = costsRect.offsetMax = Vector2.zero;

            var goldTxt = CreateText(costsGo.transform, "Gold", "Gold: 50", 14f, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.6f), new Vector2(0.5f, 1f));
            var oilTxt = CreateText(costsGo.transform, "Oil", "Oil: 10", 14f, new Color(0.4f, 0.7f, 1f), TextAlignmentOptions.MidlineLeft, new Vector2(0.5f, 0.6f), new Vector2(1f, 1f));
            var ironTxt = CreateText(costsGo.transform, "Iron", "Iron: 20", 14f, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0.2f), new Vector2(0.5f, 0.6f));
            var durTxt = CreateText(costsGo.transform, "Time", "Time: 00:15", 14f, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0.5f, 0.2f), new Vector2(1f, 0.6f));

            var prereqTxt = CreateText(costsGo.transform, "Prereq", "✔ Prerequisites Satisfied", 13f, new Color(0.2f, 0.8f, 0.4f), TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0f), new Vector2(1f, 0.2f));

            // Recruit Action Button
            var recruitBtn = CreateButton(drawerRoot.transform, "BtnRecruit", "Commence Mobilization", new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.18f), MilitaryGreen);
            var recruitBtnTxt = recruitBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Cancel Button
            var cancelBtn = CreateButton(drawerRoot.transform, "BtnCancel", "Cancel Mobilization", new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.18f), new Color(0.7f, 0.2f, 0.2f));
            cancelBtn.gameObject.SetActive(false);

            // Training Status Root
            var trainGo = new GameObject("TrainingProgressRoot");
            trainGo.transform.SetParent(drawerRoot.transform, false);
            var trainRect = trainGo.AddComponent<RectTransform>();
            trainRect.anchorMin = new Vector2(0.05f, 0.22f);
            trainRect.anchorMax = new Vector2(0.95f, 0.35f);
            trainRect.offsetMin = trainRect.offsetMax = Vector2.zero;

            var countdownTxt = CreateText(trainGo.transform, "Countdown", "Time Remaining: 00:45", 16f, Gold, TextAlignmentOptions.Center, new Vector2(0f, 0.4f), new Vector2(1f, 1f));

            var barBg = new GameObject("ProgressBarBg");
            barBg.transform.SetParent(trainGo.transform, false);
            var barBgRect = barBg.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0f, 0.05f);
            barBgRect.anchorMax = new Vector2(1f, 0.35f);
            barBgRect.offsetMin = barBgRect.offsetMax = Vector2.zero;
            var barBgImg = barBg.AddComponent<Image>();
            barBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var barFill = new GameObject("ProgressBarFill");
            barFill.transform.SetParent(barBg.transform, false);
            var barFillRect = barFill.AddComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = Vector2.one;
            barFillRect.offsetMin = barFillRect.offsetMax = Vector2.zero;
            var barFillImg = barFill.AddComponent<Image>();
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.color = new Color(0.2f, 0.8f, 0.4f, 1f);

            trainGo.SetActive(false);

            // Setup serialized fields on drawer
            var dSo = new SerializedObject(drawer);
            dSo.FindProperty("_panelRoot").objectReferenceValue = drawerRoot;
            dSo.FindProperty("_unitNameText").objectReferenceValue = titleText;
            dSo.FindProperty("_roleText").objectReferenceValue = roleText;
            dSo.FindProperty("_descriptionText").objectReferenceValue = descText;
            dSo.FindProperty("_hpText").objectReferenceValue = hpText;
            dSo.FindProperty("_attackText").objectReferenceValue = atkText;
            dSo.FindProperty("_defenseText").objectReferenceValue = defText;
            dSo.FindProperty("_powerText").objectReferenceValue = pwrText;
            dSo.FindProperty("_btnDecrement").objectReferenceValue = btnDec;
            dSo.FindProperty("_btnIncrement").objectReferenceValue = btnInc;
            dSo.FindProperty("_btnMax").objectReferenceValue = btnMax;
            dSo.FindProperty("_quantityText").objectReferenceValue = qtyTxt;
            dSo.FindProperty("_costsRoot").objectReferenceValue = costsGo;
            dSo.FindProperty("_costGoldText").objectReferenceValue = goldTxt;
            dSo.FindProperty("_costOilText").objectReferenceValue = oilTxt;
            dSo.FindProperty("_costIronText").objectReferenceValue = ironTxt;
            dSo.FindProperty("_durationText").objectReferenceValue = durTxt;
            dSo.FindProperty("_prerequisiteText").objectReferenceValue = prereqTxt;
            dSo.FindProperty("_btnRecruit").objectReferenceValue = recruitBtn;
            dSo.FindProperty("_btnRecruitText").objectReferenceValue = recruitBtnTxt;
            dSo.FindProperty("_btnCancelRecruit").objectReferenceValue = cancelBtn;
            dSo.FindProperty("_btnClose").objectReferenceValue = closeBtn;
            dSo.FindProperty("_trainingStatusRoot").objectReferenceValue = trainGo;
            dSo.FindProperty("_countdownText").objectReferenceValue = countdownTxt;
            dSo.FindProperty("_progressBarFill").objectReferenceValue = barFillImg;
            dSo.ApplyModifiedPropertiesWithoutUndo();

            drawerRoot.SetActive(false);
            return drawerRoot;
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string name,
            string text,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            return tmp;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string text,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            go.AddComponent<KwwButton>();
            CreateText(go.transform, "Text", text, 15f, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            return btn;
        }
    }
}
