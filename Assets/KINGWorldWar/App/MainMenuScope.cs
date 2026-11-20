using KingWorldWar.Core.Alliance;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Economy;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;
using KingWorldWar.Core.WorldMap;
using KingWorldWar.Features.Army;
using KingWorldWar.Features.Base;
using KingWorldWar.Features.Camera;
using KingWorldWar.Features.WorldMap;
using KingWorldWar.Infrastructure.Alliance;
using KingWorldWar.Infrastructure.Army;
using KingWorldWar.Infrastructure.Base;
using KingWorldWar.Infrastructure.Economy;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.UI;
using KingWorldWar.Infrastructure.WorldMap;
using KingWorldWar.UI.Alliance;
using KingWorldWar.UI.Army;
using KingWorldWar.UI.Base;
using KingWorldWar.UI.Common;
using KingWorldWar.UI.Economy;
using KingWorldWar.UI.Modals;
using KingWorldWar.UI.Shell;
using KingWorldWar.UI.WorldMap;
using UnityEngine;

namespace KingWorldWar.App
{
    /// <summary>
    /// Local scene composition root for MainMenu.unity.
    /// Resolves services from ServiceContainer (or initializes fallbacks if loaded directly).
    /// </summary>
    public class MainMenuScope : MonoBehaviour
    {
        [Header("Shell & UI Views")]
        [SerializeField] private MainMenuShellView _shellView;
        [SerializeField] private ModalDialogView _modalView;
        [SerializeField] private ResourceHeaderView _resourceHeaderView;

        [Header("World Map")]
        [SerializeField] private WorldMapView _worldMapView;
        [SerializeField] private TerritoryDetailDrawer _territoryDetailDrawer;
        [SerializeField] private StrategyCameraRig _cameraRig;

        [Header("Player Base")]
        [SerializeField] private PlayerBaseView _playerBaseView;
        [SerializeField] private BuildingDetailDrawer _buildingDetailDrawer;

        [Header("Army & Military")]
        [SerializeField] private PlayerArmyView _playerArmyView;
        [SerializeField] private ArmyScreenView _armyScreenView;
        [SerializeField] private RecruitmentDrawer _recruitmentDrawer;
        [SerializeField] private UnitDetailView _unitDetailView;

        [Header("Economy & Resources")]
        [SerializeField] private EconomyDetailDrawer _economyDetailDrawer;

        [Header("Alliance (Phase 7)")]
        [SerializeField] private AllianceScreenView _allianceScreenView;
        [SerializeField] private AllianceDetailDrawer _allianceDetailDrawer;

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

        public WorldMapView WorldMapView
        {
            get => _worldMapView;
            set => _worldMapView = value;
        }

        public TerritoryDetailDrawer TerritoryDetailDrawer
        {
            get => _territoryDetailDrawer;
            set => _territoryDetailDrawer = value;
        }

        public PlayerBaseView PlayerBaseView
        {
            get => _playerBaseView;
            set => _playerBaseView = value;
        }

        public BuildingDetailDrawer BuildingDetailDrawer
        {
            get => _buildingDetailDrawer;
            set => _buildingDetailDrawer = value;
        }

        public PlayerArmyView PlayerArmyView
        {
            get => _playerArmyView;
            set => _playerArmyView = value;
        }

        public ArmyScreenView ArmyScreenView
        {
            get => _armyScreenView;
            set => _armyScreenView = value;
        }

        public RecruitmentDrawer RecruitmentDrawer
        {
            get => _recruitmentDrawer;
            set => _recruitmentDrawer = value;
        }

        public UnitDetailView UnitDetailView
        {
            get => _unitDetailView;
            set => _unitDetailView = value;
        }

        public StrategyCameraRig CameraRig
        {
            get => _cameraRig;
            set => _cameraRig = value;
        }

        public EconomyDetailDrawer EconomyDetailDrawer
        {
            get => _economyDetailDrawer;
            set => _economyDetailDrawer = value;
        }

        public AllianceScreenView AllianceScreenView
        {
            get => _allianceScreenView;
            set => _allianceScreenView = value;
        }

        public AllianceDetailDrawer AllianceDetailDrawer
        {
            get => _allianceDetailDrawer;
            set => _allianceDetailDrawer = value;
        }

        public IScreenNavigationService NavigationService { get; private set; }
        public ILocalizationService LocalizationService { get; private set; }
        public IModalService ModalService { get; private set; }
        public IWorldMapService WorldMapService { get; private set; }
        public IPlayerBaseService PlayerBaseService { get; private set; }
        public IArmyService ArmyService { get; private set; }
        public IEconomyService EconomyService { get; private set; }
        public IAllianceService AllianceService { get; private set; }

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

