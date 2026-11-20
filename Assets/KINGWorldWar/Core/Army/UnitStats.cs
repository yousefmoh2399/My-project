using System;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Pure C# combat and tactical performance statistics for a military unit.
    /// Classified as ServerAuthoritative: server calculates and verifies combat outcomes.
    /// </summary>
    [Serializable]
    public readonly struct UnitStats : IEquatable<UnitStats>
    {
        public int Health { get; }
        public int Attack { get; }
        public int Defense { get; }
        public float Speed { get; }
        public float Range { get; }
        public int CapacityWeight { get; }
        public int CombatPower { get; }

        public UnitStats(
            int health,
            int attack,
            int defense,
            float speed,
            float range,
            int capacityWeight = 1,
            int combatPower = 0)
        {
            Health = Math.Max(1, health);
            Attack = Math.Max(1, attack);
            Defense = Math.Max(0, defense);
            Speed = Math.Max(0.1f, speed);
            Range = Math.Max(1.0f, range);
            CapacityWeight = Math.Max(1, capacityWeight);

            // If combat power is not explicitly specified, calculate based on composite tactical formula
            CombatPower = combatPower > 0 
                ? combatPower 
                : (Attack * 2) + (Defense * 2) + (Health / 5);
        }

        public bool Equals(UnitStats other) =>
            Health == other.Health &&
            Attack == other.Attack &&
            Defense == other.Defense &&
            Math.Abs(Speed - other.Speed) < 0.001f &&
            Math.Abs(Range - other.Range) < 0.001f &&
            CapacityWeight == other.CapacityWeight &&
            CombatPower == other.CombatPower;

        public override bool Equals(object obj) =>
            obj is UnitStats other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Health, Attack, Defense, Speed, Range, CapacityWeight, CombatPower);

        public static bool operator ==(UnitStats left, UnitStats right) => left.Equals(right);
        public static bool operator !=(UnitStats left, UnitStats right) => !left.Equals(right);

        public override string ToString() =>
            $"HP:{Health} ATK:{Attack} DEF:{Defense} SPD:{Speed:F1} RNG:{Range:F1} CAP:{CapacityWeight} PWR:{CombatPower}";
    }
}
