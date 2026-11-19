using System;
using System.Collections.Generic;

namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Pure C# 2D spatial grid managing base layout, cell occupancy, and footprint collision.
    /// Default tactical grid dimensions: 20 x 20 cells.
    /// </summary>
    public sealed class BaseGrid
    {
        public const int DefaultWidth = 20;
        public const int DefaultHeight = 20;

        public int Width { get; }
        public int Height { get; }

        private readonly BuildingId[,] _cells;
        private readonly Dictionary<BuildingId, BuildingRuntimeState> _buildings;
        private readonly Dictionary<BuildingId, (int width, int height)> _buildingSizes;

        public IReadOnlyDictionary<BuildingId, BuildingRuntimeState> Buildings => _buildings;

        public BaseGrid(int width = DefaultWidth, int height = DefaultHeight)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Grid dimensions must be strictly positive.");

            Width = width;
            Height = height;
            _cells = new BuildingId[width, height];
            _buildings = new Dictionary<BuildingId, BuildingRuntimeState>();
            _buildingSizes = new Dictionary<BuildingId, (int width, int height)>();
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsFootprintInBounds(int originX, int originY, int width, int height)
        {
            if (width <= 0 || height <= 0) return false;
            return originX >= 0 && (originX + width) <= Width &&
                   originY >= 0 && (originY + height) <= Height;
        }

        public bool CanPlace(int originX, int originY, int width, int height, BuildingId? ignoreId = null)
        {
            if (!IsFootprintInBounds(originX, originY, width, height))
                return false;

            for (int x = originX; x < originX + width; x++)
            {
                for (int y = originY; y < originY + height; y++)
                {
                    var occupant = _cells[x, y];
                    if (occupant.IsValid)
                    {
                        if (ignoreId.HasValue && occupant == ignoreId.Value)
                            continue;
                        return false;
                    }
                }
            }

            return true;
        }

        public bool TryPlace(BuildingRuntimeState building, int width, int height)
        {
            if (building == null) throw new ArgumentNullException(nameof(building));
            if (!building.Id.IsValid) return false;
            if (_buildings.ContainsKey(building.Id)) return false;

            if (!CanPlace(building.GridX, building.GridY, width, height))
                return false;

            for (int x = building.GridX; x < building.GridX + width; x++)
            {
                for (int y = building.GridY; y < building.GridY + height; y++)
                {
                    _cells[x, y] = building.Id;
                }
            }

            _buildings[building.Id] = building;
            _buildingSizes[building.Id] = (width, height);
            return true;
        }

        public bool TryMove(BuildingId id, int newX, int newY)
        {
            if (!_buildings.TryGetValue(id, out var building))
                return false;
            if (!_buildingSizes.TryGetValue(id, out var size))
                return false;

            if (!CanPlace(newX, newY, size.width, size.height, ignoreId: id))
                return false;

            // Clear old cells
            for (int x = building.GridX; x < building.GridX + size.width; x++)
            {
                for (int y = building.GridY; y < building.GridY + size.height; y++)
                {
                    _cells[x, y] = BuildingId.Empty;
                }
            }

            // Occupy new cells
            for (int x = newX; x < newX + size.width; x++)
            {
                for (int y = newY; y < newY + size.height; y++)
                {
                    _cells[x, y] = id;
                }
            }

            building.SetPosition(newX, newY);
            return true;
        }

        public bool Remove(BuildingId id)
        {
            if (!_buildings.TryGetValue(id, out var building))
                return false;
            if (!_buildingSizes.TryGetValue(id, out var size))
                return false;

            for (int x = building.GridX; x < building.GridX + size.width; x++)
            {
                for (int y = building.GridY; y < building.GridY + size.height; y++)
                {
                    _cells[x, y] = BuildingId.Empty;
                }
            }

            _buildings.Remove(id);
            _buildingSizes.Remove(id);
            return true;
        }

        public BuildingId GetOccupant(int x, int y)
        {
            if (!IsInBounds(x, y)) return BuildingId.Empty;
            return _cells[x, y];
        }

        public bool TryGetBuilding(BuildingId id, out BuildingRuntimeState building)
        {
            return _buildings.TryGetValue(id, out building);
        }

        public bool TryGetBuildingSize(BuildingId id, out int width, out int height)
        {
            if (_buildingSizes.TryGetValue(id, out var size))
            {
                width = size.width;
                height = size.height;
                return true;
            }

            width = 0;
            height = 0;
            return false;
        }
    }
}
