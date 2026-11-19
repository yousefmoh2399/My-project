using System;
using KingWorldWar.Core.Base;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class BaseGridTests
    {
        [Test]
        public void Grid_Constructor_InitializesDimensions()
        {
            var grid = new BaseGrid(20, 20);

            Assert.AreEqual(20, grid.Width);
            Assert.AreEqual(20, grid.Height);
            Assert.AreEqual(0, grid.Buildings.Count);
            Assert.AreEqual(BuildingId.Empty, grid.GetOccupant(0, 0));
            Assert.AreEqual(BuildingId.Empty, grid.GetOccupant(19, 19));
        }

        [Test]
        public void Grid_Constructor_InvalidDimensions_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new BaseGrid(0, 20));
            Assert.Throws<ArgumentException>(() => new BaseGrid(20, -5));
        }

        [Test]
        public void Grid_IsInBounds_ValidAndInvalidCoordinates()
        {
            var grid = new BaseGrid(20, 20);

            Assert.IsTrue(grid.IsInBounds(0, 0));
            Assert.IsTrue(grid.IsInBounds(19, 19));
            Assert.IsTrue(grid.IsInBounds(10, 10));

            Assert.IsFalse(grid.IsInBounds(-1, 0));
            Assert.IsFalse(grid.IsInBounds(0, -1));
            Assert.IsFalse(grid.IsInBounds(20, 10));
            Assert.IsFalse(grid.IsInBounds(10, 20));
        }

        [Test]
        public void Grid_IsFootprintInBounds_ChecksBoundaries()
        {
            var grid = new BaseGrid(20, 20);

            Assert.IsTrue(grid.IsFootprintInBounds(0, 0, 4, 4));
            Assert.IsTrue(grid.IsFootprintInBounds(16, 16, 4, 4));

            Assert.IsFalse(grid.IsFootprintInBounds(17, 16, 4, 4)); // X exceeds: 17 + 4 = 21 > 20
            Assert.IsFalse(grid.IsFootprintInBounds(16, 17, 4, 4)); // Y exceeds: 17 + 4 = 21 > 20
            Assert.IsFalse(grid.IsFootprintInBounds(-1, 0, 4, 4));
            Assert.IsFalse(grid.IsFootprintInBounds(0, 0, 0, 4));
        }

        [Test]
        public void Grid_TryPlace_PlacesBuildingAndUpdatesCells()
        {
            var grid = new BaseGrid(20, 20);
            var hqId = new BuildingId("bld_hq");
            var hq = new BuildingRuntimeState(hqId, BuildingType.CommandCenter, currentLevel: 1, gridX: 8, gridY: 8);

            bool placed = grid.TryPlace(hq, 4, 4);

            Assert.IsTrue(placed);
            Assert.AreEqual(1, grid.Buildings.Count);
            Assert.IsTrue(grid.TryGetBuilding(hqId, out var registered));
            Assert.AreEqual(hq, registered);

            // Verify all 16 footprint cells are occupied
            for (int x = 8; x < 12; x++)
            {
                for (int y = 8; y < 12; y++)
                {
                    Assert.AreEqual(hqId, grid.GetOccupant(x, y));
                }
            }

            // Neighboring cells should remain empty
            Assert.AreEqual(BuildingId.Empty, grid.GetOccupant(7, 8));
            Assert.AreEqual(BuildingId.Empty, grid.GetOccupant(12, 8));
        }

        [Test]
        public void Grid_TryPlace_Overlap_Fails()
        {
            var grid = new BaseGrid(20, 20);
            var hq = new BuildingRuntimeState(new BuildingId("bld_hq"), BuildingType.CommandCenter, 1, 8, 8);
            grid.TryPlace(hq, 4, 4);

            var barracks = new BuildingRuntimeState(new BuildingId("bld_barracks_01"), BuildingType.Barracks, 1, 10, 10);
            bool placed = grid.TryPlace(barracks, 3, 3); // Overlaps cells [10..11, 10..11]

            Assert.IsFalse(placed);
            Assert.AreEqual(1, grid.Buildings.Count);
        }

        [Test]
        public void Grid_TryPlace_OutOfBounds_Fails()
        {
            var grid = new BaseGrid(20, 20);
            var b = new BuildingRuntimeState(new BuildingId("bld_hq"), BuildingType.CommandCenter, 1, 18, 18);
            bool placed = grid.TryPlace(b, 4, 4);

            Assert.IsFalse(placed);
            Assert.AreEqual(0, grid.Buildings.Count);
        }

        [Test]
        public void Grid_TryMove_ValidVacant_MovesAndClearsOldCells()
        {
            var grid = new BaseGrid(20, 20);
            var barId = new BuildingId("bld_barracks_01");
            var barracks = new BuildingRuntimeState(barId, BuildingType.Barracks, 1, 2, 4);
            grid.TryPlace(barracks, 3, 3);

            bool moved = grid.TryMove(barId, 2, 10);

            Assert.IsTrue(moved);
            Assert.AreEqual(2, barracks.GridX);
            Assert.AreEqual(10, barracks.GridY);

            // Old cells must be empty
            for (int x = 2; x < 5; x++)
            {
                for (int y = 4; y < 7; y++)
                {
                    Assert.AreEqual(BuildingId.Empty, grid.GetOccupant(x, y));
                }
            }

            // New cells must be occupied
            for (int x = 2; x < 5; x++)
            {
                for (int y = 10; y < 13; y++)
                {
                    Assert.AreEqual(barId, grid.GetOccupant(x, y));
                }
            }
        }

        [Test]
        public void Grid_TryMove_Collision_FailsAndPreservesOldCells()
        {
            var grid = new BaseGrid(20, 20);
            var hqId = new BuildingId("bld_hq");
            var hq = new BuildingRuntimeState(hqId, BuildingType.CommandCenter, 1, 8, 8);
            grid.TryPlace(hq, 4, 4);

            var barId = new BuildingId("bld_barracks_01");
            var barracks = new BuildingRuntimeState(barId, BuildingType.Barracks, 1, 2, 4);
            grid.TryPlace(barracks, 3, 3);

            bool moved = grid.TryMove(barId, 7, 7); // Collides with HQ

            Assert.IsFalse(moved);
            Assert.AreEqual(2, barracks.GridX);
            Assert.AreEqual(4, barracks.GridY);

            // Old cells still belong to barracks
            Assert.AreEqual(barId, grid.GetOccupant(2, 4));
            // HQ still occupies its cells
            Assert.AreEqual(hqId, grid.GetOccupant(8, 8));
        }

        [Test]
        public void Grid_TryMove_SamePosition_Succeeds()
        {
            var grid = new BaseGrid(20, 20);
            var barId = new BuildingId("bld_barracks_01");
            var barracks = new BuildingRuntimeState(barId, BuildingType.Barracks, 1, 2, 4);
            grid.TryPlace(barracks, 3, 3);

            bool moved = grid.TryMove(barId, 2, 4);

            Assert.IsTrue(moved);
            Assert.AreEqual(barId, grid.GetOccupant(2, 4));
        }

        [Test]
        public void Grid_Remove_RemovesBuildingAndClearsOccupancy()
        {
            var grid = new BaseGrid(20, 20);
            var barId = new BuildingId("bld_barracks_01");
            var barracks = new BuildingRuntimeState(barId, BuildingType.Barracks, 1, 2, 4);
            grid.TryPlace(barracks, 3, 3);

            bool removed = grid.Remove(barId);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, grid.Buildings.Count);
            for (int x = 2; x < 5; x++)
            {
                for (int y = 4; y < 7; y++)
                {
                    Assert.AreEqual(BuildingId.Empty, grid.GetOccupant(x, y));
                }
            }
        }
    }
}
