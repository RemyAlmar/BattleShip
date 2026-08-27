using UnityEngine;

public interface IGridMovable
{
	public Transform Transform { get; }
	public float CurrentSpeed { get; }
	public void ExecuteOrder(MoveOrder order);
}


public struct MoveOrder
{
	public int TargetSpeed;
	public int RequestedTurn;
	public bool EmergencyStop;

	public MoveOrder(int targetSpeed, int requestedTurn, bool emergencyStop = false)
	{
		TargetSpeed = targetSpeed;
		RequestedTurn = requestedTurn;
		EmergencyStop = emergencyStop;
	}
}

public struct PathNode
{
	public Vector2Int Position;
	public byte Direction;
	public int AccumulatedDistance;

	public PathNode(Vector2Int position, byte direction, int accumulatedDistance)
	{
		Position = position;
		Direction = direction;
		AccumulatedDistance = accumulatedDistance;
	}
}