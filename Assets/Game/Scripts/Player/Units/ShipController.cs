using CustomCore;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RenderProvider))]
public class ShipController : MonoBehaviour, IInitializable<ShipData_SO>, IGridOccupant
{
	[SerializeField] private ShipData_SO _data;
	private RenderProvider _render;

	public Transform Transform => transform;

	public Vector2Int Origin { get; private set; }
	public byte Direction { get; private set; }

	public int Size { get; private set; }

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
}

public interface IGridOccupant
{
	public Transform Transform { get; }
	public Vector2Int Origin { get; }
	public byte Direction { get; }
	public int Size { get; }
	public List<Vector2Int> GetOccupiedCells();
	public List<Vector2Int> GetOccupiedCellsAt(Vector2Int targetOrigin, byte targetDirection);
	public void SetGridPositionAndRotation(Vector2Int newOrigin, byte direction);
}