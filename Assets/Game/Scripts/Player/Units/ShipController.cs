using CustomCore;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RenderProvider))]
public class ShipController : MonoBehaviour, IInitializable<ShipData_SO>, IGridOccupant, IGridMovable, IGridTickable
{
	[SerializeField] private ShipData_SO _data;
	private RenderProvider _render;

	public Transform Transform => transform;
	public Vector2Int Origin { get; private set; }
	public byte Direction { get; private set; }
	public int Size { get; private set; }
	public float CurrentSpeed { get; private set; }
	public int TargetSpeed { get; private set; }

	private void Awake()
	{
		_render = GetComponent<RenderProvider>();
		if (_data != null)
			Initialize(_data);
	}
	public void Initialize(ShipData_SO data)
	{
		if (data == null || _render == null) return;

		_data = data;
		_render.SetMesh(_data.Mesh);
		_render.SetMaterial(_data.Material);
		_render.SetColor(_data.Color);

		Size = _data.Data.Size;
	}

	public List<Vector2Int> GetOccupiedCells() => GetOccupiedCellsAt(Origin, Direction);

	public List<Vector2Int> GetOccupiedCellsAt(Vector2Int origin, byte direction)
	{
		List<Vector2Int> occupied = new();
		for (int i = 0; i < _data.Data.Size; i++)
		{
			Vector2Int dirOffset = GridMap.Instance.GetDirection(direction, origin.y);
			occupied.Add(origin);
			origin += dirOffset;
		}

		return occupied;
	}

	public void SetGridPositionAndRotation(Vector2Int newOrigin, byte newDirection)
	{
		Origin = newOrigin;
		Direction = newDirection;

		// Le bateau demande au service de grille les vraies coordonnées 3D
		IGridService grid = GridMap.Instance;
		if (grid != null)
		{
			Vector3 targetWorldPos = grid.GridToWorldPosition(newOrigin);
			Quaternion targetRotation = grid.DirectionToWorldRotation(newDirection);

			transform.SetPositionAndRotation(targetWorldPos, targetRotation);
		}
	}


	public void CalculateSpeed(bool _haveToStop = false)
	{
		if (_data == null) return;

		if (_haveToStop)
		{
			Stop();
			return;
		}

		if (CurrentSpeed < TargetSpeed)
		{
			CurrentSpeed = Mathf.Min(TargetSpeed, CurrentSpeed + _data.Data.AccelerationRate);
		}
		else
		{
			float reduction = TargetSpeed < 0 ? _data.Data.DecelerationRate + _data.Data.Braking : _data.Data.DecelerationRate;
			CurrentSpeed = Mathf.Max(Mathf.Max(0, TargetSpeed), CurrentSpeed - reduction);
		}
	}
	public void SetTargetSpeed(int target)
	{
		TargetSpeed = Mathf.Clamp(target, -1, _data.Data.SpeedMax);
	}
	public void Move()
	{
		IGridService grid = GridMap.Instance;
		if (_data == null || grid == null) return;

		List<Vector2Int> path = CalculatePath(grid);
		if (path.Count <= 1) return;

		Vector2Int destination = ResolveMovementPath(grid, path);
		ApplyMovement(grid, destination);
	}

	#region MoveFunctionHelpers
	private List<Vector2Int> CalculatePath(IGridService grid)
	{
		Vector2Int currentCell = Origin;
		Vector2Int targetCell = Origin;

		for (int step = 0; step < (int)CurrentSpeed; step++)
		{
			Vector2Int dirVector = grid.GetDirection(Direction, currentCell.y);
			Vector2Int nextCell = currentCell + dirVector;

			if (!grid.IsValidCell(nextCell.x, nextCell.y))
				break;

			currentCell = nextCell;
			targetCell = nextCell;
		}

		return (targetCell == Origin) ? new List<Vector2Int>() : grid.GetLine(Origin, targetCell);
	}
	private Vector2Int ResolveMovementPath(IGridService grid, List<Vector2Int> path)
	{
		Vector2Int lastValidPos = Origin;

		for (int i = 1; i < path.Count; i++)
		{
			Vector2Int candidateCell = path[i];

			if (grid.TryGetOccupant(candidateCell, out IGridOccupant targetOccupant))
			{
				Hit(targetOccupant);
				return lastValidPos;
			}

			lastValidPos = candidateCell;
		}

		return lastValidPos;
	}
	private void ApplyMovement(IGridService grid, Vector2Int destination)
	{
		if (destination == Origin) return;

		if (grid.TryPlaceOccupant(this, destination, Direction))
		{
			SetGridPositionAndRotation(destination, Direction);
		}
	}
	#endregion
	private void Hit(IGridOccupant targetOccupant)
	{
		Debug.Log($"{name} Hit {targetOccupant.Transform.name}");
	}

	public void Stop()
	{
		CurrentSpeed = 0;
		TargetSpeed = 0;
	}

	public void Tick(int tick)
	{
	}
}