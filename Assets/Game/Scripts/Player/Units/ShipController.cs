using CustomCore;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RenderProvider))]
public class ShipController : MonoBehaviour, IInitializable<ShipData_SO>, IGridOccupant, IGridMovable, IGridTickable
{
	[SerializeField] private ShipData_SO _data;
	private RenderProvider _render;
	private int _accumulatedDistance = 0;

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

		IGridService grid = GridMap.Instance;
		if (grid != null)
		{
			Vector3 targetWorldPos = grid.GridToWorldPosition(newOrigin);
			Quaternion targetRotation = grid.DirectionToWorldRotation(newDirection);

			transform.SetPositionAndRotation(targetWorldPos, targetRotation);
		}
	}


	private void SetTargetSpeed(int target) => TargetSpeed = Mathf.Clamp(target, -1, _data.Data.SpeedMax);
	public void ExecuteOrder(MoveOrder order)
	{
		IGridService grid = GridMap.Instance;
		if (_data == null || grid == null) return;

		if (order.EmergencyStop)
		{
			TryEmergencyStop();
			return;
		}

		SetTargetSpeed(order.TargetSpeed);
		CalculateSpeed();

		// Reçoit une List<PathNode> au lieu de List<Vector2Int>
		List<PathNode> path = CalculateCurvedPath(grid, order.RequestedTurn);
		if (path.Count <= 1) return;

		PathNode destinationNode = ResolveMovementPath(grid, path);
		ApplyMovement(grid, destinationNode);
	}
	private void TryEmergencyStop()
	{
		float risk = Mathf.Clamp(_data.Data.RiskFactor * CurrentSpeed, 0f, 100f);
		float roll = Random.value * 100;
		if (roll <= risk)
			ApplyStructuralDamage();
		Stop();
	}

	private void ApplyStructuralDamage()
	{
		Debug.LogWarning($"{name} a subi des dégâts de structure suite au freinage d'urgence !");
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
	private List<PathNode> CalculateCurvedPath(IGridService grid, int requestedTurn)
	{
		List<PathNode> path = new() { new(Origin, Direction, _accumulatedDistance) };
		Vector2Int currentCell = Origin;
		byte currentDir = Direction;

		int turnSign = System.Math.Sign(requestedTurn);
		int remainingTurns = System.Math.Abs(requestedTurn);

		int tempAccumulatedDistance = _accumulatedDistance;
		int stepPerTurn = _data.Data.StepsPerTurn;

		if (stepPerTurn == 0 && remainingTurns > 0)
		{
			currentDir = grid.GetRotation(currentDir, turnSign * remainingTurns);
			remainingTurns = 0;
		}

		for (int step = 0; step < (int)CurrentSpeed; step++)
		{
			tempAccumulatedDistance++;
			if (stepPerTurn > 0 && remainingTurns > 0 && tempAccumulatedDistance >= stepPerTurn)
			{
				currentDir = grid.GetRotation(currentDir, turnSign);
				remainingTurns--;
				tempAccumulatedDistance = 0;
			}
			Vector2Int dirVector = grid.GetDirection(currentDir, currentCell.y);
			Vector2Int nextCell = currentCell + dirVector;

			if (!grid.IsValidCell(nextCell.x, nextCell.y))
				break;

			currentCell = nextCell;
			path.Add(new(currentCell, currentDir, tempAccumulatedDistance));
		}

		return path;
	}
	private PathNode ResolveMovementPath(IGridService grid, List<PathNode> path)
	{
		PathNode lastValidNode = path[0];

		for (int i = 1; i < path.Count; i++)
		{
			PathNode candidateNode = path[i];

			if (grid.TryGetOccupant(candidateNode.Position, out IGridOccupant targetOccupant) && targetOccupant != (IGridOccupant)this)
			{
				Hit(targetOccupant);
				return lastValidNode;
			}
			lastValidNode = candidateNode;
		}
		return lastValidNode;
	}

	private void ApplyMovement(IGridService grid, PathNode destinationNode)
	{
		if (destinationNode.Position == Origin) return;

		_accumulatedDistance = destinationNode.AccumulatedDistance;

		if (grid.TryPlaceOccupant(this, destinationNode.Position, destinationNode.Direction))
		{
			SetGridPositionAndRotation(destinationNode.Position, destinationNode.Direction);
		}
	}
	private void CalculateSpeed()
	{
		if (_data == null) return;
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
	#endregion
	private void Hit(IGridOccupant targetOccupant)
	{
		Debug.Log($"{name} Hit {targetOccupant.Transform.name}");
	}

	public void Stop()
	{
		CurrentSpeed = 0;
		TargetSpeed = 0;
		_accumulatedDistance = 0;
	}

	public void Tick(int tick)
	{
	}
}
