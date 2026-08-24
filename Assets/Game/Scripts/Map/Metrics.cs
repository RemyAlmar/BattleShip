using System;
using UnityEngine;

[Serializable]
public abstract class Metrics : IGridMetrics
{
	protected float _cellSize = 1f;
	public abstract Vector3 GridToWorldPosition(int x, int y);
	public abstract Vector2Int WorldToGridPosition(Vector3 worldPos);

	public abstract Vector2Int GetDirection(byte dir, int currentY = 0);
	public abstract int GetAngle(byte dir);
	public abstract byte Rotate(byte dir, int count);

	public virtual float CellSize { get => _cellSize; set => _cellSize = Mathf.Max(0.01f, value); }
}
[Serializable]
public class HexaMetrics : Metrics
{
	private float _innerRadius = 0.866025404f;

	public static readonly int[] AngleDirections = new int[] { 30, 90, 150, 210, 270, 330 };

	private static readonly Vector2Int[] EvenRowDirections = new Vector2Int[]
	{
		new(0, 1),   // NorthEast (0)
		new(1, 0),   // East (1)
		new(0, -1),  // SouthEast (2)
		new(-1, -1), // SouthWest (3)
		new(-1, 0),  // West (4)
		new(-1, 1)   // NorthWest (5)
	};

	private static readonly Vector2Int[] OddRowDirections = new Vector2Int[]
	{
		new(1, 1),   // NorthEast (0)
		new(1, 0),   // East (1)
		new(1, -1),  // SouthEast (2)
		new(0, -1),  // SouthWest (3)
		new(-1, 0),  // West (4)
		new(0, 1)    // NorthWest (5)
	};

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
		if (y % 2 != 0) xPos += _innerRadius;
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

	public override Vector2Int GetDirection(byte dir, int currentY = 0) => (currentY % 2 == 0) ? EvenRowDirections[dir] : OddRowDirections[dir];

	public override int GetAngle(byte dir) => AngleDirections[dir];

	public override byte Rotate(byte dir, int count) => (byte)((dir + (count % 6) + 6) % 6);

	public enum HexaDirection : byte
	{
		NorthEast = 0,
		East = 1,
		SouthEast = 2,
		SouthWest = 3,
		West = 4,
		NorthWest = 5
	}
}

[Serializable]
public class SquareMetrics : Metrics
{
	public static readonly int[] AngleDirections = new int[] { 0, 90, 180, 270 };

	private static readonly Vector2Int[] GridDirections = new Vector2Int[]
	{
		new(0, 1),  // North (0)
		new(1, 0),  // East (1)
		new(0, -1), // South (2)
		new(-1, 0)  // West (3)
	};

	public override int GetAngle(byte dir) => AngleDirections[dir];

	public override Vector2Int GetDirection(byte dir, int currentY = 0) => GridDirections[dir];

	public override Vector3 GridToWorldPosition(int x, int y) => new(x * CellSize, 0f, y * CellSize);

	public override byte Rotate(byte dir, int count) => (byte)((dir + (count % 4) + 4) % 4);

	public override Vector2Int WorldToGridPosition(Vector3 worldPos)
	{
		int x = Mathf.RoundToInt(worldPos.x / CellSize);
		int y = Mathf.RoundToInt(worldPos.z / CellSize);
		return new Vector2Int(x, y);
	}

	public enum SquareDirection : byte
	{
		North = 0,
		East = 1,
		South = 2,
		West = 3
	}
}