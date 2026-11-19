using UnityEngine;

namespace KingWorldWar.Features.Base
{
    /// <summary>
    /// Visual representation of the player's tactical base ground grid.
    /// Manages grid-to-world coordinate projections, cell spacing, and bounds.
    /// </summary>
    public class BaseGridView : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private int _gridWidth = 20;
        [SerializeField] private int _gridHeight = 20;
        [SerializeField] private float _cellSize = 4f;
        [SerializeField] private Vector3 _originOffset = new Vector3(200f, 0f, 0f);

        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public float CellSize => _cellSize;
        public Vector3 OriginOffset => _originOffset;

        public Vector3 BaseCenterWorldPosition =>
            _originOffset + new Vector3(_gridWidth * 0.5f * _cellSize, 0f, _gridHeight * 0.5f * _cellSize);

        public Vector3 GridToWorld(int gridX, int gridY, int footprintWidth, int footprintHeight)
        {
            float centerX = _originOffset.x + (gridX + footprintWidth * 0.5f) * _cellSize;
            float centerZ = _originOffset.z + (gridY + footprintHeight * 0.5f) * _cellSize;
            return new Vector3(centerX, _originOffset.y, centerZ);
        }

        public (int x, int y) WorldToGrid(Vector3 worldPos)
        {
            float relativeX = worldPos.x - _originOffset.x;
            float relativeZ = worldPos.z - _originOffset.z;

            int gridX = Mathf.FloorToInt(relativeX / _cellSize);
            int gridY = Mathf.FloorToInt(relativeZ / _cellSize);
            return (gridX, gridY);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.5f, 0.3f, 0.3f);
            for (int x = 0; x <= _gridWidth; x++)
            {
                Vector3 start = _originOffset + new Vector3(x * _cellSize, 0f, 0f);
                Vector3 end = _originOffset + new Vector3(x * _cellSize, 0f, _gridHeight * _cellSize);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= _gridHeight; y++)
            {
                Vector3 start = _originOffset + new Vector3(0f, 0f, y * _cellSize);
                Vector3 end = _originOffset + new Vector3(_gridWidth * _cellSize, 0f, y * _cellSize);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
