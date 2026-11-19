using System;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.WorldMap;
using KingWorldWar.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.WorldMap
{
    /// <summary>
    /// UI drawer displaying detailed intelligence dossier for the selected territory.
    /// Provides clear ownership indicators, colorblind symbols, and disabled action buttons.
    /// </summary>
    public class TerritoryDetailDrawer : MonoBehaviour
    {
        [Header("Root Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Header Texts")]
        [SerializeField] private TextMeshProUGUI _territoryNameText;
        [SerializeField] private TextMeshProUGUI _regionBadgeText;

        [Header("Faction & Owner")]
        [SerializeField] private TextMeshProUGUI _factionNameText;
        [SerializeField] private TextMeshProUGUI _factionSymbolText;
        [SerializeField] private TextMeshProUGUI _ownerPlayerText;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _yieldGoldText;
        [SerializeField] private TextMeshProUGUI _yieldOilText;
        [SerializeField] private TextMeshProUGUI _yieldIronText;
        [SerializeField] private TextMeshProUGUI _garrisonDefenseText;
        [SerializeField] private TextMeshProUGUI _adjacentFrontiersText;

        [Header("Action Buttons (Disabled Placeholder)")]
        [SerializeField] private Button _btnAttack;
        [SerializeField] private Button _btnScout;
        [SerializeField] private TextMeshProUGUI _phase9NoteText;
        [SerializeField] private Button _btnClose;

        [Header("Legend Panel")]
        [SerializeField] private GameObject _legendPanelRoot;
        [SerializeField] private Button _btnToggleLegend;

        private IWorldMapService _mapService;
        private ILocalizationService _loc;
        private TerritoryId? _currentTerritoryId;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;
        public TerritoryId? CurrentTerritoryId => _currentTerritoryId;

        public TextMeshProUGUI TerritoryNameText => _territoryNameText;
        public TextMeshProUGUI RegionBadgeText => _regionBadgeText;
        public TextMeshProUGUI FactionNameText => _factionNameText;
        public TextMeshProUGUI FactionSymbolText => _factionSymbolText;
        public TextMeshProUGUI OwnerPlayerText => _ownerPlayerText;
        public TextMeshProUGUI YieldGoldText => _yieldGoldText;
        public TextMeshProUGUI YieldOilText => _yieldOilText;
        public TextMeshProUGUI YieldIronText => _yieldIronText;
        public TextMeshProUGUI GarrisonDefenseText => _garrisonDefenseText;
        public Button BtnAttack => _btnAttack;
        public Button BtnScout => _btnScout;
        public Button BtnClose => _btnClose;
        public Button BtnToggleLegend => _btnToggleLegend;
        public GameObject LegendPanelRoot => _legendPanelRoot;

        public void SetupComponentsForTest(
            GameObject panelRoot,
            TextMeshProUGUI territoryNameText,
            TextMeshProUGUI regionBadgeText,
            TextMeshProUGUI factionNameText,
            TextMeshProUGUI factionSymbolText,
            TextMeshProUGUI ownerPlayerText,
            TextMeshProUGUI yieldGoldText,
            TextMeshProUGUI yieldOilText,
            TextMeshProUGUI yieldIronText,
            TextMeshProUGUI garrisonDefenseText,
            TextMeshProUGUI adjacentFrontiersText,
            Button btnAttack,
            Button btnScout,
            TextMeshProUGUI phase9NoteText,
            Button btnClose,
            GameObject legendPanelRoot,
            Button btnToggleLegend)
        {
            _panelRoot = panelRoot;
            _territoryNameText = territoryNameText;
            _regionBadgeText = regionBadgeText;
            _factionNameText = factionNameText;
            _factionSymbolText = factionSymbolText;
            _ownerPlayerText = ownerPlayerText;
            _yieldGoldText = yieldGoldText;
            _yieldOilText = yieldOilText;
            _yieldIronText = yieldIronText;
            _garrisonDefenseText = garrisonDefenseText;
            _adjacentFrontiersText = adjacentFrontiersText;
            _btnAttack = btnAttack;
            _btnScout = btnScout;
            _phase9NoteText = phase9NoteText;
            _btnClose = btnClose;
            _legendPanelRoot = legendPanelRoot;
            _btnToggleLegend = btnToggleLegend;

            if (_legendPanelRoot != null) _legendPanelRoot.SetActive(false);
            if (_btnClose != null) _btnClose.onClick.AddListener(OnCloseClicked);
            if (_btnToggleLegend != null) _btnToggleLegend.onClick.AddListener(OnToggleLegendClicked);
            if (_btnAttack != null) _btnAttack.interactable = false;
            if (_btnScout != null) _btnScout.interactable = false;
        }

        public void Bind(IWorldMapService mapService, ILocalizationService loc)
        {
            if (_mapService != null)
            {
                _mapService.OnTerritorySelected -= HandleTerritorySelected;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            _mapService = mapService;
            _loc = loc;

            if (_mapService != null)
            {
                _mapService.OnTerritorySelected += HandleTerritorySelected;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }

            Hide();
        }

        private void Awake()
        {
            if (_btnClose != null)
            {
                _btnClose.onClick.AddListener(OnCloseClicked);
            }

            if (_btnToggleLegend != null)
            {
                _btnToggleLegend.onClick.AddListener(OnToggleLegendClicked);
            }

            // Attack & Scout are disabled placeholders for MVP
            if (_btnAttack != null) _btnAttack.interactable = false;
            if (_btnScout != null) _btnScout.interactable = false;

            if (_legendPanelRoot != null)
            {
                _legendPanelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_mapService != null)
            {
                _mapService.OnTerritorySelected -= HandleTerritorySelected;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        public void Show(TerritoryId territoryId)
        {
            _currentTerritoryId = territoryId;
            UpdateDrawerContents();

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            _currentTerritoryId = null;
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void HandleTerritorySelected(TerritoryId? territoryId)
        {
            if (territoryId.HasValue && territoryId.Value.IsValid)
            {
                Show(territoryId.Value);
            }
            else
            {
                Hide();
            }
        }

        private void HandleLanguageChanged(Language lang)
        {
            if (_currentTerritoryId.HasValue)
            {
                UpdateDrawerContents();
            }
        }

        private void UpdateDrawerContents()
        {
            if (!_currentTerritoryId.HasValue || _mapService == null) return;

            if (!_mapService.DataSource.TryGetTerritory(_currentTerritoryId.Value, out var territory))
            {
                Hide();
                return;
            }

            var state = _mapService.StateRepository.GetState(_currentTerritoryId.Value);
            _mapService.DataSource.TryGetRegion(territory.RegionId, out var region);

            // Territory & Region Names
            if (_territoryNameText != null)
            {
                _territoryNameText.text = _loc != null ? _loc.GetString(territory.NameKey) : territory.Id.Value;
            }

            if (_regionBadgeText != null)
            {
                string regName = (region != null && _loc != null) ? _loc.GetString(region.NameKey) : territory.RegionId.Value;
                _regionBadgeText.text = $"[{regName.ToUpperInvariant()}]";
            }

            // Faction & Colorblind Symbols
            if (_factionNameText != null)
            {
                string factionKey = state.ControllingFaction.GetLocalizationKey();
                _factionNameText.text = _loc != null ? _loc.GetString(factionKey) : state.ControllingFaction.ToString();
            }

            if (_factionSymbolText != null)
            {
                _factionSymbolText.text = state.ControllingFaction.GetSymbol();
            }

            if (_ownerPlayerText != null)
            {
                if (!string.IsNullOrEmpty(state.OwnerPlayerName))
                {
                    string allianceTag = !string.IsNullOrEmpty(state.OwnerAllianceTag) ? $"[{state.OwnerAllianceTag}] " : "";
                    _ownerPlayerText.text = $"{allianceTag}{state.OwnerPlayerName}";
                }
                else
                {
                    _ownerPlayerText.text = _loc != null ? _loc.GetString("faction.neutral") : "Neutral";
                }
            }

            // Yields & Defense
            if (_yieldGoldText != null) _yieldGoldText.text = $"+{territory.BaseYield.GoldPerHour:N0}/h";
            if (_yieldOilText != null) _yieldOilText.text = $"+{territory.BaseYield.OilPerHour:N0}/h";
            if (_yieldIronText != null) _yieldIronText.text = $"+{territory.BaseYield.IronPerHour:N0}/h";
            if (_garrisonDefenseText != null) _garrisonDefenseText.text = state.CurrentGarrisonPower.ToString("N0");

            // Adjacent Frontiers
            if (_adjacentFrontiersText != null)
            {
                var neighbors = territory.AdjacentTerritories;
                var neighborNames = new System.Collections.Generic.List<string>();
                foreach (var nId in neighbors)
                {
                    if (_mapService.DataSource.TryGetTerritory(nId, out var neighborTerr))
                    {
                        string nName = _loc != null ? _loc.GetString(neighborTerr.NameKey) : nId.Value;
                        neighborNames.Add(nName);
                    }
                }
                _adjacentFrontiersText.text = string.Join(" • ", neighborNames);
            }

            // Phase 9 Note
            if (_phase9NoteText != null && _loc != null)
            {
                _phase9NoteText.text = _loc.GetString("world.drawer.phase9_note");
            }
        }

        private void OnCloseClicked()
        {
            _mapService?.SelectTerritory(null);
            Hide();
        }

        private void OnToggleLegendClicked()
        {
            if (_legendPanelRoot != null)
            {
                _legendPanelRoot.SetActive(!_legendPanelRoot.activeSelf);
            }
        }
    }
}
