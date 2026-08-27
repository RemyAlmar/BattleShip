using System.Collections.Generic;
using UnityEngine;

public interface IGridService : IGridGeometry, IGridRaycaster, IGridPathfinding, IGridOccupancy
{
}
public interface IGridGeometry
{
	public Vector2Int MapSize { get; }
	public bool IsValidCell(int x, int y);
	public Vector3 GridToWorldPosition(Vector2Int gridPos);
	public Quaternion DirectionToWorldRotation(byte direction);
	public Vector2Int GetDirection(byte dir, int currentY = 0);
}
public interface IGridRaycaster
{
	public bool TryGetHoveredCell(Ray ray, out Vector2Int gridPos);
	public bool TryGetWorldPosCell(Ray ray, out Vector3 worldPos);
}
public interface IGridPathfinding
{
	public List<Vector2Int> GetNeighbors(Vector2Int gridPos, int depth = 1);
	public List<Vector2Int> GetNeighbors(List<Vector2Int> gridPoses, int depth = 1);
	public List<Vector2Int> GetLine(Vector2Int start, Vector2Int end);
	public List<Cell> GetCellsInRange(Vector2Int min, Vector2Int max);
	public Cell GetCell(int x, int y);
	public bool TryGetCell(Vector2Int gridPos, out Cell cell);
}
public interface IGridOccupancy
{
	public bool IsCellOccupied(int x, int y);
	public bool TryPlaceOccupant(IGridOccupant occupant, Vector2Int origin, byte direction);
	public bool TryRotateOccupant(IGridOccupant occupant, int stepCount);
	public void ClearOccupantCells(IGridOccupant occupant);
	public void ClearOccupantsCells(List<IGridOccupant> occupants);
	public List<Cell> GetFreeCells(List<Cell> cells);
	public bool TryGetOccupant(Vector2Int candidateCell, out IGridOccupant targetOccupant);
}