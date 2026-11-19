using System;

namespace KingWorldWar.Core.Economy
{
    /// <summary>
    /// Immutable value representation of a specific resource amount with overflow safety.
    /// </summary>
    public readonly struct ResourceAmount : IEquatable<ResourceAmount>
    {
        public ResourceType Type { get; }
        public long Amount { get; }

        public ResourceAmount(ResourceType type, long amount)
        {
            Type = type;
            Amount = Math.Max(0, amount);
        }

        public ResourceAmount Add(long value)
        {
            if (value <= 0) return this;
            long newAmount;
            try
            {
                checked { newAmount = Amount + value; }
            }
            catch (OverflowException)
            {
                newAmount = long.MaxValue;
            }
            return new ResourceAmount(Type, newAmount);
        }

        public ResourceAmount Subtract(long value)
        {
            if (value <= 0) return this;
            return new ResourceAmount(Type, Math.Max(0, Amount - value));
        }

        public ResourceAmount Multiply(int multiplier)
        {
            if (multiplier <= 0) return new ResourceAmount(Type, 0);
            long newAmount;
            try
            {
                checked { newAmount = Amount * multiplier; }
            }
            catch (OverflowException)
            {
                newAmount = long.MaxValue;
            }
            return new ResourceAmount(Type, newAmount);
        }

        public bool Equals(ResourceAmount other) => Type == other.Type && Amount == other.Amount;
        public override bool Equals(object obj) => obj is ResourceAmount other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Type, Amount);
        public override string ToString() => $"{Type}: {Amount:N0}";

        public static bool operator ==(ResourceAmount left, ResourceAmount right) => left.Equals(right);
        public static bool operator !=(ResourceAmount left, ResourceAmount right) => !left.Equals(right);
    }
}
