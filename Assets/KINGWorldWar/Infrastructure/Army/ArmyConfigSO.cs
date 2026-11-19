using System.Collections.Generic;
using KingWorldWar.Core.Army;
using UnityEngine;

namespace KingWorldWar.Infrastructure.Army
{
    /// <summary>
    /// ScriptableObject asset container implementing IArmyDataSource.
    /// Provides inspector-configurable unit definitions with automated fallback to defaults.
    /// </summary>
    [CreateAssetMenu(fileName = "ArmyConfig", menuName = "KingWorldWar/Army/Army Config")]
    public sealed class ArmyConfigSO : ScriptableObject, IArmyDataSource
    {
        [SerializeField]
        private List<UnitDefinition> _definitions = new List<UnitDefinition>();

        private readonly Dictionary<UnitType, UnitDefinition> _byType = new Dictionary<UnitType, UnitDefinition>();
        private readonly Dictionary<UnitId, UnitDefinition> _byId = new Dictionary<UnitId, UnitDefinition>();

        private void OnEnable()
        {
            RebuildIndex();
        }

        public void InitializeFromDefaults()
        {
            _definitions = DefaultArmyConfig.CreateDefaultDefinitions();
            RebuildIndex();
        }

        private void RebuildIndex()
        {
            _byType.Clear();
            _byId.Clear();

            if (_definitions == null || _definitions.Count == 0)
            {
                _definitions = DefaultArmyConfig.CreateDefaultDefinitions();
            }

            foreach (var def in _definitions)
            {
                if (def == null) continue;
                _byType[def.Type] = def;
                _byId[def.Id] = def;
            }
        }

        public IReadOnlyList<UnitDefinition> GetAllDefinitions()
        {
            if (_definitions == null || _definitions.Count == 0)
            {
                InitializeFromDefaults();
            }
            return _definitions;
        }

        public UnitDefinition GetDefinition(UnitType type)
        {
            if (_byType.Count == 0) RebuildIndex();
            return _byType.TryGetValue(type, out var def) ? def : null;
        }

        public UnitDefinition GetDefinition(UnitId id)
        {
            if (_byId.Count == 0) RebuildIndex();
            return _byId.TryGetValue(id, out var def) ? def : null;
        }

        public bool TryGetDefinition(UnitType type, out UnitDefinition definition)
        {
            if (_byType.Count == 0) RebuildIndex();
            return _byType.TryGetValue(type, out definition);
        }

        public bool TryGetDefinition(UnitId id, out UnitDefinition definition)
        {
            if (_byId.Count == 0) RebuildIndex();
            return _byId.TryGetValue(id, out definition);
        }
    }
}