            // World Map fallback if scene launched directly
            if (!container.TryResolve<IWorldMapService>(out var mapService))
            {
                var worldMapConfig = ScriptableObject.CreateInstance<WorldMapConfigSO>();
                worldMapConfig.InitializeFromDefaults();
                var stateRepo = new InMemoryTerritoryStateRepository(worldMapConfig);
                mapService = new WorldMapService(worldMapConfig, stateRepo, log);
                container.Register<ITerritoryDataSource>(worldMapConfig);
                container.Register<ITerritoryStateRepository>(stateRepo);
                container.Register<IWorldMapService>(mapService);
            }

            // Player Base fallback if scene launched directly
            if (!container.TryResolve<IPlayerBaseService>(out var baseService))
            {
                var baseConfig = ScriptableObject.CreateInstance<BaseConfigSO>();
                baseConfig.InitializeFromDefaults();
                var baseRepo = new InMemoryBaseStateRepository(baseConfig);
                baseService = new PlayerBaseService(baseConfig, baseRepo, log);
                container.Register<IBuildingDataSource>(baseConfig);
                container.Register<IBaseStateRepository>(baseRepo);
                container.Register<IPlayerBaseService>(baseService);
            }

            // Army & Units fallback if scene launched directly
            if (!container.TryResolve<IArmyService>(out var armyService))
            {
                var armyConfig = ScriptableObject.CreateInstance<ArmyConfigSO>();
                armyConfig.InitializeFromDefaults();
#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
                var armyRepo = new LocalMockArmyRepository(armyConfig);
#else
                var armyRepo = new LocalMockArmyRepository(armyConfig);
#endif
                var legacyRepo = new InMemoryArmyStateRepository();
                var baseRepo = container.TryResolve<IBaseStateRepository>(out var br) ? br : null;
                armyService = new ArmyService(armyConfig, armyRepo, legacyRepo, baseRepo, log);
                container.Register<IArmyDataSource>(armyConfig);
                container.Register<IArmyRepository>(armyRepo);
                container.Register<IArmyStateRepository>(legacyRepo);
                container.Register<IArmyService>(armyService);
            }

            // Economy fallback if scene launched directly
            if (!container.TryResolve<IEconomyService>(out var economyService))
            {
                var economyConfig = ScriptableObject.CreateInstance<EconomyConfigSO>();
                economyConfig.InitializeFromDefaults();
#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
                var economyRepo = new LocalMockEconomyRepository(
                    economyConfig.StartingGold,
                    economyConfig.StartingOil,
                    economyConfig.StartingIron,
                    economyConfig.GetStorageCapForLevel(1),
                    economyConfig.GetProductionRateForLevel(1));
#else
                var economyRepo = new LocalMockEconomyRepository();
#endif
                var ecoImpl = new EconomyService(economyRepo, log);
                economyService = ecoImpl;
                container.Register<EconomyConfigSO>(economyConfig);
                container.Register<IEconomyRepository>(economyRepo);
                container.Register<IEconomyService>(economyService);
                container.Register<IResourceState>(ecoImpl);
            }

            if (!container.TryResolve<IAllianceService>(out var allianceService))
            {
                var allianceRepo = new LocalMockAllianceRepository();
                allianceService = new AllianceService(allianceRepo, economyService, log);
                container.Register<IAllianceRepository>(allianceRepo);
                container.Register<IAllianceService>(allianceService);
            }

            NavigationService = nav;
            LocalizationService = loc;
            ModalService = modal;
            WorldMapService = mapService;
            PlayerBaseService = baseService;
            ArmyService = armyService;
            EconomyService = economyService;
            AllianceService = allianceService;

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
                _resourceHeaderView.BindEconomyService(economyService);
                _resourceHeaderView.OnHeaderClicked += () =>
                {
                    if (_economyDetailDrawer != null)
                    {
                        _economyDetailDrawer.Open();
                    }
                };
            }

            if (_economyDetailDrawer != null)
            {
                _economyDetailDrawer.Bind(economyService, loc);
            }

            if (_worldMapView != null)
            {
                _worldMapView.Bind(mapService, loc, _cameraRig);
            }

            if (_territoryDetailDrawer != null)
            {
                _territoryDetailDrawer.Bind(mapService, loc);
            }

            if (_playerBaseView != null)
            {
                _playerBaseView.Bind(baseService, loc, _cameraRig);
            }

            if (_buildingDetailDrawer != null)
            {
                _buildingDetailDrawer.Bind(baseService, loc, modal, economyService);
            }

            if (_playerArmyView != null)
            {
                _playerArmyView.Initialize(armyService, loc, _cameraRig);
                _playerArmyView.OnUnitSelected += (unitView) =>
                {
                    if (_recruitmentDrawer != null)
                    {
                        _recruitmentDrawer.OpenForUnit(unitView.UnitType);
                    }
                };
            }

            if (_recruitmentDrawer != null)
            {
                _recruitmentDrawer.Initialize(armyService, loc, modal);
            }

