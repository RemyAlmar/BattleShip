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

	[SerializeField] private RenderProvider _cellRenderPrefab;
	[SerializeField] private Color _colorEmpty = Color.white;
	[SerializeField] private Color _colorOccupied = Color.red;
	private Cell[,] _grid;
	public static IGridService Instance { get; private set; }

	public Vector2Int MapSize => new(_grid.GetLength(0) - 1, _grid.GetLength(1) - 1);

	private void Awake()
	{
		Instance = this;
		_metrics.CellSize = _cellSize;
		GenerateGrid();
		ActionInGrid((x, y, pos) =>
		{
			Cell cell = GetCell(x, y);
			cell.CellRender = Instantiate(_cellRenderPrefab, pos, Quaternion.identity);
			cell.CellRender.SetColor(_colorEmpty);
		});
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
		foreach (Vector2Int cellGridPos in targetCells)
		{
			if (!TryGetCell(cellGridPos, out Cell cell))
				return false;
			if (cell.IsOccupied && cell.Occupant != occupant)
				return false;
		}

		ClearOccupantCells(occupant);

		occupant.SetGridPositionAndRotation(origin, direction);

		foreach (Vector2Int cellGridPos in targetCells)
		{
			Cell cell = GetCell(cellGridPos.x, cellGridPos.y);
			cell.Occupant = occupant;
			cell.CellRender.SetColor(_colorOccupied);
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
		foreach (Vector2Int cellGridPos in occupant.GetOccupiedCells())
		{
			if (TryGetCell(cellGridPos, out Cell cell) && cell.Occupant == occupant)
			{
				cell.Occupant = null;
				cell.CellRender.SetColor(_colorEmpty);
			}
		}
	}

	public void ClearOccupantsCells(List<IGridOccupant> occupants)
	{
		foreach (IGridOccupant occupant in occupants)
			ClearOccupantCells(occupant);
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

	public Vector3 GridToWorldPosition(Vector2Int gridPos)
	{
		Vector3 worldPos = Vector3.zero;
		if (TryGetCell(gridPos, out Cell cell))
			worldPos = cell.WorldPosition;
		return worldPos;
	}

	public Quaternion DirectionToWorldRotation(byte direction) => Quaternion.Euler(new Vector3(0, _metrics.GetAngle(direction), 0));

	public List<Cell> GetCellsInRange(Vector2Int min, Vector2Int max)
	{
		List<Cell> cells = new();
		for (int y = min.y; y <= max.y; y++)
		{
			for (int x = min.x; x <= max.x; x++)
			{
				if (TryGetCell(new(x, y), out Cell cell))
					cells.Add(cell);
			}
		}
		return cells;
	}

	public List<Cell> GetFreeCells(List<Cell> cells)
	{
		List<Cell> validCell = new(cells);
		cells.ForEach(cell =>
		{
			if (cell.IsOccupied)
				validCell.Remove(cell);
		});
		return validCell;
	}
	public List<Vector2Int> GetNeighbors(Vector2Int gridPos, int depth = 1)
	{
		if (depth <= 0) return new List<Vector2Int>();

		List<Vector2Int> neighbors = new();
		HashSet<Vector2Int> visited = new() { gridPos };
		Queue<Vector2Int> currentLayer = new();
		currentLayer.Enqueue(gridPos);

		for (int step = 0; step < depth; step++)
		{
			int layerSize = currentLayer.Count;
			for (int i = 0; i < layerSize; i++)
			{
				Vector2Int currentPos = currentLayer.Dequeue();
				for (byte dir = 0; dir < _metrics.DirectionCount; dir++)
				{
					Vector2Int neighborPos = currentPos + GetDirection(dir, currentPos.y);

					if (visited.Add(neighborPos) && TryGetCell(neighborPos, out Cell neighborCell))
					{
						neighbors.Add(neighborCell.GridPosition);
						currentLayer.Enqueue(neighborPos);
						neighborCell.CellRender.SetColor(Color.green);
					}
				}
			}
		}
		return neighbors;
	}
	public List<Vector2Int> GetNeighbors(List<Vector2Int> gridPoses, int depth = 1)
	{
		if (depth <= 0 || gridPoses == null || gridPoses.Count == 0)
			return new List<Vector2Int>();

		List<Vector2Int> neighbors = new();
		HashSet<Vector2Int> visited = new(gridPoses);
		Queue<Vector2Int> currentLayer = new(gridPoses);

		for (int step = 0; step < depth; step++)
		{
			int layerSize = currentLayer.Count;
			for (int i = 0; i < layerSize; i++)
			{
				Vector2Int currentPos = currentLayer.Dequeue();
				for (byte dir = 0; dir < _metrics.DirectionCount; dir++)
				{
					Vector2Int targetPos = currentPos + GetDirection(dir, currentPos.y);

					if (visited.Add(targetPos) && TryGetCell(targetPos, out Cell cell))
					{
						neighbors.Add(cell.GridPosition);
						currentLayer.Enqueue(targetPos);
						cell.CellRender.SetColor(Color.cyan);
					}
				}
			}
		}
		return neighbors;
	}
}

public class Cell
{
	public Vector3 WorldPosition { get; }
	public Vector2Int GridPosition { get; }
	public RenderProvider CellRender { get; set; }
	public bool IsOccupied => Occupant != null;
	public IGridOccupant Occupant;

	public Cell(int x, int y, Vector3 worldPosition)
	{
		GridPosition = new Vector2Int(x, y);
		WorldPosition = worldPosition;
	}
}