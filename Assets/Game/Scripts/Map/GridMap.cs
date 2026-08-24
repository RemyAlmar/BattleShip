using System;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour, IGridService
{
	[SerializeReference] private ShapeSettings _shapeSettings;
	[SerializeReference] private Metrics _metrics;
	[SerializeField] private float _cellSize = 1f;
	[SerializeField, Min(1)] private int _rows = 1;
	[SerializeField, Min(1)] private int _columns = 1;

	private Cell[,] _grid;
	public static IGridService Instance { get; private set; }

	public Vector2Int MapSize => new(_grid.GetLength(0), _grid.GetLength(1));

	private void Awake()
	{
		Instance = this;
		_metrics.CellSize = _cellSize;
		GenerateGrid();
	}
	private void OnValidate()
	{
		GenerateGrid();
	}
	private void OnDrawGizmos()
	{
		if (_shapeSettings == null || _shapeSettings.Shape == null || _grid == null || _metrics == null || _grid.Length <= 0) return;
		ActionInGrid((x, y, pos) => { _shapeSettings.Shape.DrawTo(_grid[x, y].WorldPosition); });
	}

	private void GenerateGrid()
	{
		if (_metrics == null)
		{
			Debug.LogWarning("[GridMap] Attention : une classe de type Metrics doit être sérialisée.");
			return;
		}
		_grid = new Cell[_rows, _columns];
		ActionInGrid((x, y, pos) => { _grid[x, y] = new Cell(x, y, pos); });
	}

	public bool IsValidCell(int x, int y) => x >= 0 && x < _grid.GetLength(0) && y >= 0 && y < _grid.GetLength(1);

	public bool TryGetHoveredCell(Ray ray, out Vector2Int gridPos)
	{
		gridPos = Vector2Int.zero;
		Plane groundPlane = new(Vector3.up, Vector3.zero);

		if (groundPlane.Raycast(ray, out float enterDistance))
		{
			Vector3 worldHitPoint = ray.GetPoint(enterDistance);
			gridPos = _metrics.WorldToGridPosition(worldHitPoint);
			Debug.DrawRay(ray.origin, ray.direction * enterDistance);
			return IsValidCell(gridPos.x, gridPos.y);
		}

		return false;
	}

	public bool TryGetWorldPosCell(Ray ray, out Vector3 worldPos)
	{
		worldPos = Vector3.zero;
		if (TryGetHoveredCell(ray, out Vector2Int gridPos))
		{
			worldPos = _grid[gridPos.x, gridPos.y].WorldPosition;
			return true;
		}

		return false;
	}

	private void ActionInGrid(Action<int, int, Vector3> callback)
	{
		if (callback == null) return;

		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _columns; j++)
			{
				Vector3 worldPos = _metrics.GridToWorldPosition(i, j);
				callback.Invoke(i, j, worldPos);
			}
		}
	}

	public Vector2Int GetDirection(byte dir, int currentY = 0) => _metrics.GetDirection(dir, currentY);

	public bool IsCellOccupied(int x, int y)
	{
		if (!IsValidCell(x, y)) return false;
		return _grid[x, y].IsOccupied;
	}

	/// <summary>
	/// Méthode universelle pour placer, déplacer ou faire tourner un occupant sur la grille.
	/// </summary>
	public bool TryPlaceOccupant(IGridOccupant occupant, Vector2Int origin, byte direction)
	{
		IReadOnlyList<Vector2Int> targetCells = occupant.GetOccupiedCellsAt(origin, direction);

		foreach (Vector2Int cell in targetCells)
		{
			if (!IsValidCell(cell.x, cell.y)) return false;

			if (IsCellOccupied(cell.x, cell.y) && GetCell(cell.x, cell.y).Occupant != occupant)
				return false;
		}

		ClearOccupantCells(occupant);

		occupant.SetGridPositionAndRotation(origin, direction);

		foreach (Vector2Int cell in targetCells)
		{
			_grid[cell.x, cell.y].IsOccupied = true;
			_grid[cell.x, cell.y].Occupant = occupant;
		}

		return true;
	}

	/// <summary>
	/// Effectue une rotation sur place de N crans.
	/// </summary>
	public bool TryRotateOccupant(IGridOccupant occupant, int stepCount)
	{
		byte targetDir = _metrics.Rotate(occupant.Direction, stepCount);
		return TryPlaceOccupant(occupant, occupant.Origin, targetDir);
	}

	/// <summary>
	/// Supprime l'occupant de la grille (utile lors de la destruction ou du retrait).
	/// </summary>
	public void ClearOccupantCells(IGridOccupant occupant)
	{
		foreach (Vector2Int cell in occupant.GetOccupiedCells())
		{
			if (IsValidCell(cell.x, cell.y) && _grid[cell.x, cell.y].Occupant == occupant)
			{
				_grid[cell.x, cell.y].IsOccupied = false;
				_grid[cell.x, cell.y].Occupant = null;
			}
		}
	}

	public Cell GetCell(int x, int y) => _grid[x, y];

	public bool TryGetCell(Vector2Int gridPos, out Cell cell)
	{
		cell = null;
		if (!IsValidCell(gridPos.x, gridPos.y))
			return false;

		cell = GetCell(gridPos.x, gridPos.y);
		return true;
	}

	public Vector3 GridToWorldPosition(Vector2Int gridPos) => _grid[gridPos.x, gridPos.y].WorldPosition;

	public Quaternion DirectionToWorldRotation(byte direction) => Quaternion.Euler(new Vector3(0, _metrics.GetAngle(direction), 0));
}

public class Cell
{
	public Vector3 WorldPosition { get; }
	public Vector2Int GridPosition { get; }

	public bool IsOccupied;
	public IGridOccupant Occupant;

	public Cell(int x, int y, Vector3 worldPosition)
	{
		GridPosition = new Vector2Int(x, y);
		WorldPosition = worldPosition;
	}
}