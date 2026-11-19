#if UNITY_EDITOR || DEVELOPMENT_BUILD
using KingWorldWar.Core.Base;

namespace KingWorldWar.Infrastructure.Base
{
    /// <summary>
    /// Development and Editor-only mock repository for player base structures.
    /// Strictly guarded by UNITY_EDITOR and DEVELOPMENT_BUILD defines.
    /// Never compiled into production release builds.
    /// Used to simulate server-side progression and testing offline.
    /// </summary>
    public sealed class LocalMockBaseRepository : InMemoryBaseStateRepository
    {
        public LocalMockBaseRepository(IBuildingDataSource dataSource, int gridWidth = 20, int gridHeight = 20)
            : base(dataSource, gridWidth, gridHeight)
        {
        }
    }
}
#endif
