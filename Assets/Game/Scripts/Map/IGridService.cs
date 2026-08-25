using System.Collections.Generic;
using UnityEngine;

public interface IGridService
{
	public Vector2Int MapSize { get; }
	public bool TryGetHoveredCell(Ray ray, out Vector2Int gridPos);
	public bool TryGetWorldPosCell(Ray ray, out Vector3 worldPos);
	public bool IsValidCell(int x, int y);
	public bool TryGetCell(Vector2Int gridPos, out Cell cell);
	public Cell GetCell(int x, int y);
	public Vector2Int GetDirection(byte dir, int currentY = 0);

	public bool IsCellOccupied(int x, int y);
	public bool TryPlaceOccupant(IGridOccupant occupant, Vector2Int origin, byte direction);
	public bool TryRotateOccupant(IGridOccupant occupant, int stepCount);
	public Vector3 GridToWorldPosition(Vector2Int gridPos);
	public Quaternion DirectionToWorldRotation(byte direction);
	public List<Cell> GetCellsInRange(Vector2Int min, Vector2Int max);
	public List<Cell> GetFreeCells(List<Cell> cells);
}