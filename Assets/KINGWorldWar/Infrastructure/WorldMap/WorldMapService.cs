using System;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.WorldMap;

namespace KingWorldWar.Infrastructure.WorldMap
{
    /// <summary>
    /// Master coordinator for World Map state, selection, and graph queries.
    /// </summary>
    public class WorldMapService : IWorldMapService
    {
        private readonly ILog _log;
        private readonly ITerritoryDataSource _dataSource;
        private readonly ITerritoryStateRepository _stateRepository;
        private readonly TerritoryGraph _graph = new TerritoryGraph();

        private TerritoryId? _selectedTerritoryId;
        private bool _isLoading;
        private string _loadError;

        public ITerritoryDataSource DataSource => _dataSource;
        public ITerritoryStateRepository StateRepository => _stateRepository;
        public TerritoryGraph Graph => _graph;

        public TerritoryId? SelectedTerritoryId => _selectedTerritoryId;
        public bool IsLoading => _isLoading;
        public string LoadError => _loadError;

        public event Action<TerritoryId?> OnTerritorySelected;
        public event Action<bool, string> OnLoadingStateChanged;

        public WorldMapService(
            ITerritoryDataSource dataSource,
            ITerritoryStateRepository stateRepository,
            ILog log)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            InitializeGraph();
        }

        private void InitializeGraph()
        {
            _isLoading = true;
            _loadError = null;
            OnLoadingStateChanged?.Invoke(true, null);

            try
            {
                _graph.Clear();
                var territories = _dataSource.GetTerritories();

                if (territories == null || territories.Count == 0)
                {
                    _loadError = "World map data is empty. No territories found.";
                    _log.Warning("WorldMap", _loadError);
                    _isLoading = false;
                    OnLoadingStateChanged?.Invoke(false, _loadError);
                    return;
                }

                foreach (var t in territories)
                {
                    if (t != null && t.Id.IsValid)
                    {
                        _graph.AddTerritory(t);
                    }
                }

                if (!_graph.ValidateSymmetry(out var symmetryErrors))
                {
                    _log.Warning("WorldMap", $"Adjacency asymmetry detected: {string.Join("; ", symmetryErrors)}");
                }

                if (!_graph.IsFullyConnected(out var unreached))
                {
                    _log.Warning("WorldMap", $"Graph has isolated territories: {string.Join(", ", unreached)}");
                }

                _log.Info("WorldMap", $"WorldMapService initialized with {_graph.TerritoryCount} territories across {_dataSource.GetRegions().Count} regions.");
                _isLoading = false;
                OnLoadingStateChanged?.Invoke(false, null);
            }
            catch (Exception ex)
            {
                _loadError = $"Failed to initialize world map: {ex.Message}";
                _log.Error("WorldMap", _loadError, ex);
                _isLoading = false;
                OnLoadingStateChanged?.Invoke(false, _loadError);
            }
        }

        public void SelectTerritory(TerritoryId? id)
        {
            if (id.HasValue && !id.Value.IsValid)
            {
                id = null;
            }

            if (_selectedTerritoryId == id) return;

            _selectedTerritoryId = id;
            _log.Debug("WorldMap", $"Selected territory: {(id.HasValue ? id.Value.ToString() : "None")}");
            OnTerritorySelected?.Invoke(_selectedTerritoryId);
        }
    }
}
