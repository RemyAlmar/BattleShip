using System.Collections.Generic;
using UnityEngine;

public interface IGridMetrics
{
	public Vector3 GridToWorldPosition(int x, int y);
	public Vector2Int WorldToGridPosition(Vector3 worldPos);
	public int GetAngle(byte dir);
	public Vector2Int GetDirection(byte dir, int currentY = 0);
	public byte Rotate(byte dir, int count);
	public float CellSize { get; }
	public List<Vector2Int> GetLine(Vector2Int start, Vector2Int end);

}