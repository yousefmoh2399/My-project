using System;
using System.Collections.Generic;

namespace KingWorldWar.Core.WorldMap
{
    /// <summary>
    /// Pure C# graph topology manager and pathfinder for strategic territories.
    /// Validates bi-directional adjacency and calculates march connectivity.
    /// </summary>
    public class TerritoryGraph
    {
        private readonly Dictionary<TerritoryId, HashSet<TerritoryId>> _adjacency =
            new Dictionary<TerritoryId, HashSet<TerritoryId>>();

        public int TerritoryCount => _adjacency.Count;

        public TerritoryGraph()
        {
        }

        public TerritoryGraph(IEnumerable<TerritoryData> territories, IEnumerable<RegionData> regions = null)
        {
            if (territories == null) throw new ArgumentNullException(nameof(territories));

            var knownRegions = regions != null ? new HashSet<RegionId>(System.Linq.Enumerable.Select(regions, r => r.Id)) : null;
            var addedTerritories = new HashSet<TerritoryId>();

            foreach (var t in territories)
            {
                if (t == null) throw new ArgumentNullException(nameof(territories), "Territory cannot be null");

                if (!addedTerritories.Add(t.Id))
                {
                    throw new InvalidOperationException($"Duplicate territory ID detected: {t.Id.Value}");
                }

                if (knownRegions != null && !knownRegions.Contains(t.RegionId))
                {
                    throw new InvalidOperationException($"Territory '{t.Id.Value}' references unknown Region '{t.RegionId.Value}'");
                }

                if (t.AdjacentTerritories.Count == 0 && addedTerritories.Count > 1)
                {
                    throw new InvalidOperationException($"Territory '{t.Id.Value}' has 0 connections (isolated island).");
                }

                foreach (var adj in t.AdjacentTerritories)
                {
                    if (adj == t.Id)
                    {
                        throw new InvalidOperationException($"Territory '{t.Id.Value}' has self-loop adjacency to itself.");
                    }
                }

                AddTerritory(t);
            }

            if (!ValidateSymmetry(out var symmetryErrors))
            {
                throw new InvalidOperationException($"Adjacency symmetry violation: {string.Join("; ", symmetryErrors)}");
            }

            if (!IsFullyConnected(out var unreached))
            {
                throw new InvalidOperationException($"Graph is not fully connected. Unreached territories: {unreached.Count}");
            }
        }

        public void Clear()
        {
            _adjacency.Clear();
        }

        public void AddTerritory(TerritoryData territory)
        {
            if (territory == null) throw new ArgumentNullException(nameof(territory));

            if (!_adjacency.TryGetValue(territory.Id, out var neighbors))
            {
                neighbors = new HashSet<TerritoryId>();
                _adjacency[territory.Id] = neighbors;
            }

            foreach (var adj in territory.AdjacentTerritories)
            {
                if (adj.IsValid && adj != territory.Id)
                {
                    neighbors.Add(adj);
                }
            }
        }

        public bool Contains(TerritoryId id) => _adjacency.ContainsKey(id);

        public bool IsAdjacent(TerritoryId a, TerritoryId b)
        {
            if (!_adjacency.TryGetValue(a, out var neighbors)) return false;
            return neighbors.Contains(b);
        }

        public bool AreAdjacent(TerritoryId a, TerritoryId b) => IsAdjacent(a, b);

        public IReadOnlyCollection<TerritoryId> GetNeighbors(TerritoryId id)
        {
            if (_adjacency.TryGetValue(id, out var neighbors))
            {
                return neighbors;
            }
            return Array.Empty<TerritoryId>();
        }

        /// <summary>
        /// Validates that adjacency is strictly symmetric (if A connects to B, B connects to A).
        /// </summary>
        public bool ValidateSymmetry(out List<string> errors)
        {
            errors = new List<string>();

            foreach (var kvp in _adjacency)
            {
                var from = kvp.Key;
                foreach (var to in kvp.Value)
                {
                    if (!_adjacency.TryGetValue(to, out var reverseNeighbors))
                    {
                        errors.Add($"Territory '{from}' references non-existent territory '{to}'.");
                    }
                    else if (!reverseNeighbors.Contains(from))
                    {
                        errors.Add($"Asymmetric adjacency: '{from}' lists '{to}' as neighbor, but '{to}' does not list '{from}'.");
                    }
                }
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Checks whether the entire world map forms a single connected component with no isolated areas.
        /// </summary>
        public bool IsFullyConnected(out List<TerritoryId> unreached)
        {
            unreached = new List<TerritoryId>();
            if (_adjacency.Count == 0) return true;

            var visited = new HashSet<TerritoryId>();
            var queue = new Queue<TerritoryId>();

            // Pick first node as root
            using var enumerator = _adjacency.Keys.GetEnumerator();
            enumerator.MoveNext();
            var start = enumerator.Current;

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (_adjacency.TryGetValue(current, out var neighbors))
                {
                    foreach (var next in neighbors)
                    {
                        if (visited.Add(next))
                        {
                            queue.Enqueue(next);
                        }
                    }
                }
            }

            foreach (var id in _adjacency.Keys)
            {
                if (!visited.Contains(id))
                {
                    unreached.Add(id);
                }
            }

            return unreached.Count == 0;
        }

        /// <summary>
        /// Breadth-First Search (BFS) to find the shortest hop path between two territories.
        /// </summary>
        public IReadOnlyList<TerritoryId> FindShortestPath(TerritoryId start, TerritoryId target)
        {
            if (!Contains(start) || !Contains(target)) return Array.Empty<TerritoryId>();
            if (start == target) return new[] { start };

            var queue = new Queue<TerritoryId>();
            var previous = new Dictionary<TerritoryId, TerritoryId>();
            var visited = new HashSet<TerritoryId>();

            queue.Enqueue(start);
            visited.Add(start);

            bool found = false;
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == target)
                {
                    found = true;
                    break;
                }

                if (_adjacency.TryGetValue(current, out var neighbors))
                {
                    foreach (var next in neighbors)
                    {
                        if (visited.Add(next))
                        {
                            previous[next] = current;
                            queue.Enqueue(next);
                        }
                    }
                }
            }

            if (!found) return Array.Empty<TerritoryId>();

            // Reconstruct path
            var path = new List<TerritoryId>();
            var curr = target;
            while (curr != start)
            {
                path.Add(curr);
                curr = previous[curr];
            }
            path.Add(start);
            path.Reverse();

            return path;
        }
    }
}
