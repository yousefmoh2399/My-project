using System;
using UnityEngine;

namespace KingWorldWar.Core.WorldMap
{
    /// <summary>
    /// Non-authoritative client display representation of territory hourly production yields.
    /// All actual economic updates and grants are authoritative on Nakama server.
    /// </summary>
    [Serializable]
    public struct ResourceYield : IEquatable<ResourceYield>
    {
        public static readonly ResourceYield Zero = new ResourceYield(0, 0, 0);

        [SerializeField] private long _goldPerHour;
        [SerializeField] private long _oilPerHour;
        [SerializeField] private long _ironPerHour;

        public long GoldPerHour => _goldPerHour;
        public long OilPerHour => _oilPerHour;
        public long IronPerHour => _ironPerHour;

        public ResourceYield(long gold, long oil, long iron)
        {
            _goldPerHour = Math.Max(0, gold);
            _oilPerHour = Math.Max(0, oil);
            _ironPerHour = Math.Max(0, iron);
        }

        public bool Equals(ResourceYield other) =>
            _goldPerHour == other._goldPerHour &&
            _oilPerHour == other._oilPerHour &&
            _ironPerHour == other._ironPerHour;

        public override bool Equals(object obj) => obj is ResourceYield other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_goldPerHour, _oilPerHour, _ironPerHour);

        public static bool operator ==(ResourceYield left, ResourceYield right) => left.Equals(right);

        public static bool operator !=(ResourceYield left, ResourceYield right) => !left.Equals(right);
    }
}
