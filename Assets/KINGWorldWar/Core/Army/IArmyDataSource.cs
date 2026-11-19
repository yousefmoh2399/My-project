using System.Collections.Generic;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Pure C# interface providing military unit archetype definitions and baseline balance curves.
    /// </summary>
    public interface IArmyDataSource
    {
        IReadOnlyList<UnitDefinition> GetAllDefinitions();
        UnitDefinition GetDefinition(UnitType type);
        UnitDefinition GetDefinition(UnitId id);
        bool TryGetDefinition(UnitType type, out UnitDefinition definition);
        bool TryGetDefinition(UnitId id, out UnitDefinition definition);
    }
}
