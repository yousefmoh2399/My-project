using System;
using System.Collections.Generic;
using KingWorldWar.Core.Base;
using UnityEngine;

namespace KingWorldWar.Infrastructure.Base
{
    /// <summary>
    /// ScriptableObject database containing static building definitions, costs, and balance curves.
    /// Explicitly labeled dev-only: In production, authoritative building stats reside on Nakama server.
    /// </summary>
    [CreateAssetMenu(fileName = "BaseConfig", menuName = "KingWorldWar/Base Config")]
    public class BaseConfigSO : ScriptableObject, IBuildingDataSource
    {
        private Dictionary<BuildingType, BuildingDefinition> _definitions;

        public void InitializeFromDefaults()
        {
            var defs = DefaultBaseConfig.CreateDefaultDefinitions();
            _definitions = new Dictionary<BuildingType, BuildingDefinition>();
            foreach (var d in defs)
            {
                _definitions[d.Type] = d;
            }
        }

        private void OnEnable()
        {
            if (_definitions == null || _definitions.Count == 0)
            {
                InitializeFromDefaults();
            }
        }

        public BuildingDefinition GetDefinition(BuildingType type)
        {
            EnsureInitialized();
            if (_definitions.TryGetValue(type, out var def))
                return def;
            throw new KeyNotFoundException($"Building definition for '{type}' was not found.");
        }

        public bool TryGetDefinition(BuildingType type, out BuildingDefinition definition)
        {
            EnsureInitialized();
            return _definitions.TryGetValue(type, out definition);
        }

        public IReadOnlyList<BuildingDefinition> GetAllDefinitions()
        {
            EnsureInitialized();
            var list = new List<BuildingDefinition>(_definitions.Values);
            return list;
        }

        private void EnsureInitialized()
        {
            if (_definitions == null || _definitions.Count == 0)
            {
                InitializeFromDefaults();
            }
        }
    }
}
