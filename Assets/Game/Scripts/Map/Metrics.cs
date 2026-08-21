using System;
using UnityEngine;

[Serializable]
public abstract class Metrics : IGridMetrics
{
	protected float _cellSize = 1f;
	public abstract Vector3 GridToWorldPosition(int x, int y);
	public abstract Vector2Int WorldToGridPosition(Vector3 worldPos);
	public virtual float CellSize { get => _cellSize; set => _cellSize = Mathf.Max(0.01f, value); }
}

[Serializable]
public class HexaMetrics : Metrics
{
	private float _innerRadius = 0.866025404f;

	public override float CellSize
	{
		get => _cellSize;
		set
		{
			_cellSize = Mathf.Max(0.01f, value);
			_innerRadius = _cellSize * 0.866025404f;
		}
	}
	public override Vector3 GridToWorldPosition(int x, int y)
	{
		float xPos = x * (_innerRadius * 2f);
		if (y % 2 != 0)
		{
			xPos += _innerRadius;
		}
		float zPos = y * (_cellSize * 1.5f);
		return new Vector3(xPos, 0f, zPos);
	}

	public override Vector2Int WorldToGridPosition(Vector3 worldPos)
	{
		int y = Mathf.RoundToInt(worldPos.z / (_cellSize * 1.5f));
		float xOffset = (y % 2 != 0) ? _innerRadius : 0f;
		int x = Mathf.RoundToInt((worldPos.x - xOffset) / (_innerRadius * 2f));
		return new Vector2Int(x, y);
	}
}

[Serializable]
public class SquareMetrics : Metrics
{
	public override Vector3 GridToWorldPosition(int x, int y) => new(x * CellSize, 0f, y * CellSize);

	public override Vector2Int WorldToGridPosition(Vector3 worldPos)
	{
		int x = Mathf.RoundToInt(worldPos.x / CellSize);
		int y = Mathf.RoundToInt(worldPos.z / CellSize);
		return new Vector2Int(x, y);
	}
}