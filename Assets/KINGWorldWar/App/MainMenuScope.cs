using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.UI;
using KingWorldWar.UI.Common;
using KingWorldWar.UI.Modals;
using KingWorldWar.UI.Shell;
using UnityEngine;

namespace KingWorldWar.App
{
    /// <summary>
    /// Local scene composition root for MainMenu.unity.
    /// Resolves services from ServiceContainer (or initializes fallbacks if loaded directly).
    /// </summary>
    public class MainMenuScope : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private MainMenuShellView _shellView;
        [SerializeField] private ModalDialogView _modalView;
        [SerializeField] private ResourceHeaderView _resourceHeaderView;

        public MainMenuShellView ShellView
        {
            get => _shellView;
            set => _shellView = value;
        }

        public ModalDialogView ModalView
        {
            get => _modalView;
            set => _modalView = value;
        }

        public ResourceHeaderView ResourceHeaderView
        {
            get => _resourceHeaderView;
            set => _resourceHeaderView = value;
        }

        public IScreenNavigationService NavigationService { get; private set; }
        public ILocalizationService LocalizationService { get; private set; }
        public IModalService ModalService { get; private set; }

        private void Start()
        {
            InitializeScope();
        }

        public void InitializeScope()
        {
            var container = ServiceContainer.Instance;

            var log = container.TryResolve<ILog>(out var l) ? l : GameLog.Default;
            var loc = container.TryResolve<ILocalizationService>(out var ls) ? ls : new LocalizationService(log);
            var nav = container.TryResolve<IScreenNavigationService>(out var ns) ? ns : new ScreenNavigationService(log);
            var modal = container.TryResolve<IModalService>(out var ms) ? ms : new ModalService(log);

            NavigationService = nav;
            LocalizationService = loc;
            ModalService = modal;

            if (!container.TryResolve<ILocalizationService>(out _))
                container.Register<ILocalizationService>(loc);
            if (!container.TryResolve<IScreenNavigationService>(out _))
                container.Register<IScreenNavigationService>(nav);
            if (!container.TryResolve<IModalService>(out _))
                container.Register<IModalService>(modal);

            if (_modalView != null)
            {
                _modalView.BindService(modal);
            }

            if (_shellView != null)
            {
                _shellView.BindServices(nav, loc, modal);
            }

            if (_resourceHeaderView != null)
            {
                // Default placeholder readout for initial shell
                _resourceHeaderView.SetManualValues(125000, 48000, 89000);
            }

            // Bind any LocalizedText components in scene
            var localizedTexts = FindObjectsByType<LocalizedText>(FindObjectsSortMode.None);
            foreach (var lt in localizedTexts)
            {
                lt.Bind(loc);
            }
        }
    }
}
