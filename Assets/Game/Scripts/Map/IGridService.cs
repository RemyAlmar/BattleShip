using UnityEngine;

public interface IGridService
{
	public bool TryGetHoveredCell(Ray ray, out Vector2Int gridPos);
	public bool TryGetWorldPosCell(Ray ray, out Vector3 worldPos);
	public bool IsValidCell(int x, int y);
}