using System;
using System.Collections.Generic;
using System.Linq;
using KingWorldWar.Core.WorldMap;
using KingWorldWar.Infrastructure.WorldMap;
using NUnit.Framework;
using UnityEngine;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class TerritoryGraphTests
    {
        private WorldMapConfigSO _config;
        private TerritoryGraph _graph;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<WorldMapConfigSO>();
            _config.InitializeFromDefaults();
            _graph = new TerritoryGraph(_config.GetTerritories(), _config.GetRegions());
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
        public void Adjacency_IsStrictlyBidirectionalAndSymmetric()
        {
            var territories = _config.GetTerritories();

            foreach (var t in territories)
            {
                var neighbors = _graph.GetNeighbors(t.Id);
                Assert.IsNotEmpty(neighbors, $"Territory {t.Id.Value} has no neighbors");

                foreach (var neighborId in neighbors)
                {
                    Assert.AreNotEqual(t.Id, neighborId, $"Territory {t.Id.Value} cannot have a self-loop");
                    Assert.IsTrue(_graph.AreAdjacent(t.Id, neighborId), $"{t.Id.Value} is not adjacent to {neighborId.Value}");
                    Assert.IsTrue(_graph.AreAdjacent(neighborId, t.Id), $"Asymmetry detected: {neighborId.Value} is not adjacent back to {t.Id.Value}");

                    var neighborNeighbors = _graph.GetNeighbors(neighborId);
                    Assert.IsTrue(neighborNeighbors.Contains(t.Id), $"Neighbor list asymmetry: {neighborId.Value} does not contain {t.Id.Value}");
                }
            }
        }

        [Test]
        public void Graph_IsFullyConnected()
        {
            var territories = _config.GetTerritories();
            var startId = territories.First().Id;

            var visited = new HashSet<TerritoryId>();
            var queue = new Queue<TerritoryId>();

            queue.Enqueue(startId);
            visited.Add(startId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var n in _graph.GetNeighbors(current))
                {
                    if (visited.Add(n))
                    {
                        queue.Enqueue(n);
                    }
                }
            }

            Assert.AreEqual(territories.Count, visited.Count, $"Graph is fragmented! Visited {visited.Count} out of {territories.Count} territories.");
        }

        [Test]
        public void Pathfinding_AdjacentTerritories_ReturnsOneHop()
        {
            var t1 = new TerritoryId("ter_nrd_001");
            var neighbors = _graph.GetNeighbors(t1);
            Assert.IsNotEmpty(neighbors);

            var t2 = neighbors.First();
            var path = _graph.FindShortestPath(t1, t2);

            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Count); // [t1, t2]
            Assert.AreEqual(t1, path[0]);
            Assert.AreEqual(t2, path[1]);
        }

        [Test]
        public void Pathfinding_SameTerritory_ReturnsSingleNode()
        {
            var t1 = new TerritoryId("ter_nrd_001");
            var path = _graph.FindShortestPath(t1, t1);

            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(t1, path[0]);
        }

        [Test]
        public void Pathfinding_CrossRegion_CalculatesOptimalPath()
        {
            var northCapital = new TerritoryId("ter_nrd_001");
            var coastCapital = new TerritoryId("ter_cst_001");

            var path = _graph.FindShortestPath(northCapital, coastCapital);

            Assert.IsNotNull(path);
            Assert.Greater(path.Count, 2, "Cross-region path between capitals should take multiple hops");
            Assert.AreEqual(northCapital, path.First());
            Assert.AreEqual(coastCapital, path.Last());

            // Validate that every consecutive step in the path is adjacent
            for (int i = 0; i < path.Count - 1; i++)
            {
                Assert.IsTrue(_graph.AreAdjacent(path[i], path[i + 1]),
                    $"Invalid path step: {path[i].Value} is not adjacent to {path[i + 1].Value}");
            }
        }

        [Test]
        public void Negative_AsymmetricAdjacency_ThrowsException()
        {
            var t1 = new TerritoryData(new TerritoryId("t_01"), new RegionId("reg_north"), "k1", "d1", TerritoryType.Frontier, Vector3.zero, ResourceYield.Zero);
            var t2 = new TerritoryData(new TerritoryId("t_02"), new RegionId("reg_north"), "k2", "d2", TerritoryType.Frontier, Vector3.forward, ResourceYield.Zero);

            // t1 points to t2, but t2 does NOT point to t1
            t1.AddNeighbor(t2.Id);

            var regions = _config.GetRegions();
            Assert.Throws<InvalidOperationException>(() =>
            {
                var badGraph = new TerritoryGraph(new[] { t1, t2 }, regions);
            });
        }

        [Test]
        public void Negative_SelfLoopAdjacency_ThrowsException()
        {
            var t1 = new TerritoryData(new TerritoryId("t_01"), new RegionId("reg_north"), "k1", "d1", TerritoryType.Frontier, Vector3.zero, ResourceYield.Zero);
            t1.AddNeighbor(t1.Id);

            var regions = _config.GetRegions();
            Assert.Throws<InvalidOperationException>(() =>
            {
                var badGraph = new TerritoryGraph(new[] { t1 }, regions);
            });
        }

        [Test]
        public void Negative_DisconnectedIsland_ThrowsException()
        {
            var t1 = new TerritoryData(new TerritoryId("t_01"), new RegionId("reg_north"), "k1", "d1", TerritoryType.Frontier, Vector3.zero, ResourceYield.Zero);
            var t2 = new TerritoryData(new TerritoryId("t_02"), new RegionId("reg_north"), "k2", "d2", TerritoryType.Frontier, Vector3.forward, ResourceYield.Zero);
            var t3_island = new TerritoryData(new TerritoryId("t_03"), new RegionId("reg_north"), "k3", "d3", TerritoryType.Frontier, Vector3.right, ResourceYield.Zero);

            t1.AddNeighbor(t2.Id);
            t2.AddNeighbor(t1.Id);
            // t3 has no neighbors

            var regions = _config.GetRegions();
            Assert.Throws<InvalidOperationException>(() =>
            {
                var badGraph = new TerritoryGraph(new[] { t1, t2, t3_island }, regions);
            });
        }
    }
}
