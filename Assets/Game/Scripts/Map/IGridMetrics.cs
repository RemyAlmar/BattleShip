using UnityEngine;

public interface IGridMetrics
{
	public Vector3 GridToWorldPosition(int x, int y);
	public Vector2Int WorldToGridPosition(Vector3 worldPos);
	public float CellSize { get; }
}