using System.IO;
using KingWorldWar.App;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Scenes;
using KingWorldWar.Core.UI;
using KingWorldWar.UI.Common;
using KingWorldWar.UI.Modals;
using KingWorldWar.UI.Shell;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KingWorldWar.Editor
{
    /// <summary>
    /// Automated builder for the KING WORLD WAR Main Menu shell scene (Phase 3).
    /// Constructs UI hierarchy conforming to mobile safe-area, 48dp touch targets, and WCAG military palette.
    /// </summary>
    public static class MainMenuBuilder
    {
        private const string ScenePath = "Assets/KINGWorldWar/Scenes/MainMenu.unity";

        private static readonly Color Charcoal = new Color32(0x1F, 0x24, 0x21, 0xFF);
        private static readonly Color SteelGray = new Color32(0x4A, 0x55, 0x68, 0xFF);
        private static readonly Color MilitaryGreen = new Color32(0x3E, 0x4E, 0x3D, 0xFF);
        private static readonly Color Gold = new Color32(0xD4, 0xAF, 0x37, 0xFF);

        [MenuItem("KingWorldWar/Build Main Menu Scene (Phase 3)")]
        public static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera & Lighting
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Charcoal;
            camGo.transform.position = new Vector3(0, 0, -10);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.0f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 2. Event System (Input System UI module)
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();

            // 3. Root Canvas
            var canvasGo = new GameObject("MainMenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 4. Safe Area Container
            var safeAreaGo = new GameObject("SafeAreaContainer");
            safeAreaGo.transform.SetParent(canvasGo.transform, false);
            var safeAreaRect = safeAreaGo.AddComponent<RectTransform>();
            SetStretch(safeAreaRect);
            safeAreaGo.AddComponent<SafeAreaFitter>();

            // 5. Top Bar (Resource Header)
            var topBarGo = new GameObject("TopBar_ResourceHeader");
            topBarGo.transform.SetParent(safeAreaGo.transform, false);
            var topBarRect = topBarGo.AddComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 1f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.pivot = new Vector2(0.5f, 1f);
            topBarRect.sizeDelta = new Vector2(0f, 120f);
            topBarRect.anchoredPosition = Vector2.zero;

            var topBarBg = topBarGo.AddComponent<Image>();
            topBarBg.color = new Color(0.12f, 0.14f, 0.13f, 0.95f);

            var resHeader = topBarGo.AddComponent<ResourceHeaderView>();
            var hLayout = topBarGo.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = false;
            hLayout.spacing = 20;
            hLayout.padding = new RectOffset(30, 30, 10, 10);

            var goldText = CreateResourceItem(topBarGo.transform, "Gold", "GOLD", Gold);
            var oilText = CreateResourceItem(topBarGo.transform, "Oil", "OIL", SteelGray);
            var ironText = CreateResourceItem(topBarGo.transform, "Iron", "IRON", new Color(0.7f, 0.7f, 0.75f));

            // Wire ResourceHeaderView serialized fields via SerializedObject
            var resHeaderSo = new SerializedObject(resHeader);
            resHeaderSo.FindProperty("_goldText").objectReferenceValue = goldText;
            resHeaderSo.FindProperty("_oilText").objectReferenceValue = oilText;
            resHeaderSo.FindProperty("_ironText").objectReferenceValue = ironText;
            resHeaderSo.ApplyModifiedPropertiesWithoutUndo();

            // 6. Content Container with 6 placeholder screens
            var contentGo = new GameObject("ScreenContentContainer");
            contentGo.transform.SetParent(safeAreaGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(20f, 160f); // above bottom bar
            contentRect.offsetMax = new Vector2(-20f, -140f); // below top bar

            ScreenId[] screenIds = {
                ScreenId.World,
                ScreenId.Base,
                ScreenId.Army,
                ScreenId.Alliance,
                ScreenId.Rankings,
                ScreenId.Profile
            };

            PlaceholderScreenView[] screenViews = new PlaceholderScreenView[screenIds.Length];
            for (int i = 0; i < screenIds.Length; i++)
            {
                screenViews[i] = CreatePlaceholderScreen(contentGo.transform, screenIds[i]);
                screenViews[i].gameObject.SetActive(i == 0); // start with World active
            }

            // 7. Bottom Navigation Bar
            var bottomBarGo = new GameObject("BottomBar_Navigation");
            bottomBarGo.transform.SetParent(safeAreaGo.transform, false);
            var bottomBarRect = bottomBarGo.AddComponent<RectTransform>();
            bottomBarRect.anchorMin = new Vector2(0f, 0f);
            bottomBarRect.anchorMax = new Vector2(1f, 0f);
            bottomBarRect.pivot = new Vector2(0.5f, 0f);
            bottomBarRect.sizeDelta = new Vector2(0f, 150f);
            bottomBarRect.anchoredPosition = Vector2.zero;

            var bottomBarBg = bottomBarGo.AddComponent<Image>();
            bottomBarBg.color = new Color(0.12f, 0.14f, 0.13f, 0.98f);

            var bottomLayout = bottomBarGo.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.childAlignment = TextAnchor.MiddleCenter;
            bottomLayout.childControlWidth = true;
            bottomLayout.childControlHeight = true;
            bottomLayout.childForceExpandWidth = true;
            bottomLayout.childForceExpandHeight = false;
            bottomLayout.spacing = 10;
            bottomLayout.padding = new RectOffset(20, 20, 15, 15);

            // Back button
            var btnBack = CreateNavButton(bottomBarGo.transform, "Btn_Back", "←", 70f, ButtonStyleType.Secondary);
            btnBack.gameObject.SetActive(false);

            // 6 Tab Buttons
            var btnWorld = CreateNavButton(bottomBarGo.transform, "Btn_World", "World", 130f, ButtonStyleType.Primary, "nav.world");
            var btnBase = CreateNavButton(bottomBarGo.transform, "Btn_Base", "Base", 130f, ButtonStyleType.Primary, "nav.base");
            var btnArmy = CreateNavButton(bottomBarGo.transform, "Btn_Army", "Army", 130f, ButtonStyleType.Primary, "nav.army");
            var btnAlliance = CreateNavButton(bottomBarGo.transform, "Btn_Alliance", "Alliance", 130f, ButtonStyleType.Primary, "nav.alliance");
            var btnRankings = CreateNavButton(bottomBarGo.transform, "Btn_Rankings", "Rankings", 130f, ButtonStyleType.Primary, "nav.rankings");
            var btnProfile = CreateNavButton(bottomBarGo.transform, "Btn_Profile", "Profile", 130f, ButtonStyleType.Primary, "nav.profile");

            // Language Toggle Button
            var btnLang = CreateNavButton(bottomBarGo.transform, "Btn_LanguageToggle", "عربي", 90f, ButtonStyleType.Secondary);
            var langText = btnLang.GetComponentInChildren<TextMeshProUGUI>();

            // 8. Modal Dialog View (Highest Canvas layer)
            var modalView = CreateModalDialog(canvasGo.transform);

            // 9. MainMenuRoot with Scope and ShellView
            var rootGo = new GameObject("MainMenuRoot");
            var shellView = rootGo.AddComponent<MainMenuShellView>();
            var shellSo = new SerializedObject(shellView);
            shellSo.FindProperty("_btnWorld").objectReferenceValue = btnWorld;
            shellSo.FindProperty("_btnBase").objectReferenceValue = btnBase;
            shellSo.FindProperty("_btnArmy").objectReferenceValue = btnArmy;
            shellSo.FindProperty("_btnAlliance").objectReferenceValue = btnAlliance;
            shellSo.FindProperty("_btnRankings").objectReferenceValue = btnRankings;
            shellSo.FindProperty("_btnProfile").objectReferenceValue = btnProfile;
            shellSo.FindProperty("_btnBack").objectReferenceValue = btnBack;
            shellSo.FindProperty("_btnLanguageToggle").objectReferenceValue = btnLang;
            shellSo.FindProperty("_languageText").objectReferenceValue = langText;

            var screensProp = shellSo.FindProperty("_screens");
            screensProp.arraySize = screenViews.Length;
            for (int i = 0; i < screenViews.Length; i++)
            {
                screensProp.GetArrayElementAtIndex(i).objectReferenceValue = screenViews[i];
            }
            shellSo.ApplyModifiedPropertiesWithoutUndo();

            var scope = rootGo.AddComponent<MainMenuScope>();
            var scopeSo = new SerializedObject(scope);
            scopeSo.FindProperty("_shellView").objectReferenceValue = shellView;
            scopeSo.FindProperty("_modalView").objectReferenceValue = modalView;
            scopeSo.FindProperty("_resourceHeaderView").objectReferenceValue = resHeader;
            scopeSo.ApplyModifiedPropertiesWithoutUndo();

            // Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[MainMenuBuilder] MainMenu.unity built and saved successfully!");
        }

        private static TextMeshProUGUI CreateResourceItem(Transform parent, string name, string label, Color color)
        {
            var itemGo = new GameObject($"Res_{name}");
            itemGo.transform.SetParent(parent, false);
            var layout = itemGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(itemGo.transform, false);
            var lblTmp = labelGo.AddComponent<TextMeshProUGUI>();
            lblTmp.text = label;
            lblTmp.fontSize = 18;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.color = color;
            lblTmp.alignment = TextAlignmentOptions.Center;

            var valGo = new GameObject("Value");
            valGo.transform.SetParent(itemGo.transform, false);
            var valTmp = valGo.AddComponent<TextMeshProUGUI>();
            valTmp.text = "0";
            valTmp.fontSize = 26;
            valTmp.color = Color.white;
            valTmp.alignment = TextAlignmentOptions.Center;

            return valTmp;
        }

        private static PlaceholderScreenView CreatePlaceholderScreen(Transform parent, ScreenId screenId)
        {
            var screenGo = new GameObject($"Screen_{screenId}");
            screenGo.transform.SetParent(parent, false);
            var rect = screenGo.AddComponent<RectTransform>();
            SetStretch(rect);

            var bg = screenGo.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.17f, 0.15f, 0.95f);

            var layout = screenGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 30;
            layout.padding = new RectOffset(60, 60, 80, 80);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;

            // Phase Badge
            var badgeGo = new GameObject("PhaseBadge");
            badgeGo.transform.SetParent(screenGo.transform, false);
            var badgeTmp = badgeGo.AddComponent<TextMeshProUGUI>();
            badgeTmp.text = $"[PLACEHOLDER - {screenId}]";
            badgeTmp.fontSize = 22;
            badgeTmp.fontStyle = FontStyles.Bold;
            badgeTmp.color = Gold;
            badgeTmp.alignment = TextAlignmentOptions.Center;

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(screenGo.transform, false);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = screenId.ToString();
            titleTmp.fontSize = 48;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Description
            var descGo = new GameObject("Description");
            descGo.transform.SetParent(screenGo.transform, false);
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.text = $"Detailed tactical gameplay systems for {screenId} will unlock in future milestone phases.";
            descTmp.fontSize = 26;
            descTmp.color = new Color(0.8f, 0.82f, 0.85f);
            descTmp.alignment = TextAlignmentOptions.Center;

            var view = screenGo.AddComponent<PlaceholderScreenView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("_screenId").enumValueIndex = (int)screenId;
            viewSo.FindProperty("_titleText").objectReferenceValue = titleTmp;
            viewSo.FindProperty("_descText").objectReferenceValue = descTmp;
            viewSo.FindProperty("_phaseBadgeText").objectReferenceValue = badgeTmp;
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static Button CreateNavButton(Transform parent, string name, string defaultText, float minWidth, ButtonStyleType style, string locKey = null)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(minWidth, 80f);

            var img = btnGo.AddComponent<Image>();
            img.color = (style == ButtonStyleType.Primary) ? MilitaryGreen : SteelGray;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;

            var kwwBtn = btnGo.AddComponent<KwwButton>();
            var kwwSo = new SerializedObject(kwwBtn);
            kwwSo.FindProperty("_style").enumValueIndex = (int)style;
            kwwSo.FindProperty("_enforceMinTouchTarget").boolValue = true;
            kwwSo.ApplyModifiedPropertiesWithoutUndo();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            SetStretch(textRect);

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            if (!string.IsNullOrEmpty(locKey))
            {
                var locText = textGo.AddComponent<LocalizedText>();
                var ltSo = new SerializedObject(locText);
                ltSo.FindProperty("_key").stringValue = locKey;
                ltSo.FindProperty("_autoAlignRTL").boolValue = true;
                ltSo.ApplyModifiedPropertiesWithoutUndo();
            }

            return btn;
        }

        private static ModalDialogView CreateModalDialog(Transform parent)
        {
            var modalOverlayGo = new GameObject("ModalDialogOverlay");
            modalOverlayGo.transform.SetParent(parent, false);
            var overlayRect = modalOverlayGo.AddComponent<RectTransform>();
            SetStretch(overlayRect);

            // Backdrop (click to dismiss alerts, blocks all raycasts)
            var backdropGo = new GameObject("Backdrop");
            backdropGo.transform.SetParent(modalOverlayGo.transform, false);
            var backdropRect = backdropGo.AddComponent<RectTransform>();
            SetStretch(backdropRect);
            var backdropImg = backdropGo.AddComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.75f);
            var backdropBtn = backdropGo.AddComponent<Button>();

            // Dialog Panel
            var panelGo = new GameObject("DialogPanel");
            panelGo.transform.SetParent(modalOverlayGo.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(760f, 480f);

            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = Charcoal;

            var vLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(40, 40, 40, 40);
            vLayout.spacing = 25;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Alert";
            titleTmp.fontSize = 36;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Gold;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Message
            var msgGo = new GameObject("Message");
            msgGo.transform.SetParent(panelGo.transform, false);
            var msgTmp = msgGo.AddComponent<TextMeshProUGUI>();
            msgTmp.text = "Dialog message details go here.";
            msgTmp.fontSize = 24;
            msgTmp.color = Color.white;
            msgTmp.alignment = TextAlignmentOptions.Center;

            // Buttons Container
            var btnRowGo = new GameObject("ButtonRow");
            btnRowGo.transform.SetParent(panelGo.transform, false);
            var rowRect = btnRowGo.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 90f);
            var hRow = btnRowGo.AddComponent<HorizontalLayoutGroup>();
            hRow.childAlignment = TextAnchor.MiddleCenter;
            hRow.spacing = 30;
            hRow.childControlWidth = true;
            hRow.childControlHeight = true;
            hRow.childForceExpandWidth = true;

            var btnConfirm = CreateNavButton(btnRowGo.transform, "Btn_Confirm", "Confirm", 220f, ButtonStyleType.Primary);
            var btnConfirmText = btnConfirm.GetComponentInChildren<TextMeshProUGUI>();

            var btnCancel = CreateNavButton(btnRowGo.transform, "Btn_Cancel", "Cancel", 220f, ButtonStyleType.Secondary);
            var btnCancelText = btnCancel.GetComponentInChildren<TextMeshProUGUI>();

            var modalView = modalOverlayGo.AddComponent<ModalDialogView>();
            var mvSo = new SerializedObject(modalView);
            mvSo.FindProperty("_dialogRoot").objectReferenceValue = modalOverlayGo;
            mvSo.FindProperty("_backdropButton").objectReferenceValue = backdropBtn;
            mvSo.FindProperty("_titleText").objectReferenceValue = titleTmp;
            mvSo.FindProperty("_messageText").objectReferenceValue = msgTmp;
            mvSo.FindProperty("_confirmButton").objectReferenceValue = btnConfirm;
            mvSo.FindProperty("_confirmButtonText").objectReferenceValue = btnConfirmText;
            mvSo.FindProperty("_cancelButton").objectReferenceValue = btnCancel;
            mvSo.FindProperty("_cancelButtonText").objectReferenceValue = btnCancelText;
            mvSo.ApplyModifiedPropertiesWithoutUndo();

            modalOverlayGo.SetActive(true); // Awake handles initial close
            return modalView;
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
