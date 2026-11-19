using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingWorldWar.Core.WorldMap
{
    /// <summary>
    /// Pure C# domain model defining static territory data.
    /// Never use GameObject names as identifiers; views reference stable TerritoryId.
    /// </summary>
    [Serializable]
    public class TerritoryData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _regionId;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private TerritoryType _type;
        [SerializeField] private Vector3 _mapPosition;
        [SerializeField] private ResourceYield _baseYield;
        [SerializeField] private int _garrisonDefenseRating;
        [SerializeField] private string[] _adjacentTerritoryIds;

        public TerritoryId Id => new TerritoryId(_id);
        public RegionId RegionId => new RegionId(_regionId);
        public string NameKey => _nameKey;
        public string DescriptionKey => _descriptionKey;
        public TerritoryType Type => _type;
        public Vector3 MapPosition => _mapPosition;
        public ResourceYield BaseYield => _baseYield;
        public int GarrisonDefenseRating => _garrisonDefenseRating;

        public IReadOnlyList<TerritoryId> AdjacentTerritories
        {
            get
            {
                if (_adjacentTerritoryIds == null || _adjacentTerritoryIds.Length == 0)
                {
                    return Array.Empty<TerritoryId>();
                }

                var list = new TerritoryId[_adjacentTerritoryIds.Length];
                for (int i = 0; i < _adjacentTerritoryIds.Length; i++)
                {
                    list[i] = new TerritoryId(_adjacentTerritoryIds[i]);
                }
                return list;
            }
        }

        public TerritoryData(
            TerritoryId id,
            RegionId regionId,
            string nameKey,
            string descriptionKey,
            TerritoryType type,
            Vector3 mapPosition,
            ResourceYield baseYield)
            : this(id, regionId, nameKey, descriptionKey, type, mapPosition, baseYield, 10000, Array.Empty<string>())
        {
        }

        public TerritoryData(
            TerritoryId id,
            RegionId regionId,
            string nameKey,
            string descriptionKey,
            TerritoryType type,
            Vector3 mapPosition,
            ResourceYield baseYield,
            int garrisonDefenseRating,
            string[] adjacentTerritoryIds)
        {
            _id = id.Value;
            _regionId = regionId.Value;
            _nameKey = nameKey ?? string.Empty;
            _descriptionKey = descriptionKey ?? string.Empty;
            _type = type;
            _mapPosition = mapPosition;
            _baseYield = baseYield;
            _garrisonDefenseRating = Math.Max(0, garrisonDefenseRating);
            _adjacentTerritoryIds = adjacentTerritoryIds ?? Array.Empty<string>();
        }

        public void AddNeighbor(TerritoryId neighborId)
        {
            var current = _adjacentTerritoryIds != null ? new List<string>(_adjacentTerritoryIds) : new List<string>();
            if (!current.Contains(neighborId.Value))
            {
                current.Add(neighborId.Value);
                _adjacentTerritoryIds = current.ToArray();
            }
        }

        public bool Validate(out string error)
        {
            if (!Id.IsValid)
            {
                error = $"Territory ID '{_id}' is invalid. Must start with 'ter_'.";
                return false;
            }

            if (!RegionId.IsValid)
            {
                error = $"Territory '{_id}' references invalid Region ID '{_regionId}'. Must start with 'reg_'.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_nameKey))
            {
                error = $"Territory '{_id}' has empty name key.";
                return false;
            }

            if (_adjacentTerritoryIds == null || _adjacentTerritoryIds.Length == 0)
            {
                error = $"Territory '{_id}' has no adjacent connections.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
