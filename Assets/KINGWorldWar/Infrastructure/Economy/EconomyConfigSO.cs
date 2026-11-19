using System;
using System.Collections.Generic;
using KingWorldWar.Core.Economy;
using UnityEngine;

namespace KingWorldWar.Infrastructure.Economy
{
    [Serializable]
    public struct StorageCapTier
    {
        public int Level;
        public long GoldCap;
        public long OilCap;
        public long IronCap;
        public long ProtectedCap;
    }

    [Serializable]
    public struct ProductionRateTier
    {
        public int Level;
        public long GoldPerHour;
        public long OilPerHour;
        public long IronPerHour;
    }

    /// <summary>
    /// ScriptableObject data container defining balance curves, starting balances,
    /// and storage limits for the King World War economy.
    /// </summary>
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "KWW/Config/EconomyConfig")]
    public class EconomyConfigSO : ScriptableObject
    {
        [Header("Starting Balances")]
        [SerializeField] private long _startingGold = 50000;
        [SerializeField] private long _startingOil = 25000;
        [SerializeField] private long _startingIron = 15000;

        [Header("Tiers (Levels 1-10)")]
        [SerializeField] private List<StorageCapTier> _storageCaps = new();
        [SerializeField] private List<ProductionRateTier> _productionRates = new();

        public long StartingGold => _startingGold;
        public long StartingOil => _startingOil;
        public long StartingIron => _startingIron;

        public void InitializeFromDefaults()
        {
            _startingGold = 50000;
            _startingOil = 25000;
            _startingIron = 15000;

            _storageCaps.Clear();
            _productionRates.Clear();

            // Default Storage Caps (Levels 1-10)
            long[] caps = {
                50_000, 100_000, 200_000, 400_000, 750_000,
                1_200_000, 1_800_000, 2_500_000, 3_500_000, 5_000_000
            };
            long[] protectedAmounts = {
                10_000, 20_000, 40_000, 80_000, 150_000,
                250_000, 400_000, 600_000, 850_000, 1_200_000
            };

            for (int i = 0; i < caps.Length; i++)
            {
                _storageCaps.Add(new StorageCapTier
                {
                    Level = i + 1,
                    GoldCap = caps[i],
                    OilCap = caps[i],
                    IronCap = caps[i],
                    ProtectedCap = protectedAmounts[i]
                });
            }

            // Default Production Rates (Levels 1-10)
            long[] gRates = { 1200, 2400, 4000, 6500, 10000, 15000, 22000, 30000, 40000, 55000 };
            long[] oRates = { 800, 1600, 2800, 4500, 7000, 10500, 15000, 21000, 28000, 38000 };
            long[] iRates = { 600, 1200, 2000, 3200, 5000, 7500, 11000, 15000, 20000, 28000 };

            for (int i = 0; i < gRates.Length; i++)
            {
                _productionRates.Add(new ProductionRateTier
                {
                    Level = i + 1,
                    GoldPerHour = gRates[i],
                    OilPerHour = oRates[i],
                    IronPerHour = iRates[i]
                });
            }
        }

        public StorageCapacity GetStorageCapForLevel(int hqLevel)
        {
            int clamped = Math.Clamp(hqLevel, 1, _storageCaps.Count > 0 ? _storageCaps.Count : 10);
            if (_storageCaps.Count >= clamped)
            {
                var tier = _storageCaps[clamped - 1];
                return new StorageCapacity(tier.GoldCap, tier.OilCap, tier.IronCap);
            }
            return new StorageCapacity(50_000, 50_000, 50_000);
        }

        public ResourceYieldRate GetProductionRateForLevel(int level)
        {
            int clamped = Math.Clamp(level, 1, _productionRates.Count > 0 ? _productionRates.Count : 10);
            if (_productionRates.Count >= clamped)
            {
                var tier = _productionRates[clamped - 1];
                return new ResourceYieldRate(tier.GoldPerHour, tier.OilPerHour, tier.IronPerHour);
            }
            return new ResourceYieldRate(1200, 800, 600);
        }
    }
}
