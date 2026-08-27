using System.Collections.Generic;
using UnityEngine;

public interface IGridOccupant
{
	public Transform Transform { get; }
	public Vector2Int Origin { get; }
	public byte Direction { get; }
	public int Size { get; }
	public List<Vector2Int> GetOccupiedCells();
	public List<Vector2Int> GetOccupiedCellsAt(Vector2Int targetOrigin, byte targetDirection);
	public void SetGridPositionAndRotation(Vector2Int newOrigin, byte direction);
}
