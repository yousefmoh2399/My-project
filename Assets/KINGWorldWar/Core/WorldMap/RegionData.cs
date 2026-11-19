using System;
using UnityEngine;

namespace KingWorldWar.Core.WorldMap
{
    /// <summary>
    /// Pure domain representation of a strategic theater region/continent.
    /// Fictionalized and free of real-world political borders.
    /// </summary>
    [Serializable]
    public class RegionData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private string _primaryResource;
        [SerializeField] private Color _themeColor;

        public RegionId Id => new RegionId(_id);
        public string NameKey => _nameKey;
        public string DescriptionKey => _descriptionKey;
        public string PrimaryResource => _primaryResource;
        public Color ThemeColor => _themeColor;

        public RegionData(RegionId id, string nameKey, string descriptionKey, string primaryResource, Color themeColor)
        {
            _id = id.Value;
            _nameKey = nameKey ?? string.Empty;
            _descriptionKey = descriptionKey ?? string.Empty;
            _primaryResource = primaryResource ?? string.Empty;
            _themeColor = themeColor;
        }

        public bool Validate(out string error)
        {
            if (!Id.IsValid)
            {
                error = $"Region ID '{_id}' is invalid. Must start with 'reg_'.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_nameKey))
            {
                error = $"Region '{_id}' has empty name key.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