            if (_unitDetailView != null)
            {
                _unitDetailView.Initialize(armyService, loc, type => _recruitmentDrawer?.OpenForUnit(type));
            }

            if (_armyScreenView != null)
            {
                _armyScreenView.Initialize(armyService, loc, _recruitmentDrawer, _unitDetailView);
            }

            if (_allianceDetailDrawer != null)
            {
                _allianceDetailDrawer.Bind(loc);
            }

            if (_allianceScreenView != null)
            {
                _allianceScreenView.Initialize(allianceService, loc, modal, _allianceDetailDrawer);
                nav.RegisterScreen(ScreenId.Alliance, _allianceScreenView);
            }

            // Setup Screen switching
            nav.OnScreenChanged += HandleScreenChanged;
            HandleScreenChanged(nav.CurrentScreen);

            // Bind any LocalizedText components in scene
            var localizedTexts = Object.FindObjectsByType<LocalizedText>(FindObjectsInactive.Include);
            foreach (var lt in localizedTexts)
            {
                lt.Bind(loc);
            }
        }

        private void HandleScreenChanged(ScreenId screenId)
        {
            if (screenId == ScreenId.World)
            {
                if (_worldMapView != null) _worldMapView.gameObject.SetActive(true);
                if (_playerBaseView != null) _playerBaseView.gameObject.SetActive(false);
                if (_playerArmyView != null) _playerArmyView.gameObject.SetActive(false);
                if (_buildingDetailDrawer != null) _buildingDetailDrawer.Hide();
                if (_recruitmentDrawer != null) _recruitmentDrawer.Close();
                if (_cameraRig != null)
                {
                    _cameraRig.SetFocusPosition(new Vector3(50f, 0f, 40f));
                }
            }
            else if (screenId == ScreenId.Base)
            {
                if (_worldMapView != null) _worldMapView.gameObject.SetActive(false);
                if (_playerBaseView != null) _playerBaseView.gameObject.SetActive(true);
                if (_playerArmyView != null) _playerArmyView.gameObject.SetActive(false);
                if (_territoryDetailDrawer != null) _territoryDetailDrawer.Hide();
                if (_recruitmentDrawer != null) _recruitmentDrawer.Close();
                if (_playerBaseView != null && _cameraRig != null)
                {
                    _cameraRig.SetFocusPosition(_playerBaseView.GridView.BaseCenterWorldPosition);
                }
            }
            else if (screenId == ScreenId.Army)
            {
                if (_worldMapView != null) _worldMapView.gameObject.SetActive(false);
                if (_playerBaseView != null) _playerBaseView.gameObject.SetActive(false);
                if (_playerArmyView != null) _playerArmyView.gameObject.SetActive(true);
                if (_territoryDetailDrawer != null) _territoryDetailDrawer.Hide();
                if (_buildingDetailDrawer != null) _buildingDetailDrawer.Hide();
                if (_armyScreenView != null) _armyScreenView.Refresh();
                if (_playerArmyView != null && _cameraRig != null)
                {
                    _cameraRig.SetFocusPosition(_playerArmyView.InspectionCenter);
                }
            }
            else if (screenId == ScreenId.Alliance)
            {
                if (_worldMapView != null) _worldMapView.gameObject.SetActive(false);
                if (_playerBaseView != null) _playerBaseView.gameObject.SetActive(false);
                if (_playerArmyView != null) _playerArmyView.gameObject.SetActive(false);
                if (_territoryDetailDrawer != null) _territoryDetailDrawer.Hide();
                if (_buildingDetailDrawer != null) _buildingDetailDrawer.Hide();
                if (_recruitmentDrawer != null) _recruitmentDrawer.Close();
                if (_economyDetailDrawer != null) _economyDetailDrawer.Close();
                if (_allianceScreenView != null) _allianceScreenView.Refresh();
            }
            else
            {
                if (_worldMapView != null) _worldMapView.gameObject.SetActive(false);
                if (_playerBaseView != null) _playerBaseView.gameObject.SetActive(false);
                if (_playerArmyView != null) _playerArmyView.gameObject.SetActive(false);
                if (_territoryDetailDrawer != null) _territoryDetailDrawer.Hide();
                if (_buildingDetailDrawer != null) _buildingDetailDrawer.Hide();
                if (_recruitmentDrawer != null) _recruitmentDrawer.Close();
                if (_economyDetailDrawer != null) _economyDetailDrawer.Close();
                if (_allianceDetailDrawer != null) _allianceDetailDrawer.Close();
            }
        }

        private void Update()
        {
            EconomyService?.TickProjection();
        }

        private void OnDestroy()
        {
            if (NavigationService != null)
            {
                NavigationService.OnScreenChanged -= HandleScreenChanged;
            }
        }
    }
}
