using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Map
{
    public interface IGridMetrics
    {
        public float CellSize { get; }
        public Vector3 GridToWorldPosition(int x, int y);
        public Vector2Int WorldToGridPosition(Vector3 worldPos);
        public int GetAngle(byte dir);
        public int GetDefaultAngle();
        public Vector2Int GetDirection(byte dir, int currentY = 0);
        public byte Rotate(byte dir, int count);
        public List<Vector2Int> GetLine(Vector2Int start, Vector2Int end);
    }
}