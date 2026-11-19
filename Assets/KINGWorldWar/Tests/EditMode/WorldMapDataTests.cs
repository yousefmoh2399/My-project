using System;
using System.Collections.Generic;
using System.Linq;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.WorldMap;
using KingWorldWar.Infrastructure.Localization;
using KingWorldWar.Infrastructure.WorldMap;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class WorldMapDataTests
    {
        private WorldMapConfigSO _config;
        private LocalizationService _loc;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<WorldMapConfigSO>();
            _config.InitializeFromDefaults();
            _loc = new LocalizationService(GameLog.Default);
            _loc.SetLanguage(Language.English);
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null)
            {
                UnityEngine.Object.DestroyImmediate(_config);
            }
        }

        [Test]
        public void Regions_ExactlyFiveRegionsConfigured()
        {
            var regions = _config.GetRegions();
            Assert.AreEqual(5, regions.Count, "Must have exactly 5 strategic regions in MVP");

            var expectedIds = new[] { "reg_north", "reg_coast", "reg_oasis", "reg_valley", "reg_ridge" };
            foreach (var id in expectedIds)
            {
                Assert.IsTrue(regions.Any(r => r.Id.Value == id), $"Missing region: {id}");
            }
        }

        [Test]
        public void Territories_ExactlyTwentyFiveTerritoriesConfigured()
        {
            var territories = _config.GetTerritories();
            Assert.AreEqual(25, territories.Count, "Must have exactly 25 gameplay territories in Phase 4");

            // Verify unique IDs
            var ids = new HashSet<string>();
            foreach (var t in territories)
            {
                Assert.IsTrue(t.Id.IsValid, $"Territory ID {t.Id} is invalid");
                Assert.IsTrue(ids.Add(t.Id.Value), $"Duplicate territory ID detected: {t.Id.Value}");
            }
        }

        [Test]
        public void Territories_ExactlyFivePerRegion()
        {
            var territories = _config.GetTerritories();
            var regions = _config.GetRegions();

            foreach (var r in regions)
            {
                int count = territories.Count(t => t.RegionId == r.Id);
                Assert.AreEqual(5, count, $"Region {r.Id.Value} does not contain exactly 5 territories");
            }
        }

        [Test]
        public void Territories_EachRegionHasExactlyOneCapital()
        {
            var territories = _config.GetTerritories();
            var regions = _config.GetRegions();

            foreach (var r in regions)
            {
                int capitalCount = territories.Count(t => t.RegionId == r.Id && t.Type == TerritoryType.RegionalCapital);
                Assert.AreEqual(1, capitalCount, $"Region {r.Id.Value} must have exactly one RegionalCapital");
            }
        }

        [Test]
        public void Localization_AllRegionsAndTerritoriesHaveStrictBilingualParity()
        {
            var regions = _config.GetRegions();
            var territories = _config.GetTerritories();

            foreach (var lang in new[] { Language.English, Language.Arabic })
            {
                _loc.SetLanguage(lang);

                foreach (var r in regions)
                {
                    string name = _loc.GetString(r.NameKey);
                    string desc = _loc.GetString(r.DescriptionKey);

                    Assert.IsFalse(name.StartsWith("[key:"), $"Missing {lang} localization for region name key: {r.NameKey}");
                    Assert.IsFalse(desc.StartsWith("[key:"), $"Missing {lang} localization for region desc key: {r.DescriptionKey}");
                    Assert.IsNotEmpty(name);
                    Assert.IsNotEmpty(desc);
                }

                foreach (var t in territories)
                {
                    string name = _loc.GetString(t.NameKey);
                    string desc = _loc.GetString(t.DescriptionKey);

                    Assert.IsFalse(name.StartsWith("[key:"), $"Missing {lang} localization for territory name key: {t.NameKey}");
                    Assert.IsFalse(desc.StartsWith("[key:"), $"Missing {lang} localization for territory desc key: {t.DescriptionKey}");
                    Assert.IsNotEmpty(name);
                    Assert.IsNotEmpty(desc);
                }
            }
        }

        [Test]
        public void Negative_DuplicateTerritoryId_ThrowsException()
        {
            var dupTerritories = new List<TerritoryData>
            {
                new TerritoryData(new TerritoryId("ter_dup"), new RegionId("reg_north"), "k1", "d1", TerritoryType.Frontier, Vector3.zero, ResourceYield.Zero),
                new TerritoryData(new TerritoryId("ter_dup"), new RegionId("reg_north"), "k2", "d2", TerritoryType.Frontier, Vector3.forward, ResourceYield.Zero)
            };

            var regions = _config.GetRegions();
            Assert.Throws<InvalidOperationException>(() =>
            {
                var graph = new TerritoryGraph(dupTerritories, regions);
            });
        }

        [Test]
        public void Negative_UnknownRegionReference_ThrowsException()
        {
            var territories = new List<TerritoryData>
            {
                new TerritoryData(new TerritoryId("ter_001"), new RegionId("reg_nonexistent"), "k1", "d1", TerritoryType.Frontier, Vector3.zero, ResourceYield.Zero)
            };

            var regions = _config.GetRegions();
            Assert.Throws<InvalidOperationException>(() =>
            {
                var graph = new TerritoryGraph(territories, regions);
            });
        }
    }
}
