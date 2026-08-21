using System;
using UnityEngine;

public class GridMap : MonoBehaviour, IGridService
{
	[SerializeReference] private ShapeSettings _shapeSettings;
	[SerializeReference] private Metrics _metrics;
	[SerializeField] private float _cellSize = 1f;
	[SerializeField, Min(1)] private int _rows = 1;
	[SerializeField, Min(1)] private int _columns = 1;

	private Vector3[,] _grid;
	public static IGridService Instance { get; private set; }

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

	private void GenerateGrid()
	{
		if (_metrics == null)
		{
			Debug.LogWarning($"[GridMap] Attention une classe de type Metrics doit être sérialisée");
			return;
		}
		_grid = new Vector3[_rows, _columns];
		ActionInGrid((x, y, pos) => { _grid[x, y] = pos; });
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

			return IsValidCell(gridPos.x, gridPos.y);
		}

		return false;
	}
	public bool TryGetWorldPosCell(Ray ray, out Vector3 worldPos)
	{
		worldPos = Vector3.zero;
		if (TryGetHoveredCell(ray, out Vector2Int gridPos))
		{
			worldPos = _grid[gridPos.x, gridPos.y];
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

	private void OnDrawGizmos()
	{
		if (_shapeSettings == null || _shapeSettings.Shape == null || _grid.Length <= 0) return;
		ActionInGrid((x, y, pos) => { _shapeSettings.Shape.DrawTo(_grid[x, y]); });
	}
}