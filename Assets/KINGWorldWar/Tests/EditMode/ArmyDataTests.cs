using System;
using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;
using KingWorldWar.Infrastructure.Army;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ArmyDataTests
    {
        [Test]
        public void UnitId_Default_EqualsEmpty()
        {
            var defaultId = default(UnitId);
            Assert.That(defaultId, Is.EqualTo(UnitId.Empty));
            Assert.That(defaultId.IsEmpty, Is.True);
            Assert.That(defaultId.Value, Is.EqualTo(string.Empty));
        }

        [Test]
        public void UnitId_NonEmpty_StoresNormalizedValue()
        {
            var id = new UnitId("  unt_tank  ");
            Assert.That(id.Value, Is.EqualTo("unt_tank"));
            Assert.That(id.IsEmpty, Is.False);
            Assert.That(id, Is.EqualTo(new UnitId("UNT_TANK")));
        }

        [Test]
        public void DefaultArmyConfig_ContainsAllFiveMvpArchetypes()
        {
            var defs = DefaultArmyConfig.CreateDefaultDefinitions();
            Assert.That(defs.Count, Is.EqualTo(5));

            var types = new HashSet<UnitType>();
            foreach (var d in defs)
            {
                types.Add(d.Type);
                d.Validate();
            }

            Assert.That(types.Contains(UnitType.Tank), Is.True);
            Assert.That(types.Contains(UnitType.ArmoredVehicle), Is.True);
            Assert.That(types.Contains(UnitType.Artillery), Is.True);
            Assert.That(types.Contains(UnitType.FighterAircraft), Is.True);
            Assert.That(types.Contains(UnitType.ReconAircraft), Is.True);
        }

        [Test]
        public void UnitDefinitions_Prerequisites_AreCorrect()
        {
            var defs = DefaultArmyConfig.CreateDefaultDefinitions();
            var dict = new Dictionary<UnitType, UnitDefinition>();
            foreach (var d in defs) dict[d.Type] = d;

            Assert.That(dict[UnitType.Tank].RequiredBuildingType, Is.EqualTo(BuildingType.TankFactory));
            Assert.That(dict[UnitType.Tank].RequiredBuildingLevel, Is.EqualTo(1));

            Assert.That(dict[UnitType.ArmoredVehicle].RequiredBuildingType, Is.EqualTo(BuildingType.TankFactory));
            Assert.That(dict[UnitType.ArmoredVehicle].RequiredBuildingLevel, Is.EqualTo(1));

            Assert.That(dict[UnitType.Artillery].RequiredBuildingType, Is.EqualTo(BuildingType.TankFactory));
            Assert.That(dict[UnitType.Artillery].RequiredBuildingLevel, Is.EqualTo(2));

            Assert.That(dict[UnitType.FighterAircraft].RequiredBuildingType, Is.EqualTo(BuildingType.Airfield));
            Assert.That(dict[UnitType.FighterAircraft].RequiredBuildingLevel, Is.EqualTo(1));

            Assert.That(dict[UnitType.ReconAircraft].RequiredBuildingType, Is.EqualTo(BuildingType.Airfield));
            Assert.That(dict[UnitType.ReconAircraft].RequiredBuildingLevel, Is.EqualTo(1));
        }

        [Test]
        public void UnitCost_Multiply_ScalesAccurately()
        {
            var cost = new UnitCost(gold: 50, oil: 10, iron: 20, recruitDurationSeconds: 15);
            var scaled = cost.Multiply(5);

            Assert.That(scaled.Gold, Is.EqualTo(250));
            Assert.That(scaled.Oil, Is.EqualTo(50));
            Assert.That(scaled.Iron, Is.EqualTo(100));
            Assert.That(scaled.RecruitDurationSeconds, Is.EqualTo(75));

            var zero = cost.Multiply(0);
            Assert.That(zero.Gold, Is.EqualTo(0));
            Assert.That(zero.RecruitDurationSeconds, Is.EqualTo(0));
        }

        [Test]
        public void UnitStats_AutoCalculatesCombatPower()
        {
            var stats = new UnitStats(health: 100, attack: 20, defense: 10, speed: 5f, range: 10f);
            // formula: (20*2) + (10*2) + (100/5) = 40 + 20 + 20 = 80
            Assert.That(stats.CombatPower, Is.EqualTo(80));
        }

        [Test]
        public void RecruitmentQueueItem_ProgressAndTimers_CalculateCorrectly()
        {
            var start = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var end = start.AddSeconds(60);
            var item = new RecruitmentQueueItem("q1", UnitType.Tank, 1, start, end, new UnitCost(10, 10, 10, 60));

            Assert.That(item.TotalDurationSeconds, Is.EqualTo(60.0));
            Assert.That(item.ProgressNormalized(start), Is.EqualTo(0.0f));
            Assert.That(item.ProgressNormalized(start.AddSeconds(30)), Is.EqualTo(0.5f));
            Assert.That(item.ProgressNormalized(end), Is.EqualTo(1.0f));
            Assert.That(item.ProgressNormalized(end.AddSeconds(10)), Is.EqualTo(1.0f));

            Assert.That(item.RemainingSeconds(start), Is.EqualTo(60.0));
            Assert.That(item.RemainingSeconds(start.AddSeconds(45)), Is.EqualTo(15.0));
            Assert.That(item.RemainingSeconds(end.AddSeconds(5)), Is.EqualTo(0.0));

            Assert.That(item.IsCompleted(start.AddSeconds(59)), Is.False);
            Assert.That(item.IsCompleted(end), Is.True);
        }

        [Test]
        public void ArmyRoster_AddRemoveAndTotalPower_BehaveExpectedly()
        {
            var config = ScriptableObject.CreateInstance<ArmyConfigSO>();
            config.InitializeFromDefaults();

            var roster = new ArmyRoster();
            Assert.That(roster.TotalTroops, Is.EqualTo(0));

            roster.AddUnits(UnitType.ArmoredVehicle, 50);
            roster.AddUnits(UnitType.Tank, 5);
            Assert.That(roster.GetCount(UnitType.ArmoredVehicle), Is.EqualTo(50));
            Assert.That(roster.GetCount(UnitType.Tank), Is.EqualTo(5));
            Assert.That(roster.TotalTroops, Is.EqualTo(55));

            var avPower = config.GetDefinition(UnitType.ArmoredVehicle).Stats.CombatPower * 50;
            var tankPower = config.GetDefinition(UnitType.Tank).Stats.CombatPower * 5;
            var expectedTotalPower = avPower + tankPower;

            Assert.That(roster.CalculateTotalCombatPower(config), Is.EqualTo(expectedTotalPower));

            bool removed = roster.TryRemoveUnits(UnitType.ArmoredVehicle, 20);
            Assert.That(removed, Is.True);
            Assert.That(roster.GetCount(UnitType.ArmoredVehicle), Is.EqualTo(30));

            bool overRemove = roster.TryRemoveUnits(UnitType.ArmoredVehicle, 100);
            Assert.That(overRemove, Is.False);
            Assert.That(roster.GetCount(UnitType.ArmoredVehicle), Is.EqualTo(30)); // preserved
        }
    }
}
