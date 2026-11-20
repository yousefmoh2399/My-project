using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;
using KingWorldWar.Infrastructure.Army;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ArmyDefinitionValidationTests
    {
        [Test]
        public void MVPUnitDefinitions_ContainsExactFiveArchetypes_WithUniqueIds()
        {
            var defs = DefaultArmyConfig.CreateDefaultDefinitions();
            Assert.That(defs.Count, Is.EqualTo(5));

            var idSet = new HashSet<UnitId>();
            var typeSet = new HashSet<UnitType>();

            foreach (var def in defs)
            {
                Assert.That(def.Id.IsEmpty, Is.False, $"Unit {def.Type} has empty Id");
                Assert.That(idSet.Add(def.Id), Is.True, $"Duplicate UnitId found: {def.Id}");
                Assert.That(typeSet.Add(def.Type), Is.True, $"Duplicate UnitType found: {def.Type}");
                def.Validate();
            }

            Assert.That(typeSet.Contains(UnitType.Tank), Is.True);
            Assert.That(typeSet.Contains(UnitType.ArmoredVehicle), Is.True);
            Assert.That(typeSet.Contains(UnitType.Artillery), Is.True);
            Assert.That(typeSet.Contains(UnitType.FighterAircraft), Is.True);
            Assert.That(typeSet.Contains(UnitType.ReconAircraft), Is.True);
        }

        [Test]
        public void UnitDefinitions_Categories_AreAccuratelyAssigned()
        {
            var defs = DefaultArmyConfig.CreateDefaultDefinitions();
            var dict = new Dictionary<UnitType, UnitDefinition>();
            foreach (var d in defs) dict[d.Type] = d;

            Assert.That(dict[UnitType.Tank].Category, Is.EqualTo(UnitCategory.Ground));
            Assert.That(dict[UnitType.ArmoredVehicle].Category, Is.EqualTo(UnitCategory.Ground));
            Assert.That(dict[UnitType.Artillery].Category, Is.EqualTo(UnitCategory.Ground));

            Assert.That(dict[UnitType.FighterAircraft].Category, Is.EqualTo(UnitCategory.Air));
            Assert.That(dict[UnitType.ReconAircraft].Category, Is.EqualTo(UnitCategory.Air));
        }

        [Test]
        public void UnitDefinitions_Stats_AreStrictlyPositiveAndValid()
        {
            var defs = DefaultArmyConfig.CreateDefaultDefinitions();

            foreach (var def in defs)
            {
                var s = def.Stats;
                Assert.That(s.Health, Is.GreaterThan(0), $"{def.Type} health must be > 0");
                Assert.That(s.Attack, Is.GreaterThan(0), $"{def.Type} attack must be > 0");
                Assert.That(s.Defense, Is.GreaterThan(0), $"{def.Type} defense must be > 0");
                Assert.That(s.Speed, Is.GreaterThan(0f), $"{def.Type} speed must be > 0");
                Assert.That(s.Range, Is.GreaterThan(0f), $"{def.Type} range must be > 0");
                Assert.That(s.CapacityWeight, Is.GreaterThan(0), $"{def.Type} capacity weight must be > 0");
                Assert.That(s.CombatPower, Is.GreaterThan(0), $"{def.Type} combat power must be > 0");

                var c = def.Cost;
                Assert.That(c.Gold, Is.GreaterThan(0), $"{def.Type} gold cost must be > 0");
                Assert.That(c.Oil, Is.GreaterThan(0), $"{def.Type} oil cost must be > 0");
                Assert.That(c.Iron, Is.GreaterThan(0), $"{def.Type} iron cost must be > 0");
                Assert.That(c.RecruitDurationSeconds, Is.GreaterThan(0), $"{def.Type} recruit duration must be > 0");

                Assert.That(def.RequiredBuildingLevel, Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void UnitDefinitions_StatClassification_DistinguishesDisplayOnlyVsServerAuthoritative()
        {
            var defs = DefaultArmyConfig.CreateDefaultDefinitions();

            foreach (var def in defs)
            {
                // ServerAuthoritative gameplay numbers
                Assert.That(UnitDefinition.GetStatAuthority("Health"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("Attack"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("Defense"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("Speed"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("Range"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("CapacityWeight"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("CombatPower"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("Cost"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("RequiredBuildingType"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("RequiredBuildingLevel"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("Tier"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("NextLevelUpgradeCost"), Is.EqualTo(StatAuthority.ServerAuthoritative));
                Assert.That(UnitDefinition.GetStatAuthority("NextLevelUpgradeDurationSeconds"), Is.EqualTo(StatAuthority.ServerAuthoritative));

                // DisplayOnly UI presentation fields
                Assert.That(UnitDefinition.GetStatAuthority("NameKey"), Is.EqualTo(StatAuthority.DisplayOnly));
                Assert.That(UnitDefinition.GetStatAuthority("DescKey"), Is.EqualTo(StatAuthority.DisplayOnly));
                Assert.That(UnitDefinition.GetStatAuthority("RoleKey"), Is.EqualTo(StatAuthority.DisplayOnly));
                Assert.That(UnitDefinition.GetStatAuthority("IconAssetPath"), Is.EqualTo(StatAuthority.DisplayOnly));
                Assert.That(UnitDefinition.GetStatAuthority("ModelAssetPath"), Is.EqualTo(StatAuthority.DisplayOnly));
                Assert.That(UnitDefinition.GetStatAuthority("NextLevelBenefitsKey"), Is.EqualTo(StatAuthority.DisplayOnly));
            }
        }

        [Test]
        public void UnitDefinitions_BuildingPrerequisites_ReferenceValidStructures()
        {
            var defs = DefaultArmyConfig.CreateDefaultDefinitions();
            var validBuildings = new HashSet<BuildingType>
            {
                BuildingType.TankFactory,
                BuildingType.Airfield
            };

            foreach (var def in defs)
            {
                Assert.That(validBuildings.Contains(def.RequiredBuildingType), Is.True,
                    $"Unit {def.Type} references invalid building type {def.RequiredBuildingType}");
            }
        }
    }
}
