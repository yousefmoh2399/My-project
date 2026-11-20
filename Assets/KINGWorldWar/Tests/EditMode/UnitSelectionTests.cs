using System;
using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Infrastructure.Army;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class UnitSelectionTests
    {
        private ArmyConfigSO _dataSource;
        private ArmyRosterSnapshot _snapshot;

        [SetUp]
        public void SetUp()
        {
            _dataSource = ScriptableObject.CreateInstance<ArmyConfigSO>();
            _dataSource.InitializeFromDefaults();

            var owned = new Dictionary<UnitType, int>
            {
                { UnitType.Tank, 10 },
                { UnitType.ArmoredVehicle, 20 },
                { UnitType.Artillery, 5 },
                { UnitType.FighterAircraft, 4 },
                { UnitType.ReconAircraft, 8 }
            };

            var inTraining = new Dictionary<UnitType, int>
            {
                { UnitType.Tank, 2 },
                { UnitType.ArmoredVehicle, 0 },
                { UnitType.Artillery, 1 },
                { UnitType.FighterAircraft, 0 },
                { UnitType.ReconAircraft, 0 }
            };

            var deployed = new Dictionary<UnitType, int>
            {
                { UnitType.Tank, 3 }, // Available: 10 - 2 - 3 = 5
                { UnitType.ArmoredVehicle, 5 }, // Available: 20 - 0 - 5 = 15
                { UnitType.Artillery, 0 }, // Available: 5 - 1 - 0 = 4
                { UnitType.FighterAircraft, 2 }, // Available: 4 - 0 - 2 = 2
                { UnitType.ReconAircraft, 1 } // Available: 8 - 0 - 1 = 7
            };

            _snapshot = new ArmyRosterSnapshot(
                "usr_test_commander",
                1,
                "1.0.0",
                DateTime.UtcNow,
                owned,
                inTraining,
                deployed);
        }

        [TearDown]
        public void TearDown()
        {
            if (_dataSource != null)
            {
                UnityEngine.Object.DestroyImmediate(_dataSource);
            }
        }

        [Test]
        public void Selection_WithinAvailableReserves_ValidatesSuccessfully()
        {
            var selDict = new Dictionary<UnitId, int>
            {
                { DefaultArmyConfig.TankId, 4 }, // Available 5 -> OK
                { DefaultArmyConfig.ArmoredVehicleId, 10 }, // Available 15 -> OK
                { DefaultArmyConfig.FighterAircraftId, 2 } // Available 2 -> OK
            };

            var selection = new UnitSelection(selDict);
            Assert.That(selection.IsEmpty, Is.False);
            Assert.That(selection.TotalCount, Is.EqualTo(16));

            var result = selection.Validate(_snapshot, _dataSource);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Selection_ExceedingAvailableReserves_FailsValidation()
        {
            var selDict = new Dictionary<UnitId, int>
            {
                { DefaultArmyConfig.TankId, 6 } // Available is 5
            };

            var selection = new UnitSelection(selDict);
            var result = selection.Validate(_snapshot, _dataSource);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("INVALID_PAYLOAD"));
            Assert.That(result.Error.Details.Contains("only 5 are available"), Is.True);
        }

        [Test]
        public void Selection_UnknownUnitId_FailsValidation()
        {
            var selDict = new Dictionary<UnitId, int>
            {
                { new UnitId("unt_unknown_cyber_soldier"), 5 }
            };

            var selection = new UnitSelection(selDict);
            var result = selection.Validate(_snapshot, _dataSource);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("UNIT_NOT_FOUND"));
        }

        [Test]
        public void Selection_Empty_FailsValidation()
        {
            var selection = new UnitSelection();
            Assert.That(selection.IsEmpty, Is.True);

            var result = selection.Validate(_snapshot, _dataSource);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("INVALID_PAYLOAD"));
            Assert.That(result.Error.Details.Contains("cannot be empty"), Is.True);
        }

        [Test]
        public void Selection_ToDeploymentRequest_ProducesCleanDto()
        {
            var selDict = new Dictionary<UnitId, int>
            {
                { DefaultArmyConfig.TankId, 3 },
                { DefaultArmyConfig.ArtilleryId, 2 }
            };

            var selection = new UnitSelection(selDict);
            var dto = selection.ToDeploymentRequest("cst_001", "req_custom_123");

            Assert.That(dto.RequestId, Is.EqualTo("req_custom_123"));
            Assert.That(dto.TargetTerritoryId, Is.EqualTo("cst_001"));
            Assert.That(dto.Units.Count, Is.EqualTo(2));
            Assert.That(dto.Units[DefaultArmyConfig.TankId.Value], Is.EqualTo(3));
            Assert.That(dto.Units[DefaultArmyConfig.ArtilleryId.Value], Is.EqualTo(2));
        }
    }
}
