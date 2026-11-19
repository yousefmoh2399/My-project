using System;
using System.Collections.Generic;

namespace KingWorldWar.Core.WorldMap
{
    /// <summary>
    /// Abstraction for static world map topology and region data.
    /// Can be backed by local ScriptableObject/JSON during development,
    /// or downloaded remotely from Nakama/CDN in production.
    /// </summary>
    public interface ITerritoryDataSource
    {
        IReadOnlyList<RegionData> GetRegions();
        IReadOnlyList<TerritoryData> GetTerritories();
        bool TryGetTerritory(TerritoryId id, out TerritoryData territory);
        bool TryGetRegion(RegionId id, out RegionData region);
    }

    /// <summary>
    /// Abstraction for dynamic territory runtime ownership and defense states.
    /// Backed locally in offline test mode, and synchronized via Nakama RPCs in online mode.
    /// </summary>
    public interface ITerritoryStateRepository
    {
        TerritoryRuntimeState GetState(TerritoryId id);
        void SetState(TerritoryRuntimeState state);
        IReadOnlyDictionary<TerritoryId, TerritoryRuntimeState> GetAllStates();
        event Action<TerritoryId, TerritoryRuntimeState> OnTerritoryStateChanged;
    }

    /// <summary>
    /// Master coordinator service for the World Map theater.
    /// Manages data sources, selection state, graph queries, and view synchronization.
    /// </summary>
    public interface IWorldMapService
    {
        ITerritoryDataSource DataSource { get; }
        ITerritoryStateRepository StateRepository { get; }
        TerritoryGraph Graph { get; }

        TerritoryId? SelectedTerritoryId { get; }
        bool IsLoading { get; }
        string LoadError { get; }

        void SelectTerritory(TerritoryId? id);
        event Action<TerritoryId?> OnTerritorySelected;
        event Action<bool, string> OnLoadingStateChanged;
    }
}
