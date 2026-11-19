using System;

namespace KingWorldWar.Core.WorldMap
{
    /// <summary>
    /// Dynamic runtime state of a territory.
    /// IMPORTANT: The client is untrusted. All ownership and garrison states are
    /// strictly non-authoritative display representations until provided by Nakama server.
    /// </summary>
    [Serializable]
    public class TerritoryRuntimeState
    {
        public TerritoryId TerritoryId { get; set; }
        public FactionType ControllingFaction { get; set; }
        public string OwnerPlayerId { get; set; }
        public string OwnerPlayerName { get; set; }
        public string OwnerAllianceId { get; set; }
        public string OwnerAllianceTag { get; set; }
        public int CurrentGarrisonPower { get; set; }
        public long LastUpdatedServerTimestamp { get; set; }

        public TerritoryRuntimeState(TerritoryId territoryId, FactionType faction = FactionType.Neutral)
        {
            TerritoryId = territoryId;
            ControllingFaction = faction;
            OwnerPlayerId = string.Empty;
            OwnerPlayerName = string.Empty;
            OwnerAllianceId = string.Empty;
            OwnerAllianceTag = string.Empty;
            CurrentGarrisonPower = 1000;
            LastUpdatedServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public TerritoryRuntimeState Clone()
        {
            return new TerritoryRuntimeState(TerritoryId, ControllingFaction)
            {
                OwnerPlayerId = OwnerPlayerId,
                OwnerPlayerName = OwnerPlayerName,
                OwnerAllianceId = OwnerAllianceId,
                OwnerAllianceTag = OwnerAllianceTag,
                CurrentGarrisonPower = CurrentGarrisonPower,
                LastUpdatedServerTimestamp = LastUpdatedServerTimestamp
            };
        }
    }
}
