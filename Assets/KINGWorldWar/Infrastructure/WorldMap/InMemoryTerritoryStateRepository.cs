using System;
using System.Collections.Generic;
using KingWorldWar.Core.WorldMap;

namespace KingWorldWar.Infrastructure.WorldMap
{
    /// <summary>
    /// Local in-memory repository for territory runtime ownership and defense states.
    /// Used for offline development and testing. Untrusted: Real states will sync with Nakama.
    /// </summary>
    public class InMemoryTerritoryStateRepository : ITerritoryStateRepository
    {
        private readonly Dictionary<TerritoryId, TerritoryRuntimeState> _states =
            new Dictionary<TerritoryId, TerritoryRuntimeState>();

        public event Action<TerritoryId, TerritoryRuntimeState> OnTerritoryStateChanged;

        public InMemoryTerritoryStateRepository(ITerritoryDataSource dataSource = null)
        {
            if (dataSource != null)
            {
                SeedFromDataSource(dataSource);
            }
        }

        public void SeedFromDataSource(ITerritoryDataSource dataSource)
        {
            _states.Clear();
            var territories = dataSource.GetTerritories();

            foreach (var t in territories)
            {
                FactionType faction = FactionType.Neutral;
                string pId = string.Empty;
                string pName = string.Empty;
                string aId = string.Empty;
                string aTag = string.Empty;

                if (t.RegionId.Value == "reg_valley")
                {
                    faction = FactionType.PlayerAllied;
                    pId = "usr_player_01";
                    pName = "Commander";
                    aId = "all_vanguard";
                    aTag = "VGD";
                }
                else if (t.RegionId.Value == "reg_north")
                {
                    faction = FactionType.HostileRed;
                    pId = "usr_ai_crimson";
                    pName = "General Frost";
                    aId = "all_crimson";
                    aTag = "CRM";
                }
                else if (t.RegionId.Value == "reg_ridge")
                {
                    faction = FactionType.HostileBlue;
                    pId = "usr_ai_iron";
                    pName = "Marshal Vex";
                    aId = "all_iron";
                    aTag = "IRN";
                }

                var state = new TerritoryRuntimeState(t.Id, faction)
                {
                    OwnerPlayerId = pId,
                    OwnerPlayerName = pName,
                    OwnerAllianceId = aId,
                    OwnerAllianceTag = aTag,
                    CurrentGarrisonPower = t.GarrisonDefenseRating
                };

                _states[t.Id] = state;
            }
        }

        public TerritoryRuntimeState GetState(TerritoryId id)
        {
            if (_states.TryGetValue(id, out var state))
            {
                return state.Clone();
            }

            return new TerritoryRuntimeState(id, FactionType.Neutral);
        }

        public void SetState(TerritoryRuntimeState state)
        {
            if (state == null || !state.TerritoryId.IsValid) return;

            _states[state.TerritoryId] = state.Clone();
            OnTerritoryStateChanged?.Invoke(state.TerritoryId, state.Clone());
        }

        public IReadOnlyDictionary<TerritoryId, TerritoryRuntimeState> GetAllStates()
        {
            return _states;
        }
    }
}
