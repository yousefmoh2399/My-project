using System;
using System.Collections.Generic;
using KingWorldWar.Core.WorldMap;
using UnityEngine;

namespace KingWorldWar.Infrastructure.WorldMap
{
    /// <summary>
    /// ScriptableObject database containing static world map territories and regions.
    /// Explicitly labeled dev-only: In live production, map layouts can be fetched from Nakama.
    /// </summary>
    [CreateAssetMenu(fileName = "WorldMapConfig", menuName = "KingWorldWar/World Map Config")]
    public class WorldMapConfigSO : ScriptableObject, ITerritoryDataSource
    {
        [Header("World Configuration (Dev / Local Mock)")]
        [SerializeField] private RegionData[] _regions;
        [SerializeField] private TerritoryData[] _territories;

        private Dictionary<TerritoryId, TerritoryData> _territoryLookup;
        private Dictionary<RegionId, RegionData> _regionLookup;

        public IReadOnlyList<RegionData> Regions => _regions;
        public IReadOnlyList<TerritoryData> Territories => _territories;

        private void OnEnable()
        {
            RebuildLookups();
        }

        public void InitializeFromDefaults()
        {
            _regions = DefaultWorldMapData.CreateDefaultRegions();
            _territories = DefaultWorldMapData.CreateDefaultTerritories();
            RebuildLookups();
        }

        public void RebuildLookups()
        {
            _territoryLookup = new Dictionary<TerritoryId, TerritoryData>();
            if (_territories != null)
            {
                foreach (var t in _territories)
                {
                    if (t != null && t.Id.IsValid)
                    {
                        _territoryLookup[t.Id] = t;
                    }
                }
            }

            _regionLookup = new Dictionary<RegionId, RegionData>();
            if (_regions != null)
            {
                foreach (var r in _regions)
                {
                    if (r != null && r.Id.IsValid)
                    {
                        _regionLookup[r.Id] = r;
                    }
                }
            }
        }

        public IReadOnlyList<RegionData> GetRegions()
        {
            if (_regions == null || _regions.Length == 0)
            {
                InitializeFromDefaults();
            }
            return _regions;
        }

        public IReadOnlyList<TerritoryData> GetTerritories()
        {
            if (_territories == null || _territories.Length == 0)
            {
                InitializeFromDefaults();
            }
            return _territories;
        }

        public bool TryGetTerritory(TerritoryId id, out TerritoryData territory)
        {
            if (_territoryLookup == null) RebuildLookups();
            return _territoryLookup.TryGetValue(id, out territory);
        }

        public bool TryGetRegion(RegionId id, out RegionData region)
        {
            if (_regionLookup == null) RebuildLookups();
            return _regionLookup.TryGetValue(id, out region);
        }
    }
}
