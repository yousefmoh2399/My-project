using System;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.UI;
using KingWorldWar.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingWorldWar.UI.Shell
{
    /// <summary>
    /// Master UI shell managing bottom tab navigation, back button routing,
    /// language switching, and placeholder screen registration.
    /// </summary>
    public class MainMenuShellView : MonoBehaviour
    {
        [Header("Navigation Buttons")]
        [SerializeField] private Button _btnWorld;
        [SerializeField] private Button _btnBase;
        [SerializeField] private Button _btnArmy;
        [SerializeField] private Button _btnAlliance;
        [SerializeField] private Button _btnRankings;
        [SerializeField] private Button _btnProfile;
        [SerializeField] private Button _btnBack;
        [SerializeField] private Button _btnLanguageToggle;
        [SerializeField] private TextMeshProUGUI _languageText;

        [Header("Screens")]
        [SerializeField] private PlaceholderScreenView[] _screens;

        private IScreenNavigationService _nav;
        private ILocalizationService _loc;
        private IModalService _modal;

        public IScreenNavigationService Navigation => _nav;
        public ILocalizationService Localization => _loc;

        public void BindServices(IScreenNavigationService nav, ILocalizationService loc, IModalService modal)
        {
            _nav = nav;
            _loc = loc;
            _modal = modal;

            if (_nav != null)
            {
                _nav.OnScreenChanged += OnScreenChanged;

                if (_screens != null)
                {
                    foreach (var s in _screens)
                    {
                        if (s != null)
                        {
                            _nav.RegisterScreen(s.ScreenId, s);
                        }
                    }
                }
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += OnLanguageChanged;
                RefreshLanguageButton();
            }

            // Start on World
            _nav?.NavigateTo(ScreenId.World);
            UpdateBackVisibility();
        }

        private void Awake()
        {
            if (_btnWorld != null) _btnWorld.onClick.AddListener(() => OnNavClicked(ScreenId.World));
            if (_btnBase != null) _btnBase.onClick.AddListener(() => OnNavClicked(ScreenId.Base));
            if (_btnArmy != null) _btnArmy.onClick.AddListener(() => OnNavClicked(ScreenId.Army));
            if (_btnAlliance != null) _btnAlliance.onClick.AddListener(() => OnNavClicked(ScreenId.Alliance));
            if (_btnRankings != null) _btnRankings.onClick.AddListener(() => OnNavClicked(ScreenId.Rankings));
            if (_btnProfile != null) _btnProfile.onClick.AddListener(() => OnNavClicked(ScreenId.Profile));

            if (_btnBack != null) _btnBack.onClick.AddListener(OnBackClicked);
            if (_btnLanguageToggle != null) _btnLanguageToggle.onClick.AddListener(OnLanguageToggleClicked);
        }

        private void Update()
        {
            // Android back button / Keyboard Escape handling
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                HandleBackTrigger();
            }
        }

        public void HandleBackTrigger()
        {
            if (_modal != null && _modal.IsModalOpen)
            {
                _modal.DismissCurrent();
                return;
            }

            if (_nav != null && _nav.CanGoBack)
            {
                _nav.GoBack();
            }
            else
            {
                // At root: prompt confirmation to exit or alert
                _modal?.ShowConfirm(
                    _loc?.GetString("dialog.confirm_title") ?? "Exit",
                    _loc?.GetString("dialog.confirm_exit_msg") ?? "Are you sure you want to exit?",
                    confirmText: _loc?.GetString("common.confirm") ?? "Confirm",
                    cancelText: _loc?.GetString("common.cancel") ?? "Cancel"
                );
            }
        }

        private void OnNavClicked(ScreenId id)
        {
            // If tapping different tab, push screen
            _nav?.Push(id);
        }

        private void OnBackClicked()
        {
            _nav?.GoBack();
        }

        private void OnLanguageToggleClicked()
        {
            if (_loc == null) return;
            Language target = (_loc.CurrentLanguage == Language.Arabic) ? Language.English : Language.Arabic;
            _loc.SetLanguage(target, persist: true);
        }

        private void OnScreenChanged(ScreenId id)
        {
            UpdateBackVisibility();
        }

        private void OnLanguageChanged(Language lang)
        {
            RefreshLanguageButton();
            RefreshAllScreens();
        }

        private void UpdateBackVisibility()
        {
            if (_btnBack != null)
            {
                _btnBack.gameObject.SetActive(_nav != null && _nav.CanGoBack);
            }
        }

        private void RefreshLanguageButton()
        {
            if (_languageText == null || _loc == null) return;
            // Display opposite or current language indicator
            _languageText.text = (_loc.CurrentLanguage == Language.Arabic) ? "EN" : "عربي";
        }

        private void RefreshAllScreens()
        {
            if (_screens == null || _loc == null) return;

            foreach (var s in _screens)
            {
                if (s == null) continue;
                string titleKey = $"placeholder.{s.ScreenId.ToString().ToLowerInvariant()}.title";
                string descKey = $"placeholder.{s.ScreenId.ToString().ToLowerInvariant()}.desc";
                string badge = $"PLACEHOLDER – {s.ScreenId} (Phase {GetPhaseNumber(s.ScreenId)})";
                s.Initialize(s.ScreenId, titleKey, descKey, badge, _loc);
            }
        }

        private int GetPhaseNumber(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.World: return 4;
                case ScreenId.Base: return 5;
                case ScreenId.Army: return 6;
                case ScreenId.Alliance: return 7;
                case ScreenId.Rankings: return 8;
                case ScreenId.Profile: return 9;
                default: return 0;
            }
        }
    }
}
