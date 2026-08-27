using Extensions;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FleetSpawner : MonoBehaviour
{
	[Header("Configuration")]
	[SerializeField] private ShipController _shipPrefab;
	[SerializeField] private List<ShipData_SO> _shipList = new();
	[SerializeField] private List<IGridOccupant> _fleet = new();

	[Header("Zone de Spawn (en % de la Map) donc rester de 0 à 100")]
	[SerializeField] private MinMax<Vector2Int> _spawnZonePercent = new(new(0, 0), new(100, 100));
	[SerializeField, Range(0, 5)] private int _spawnPadding = 1;

	public void SpawnFleet()
	{
		IGridService grid = GridMap.Instance;
		if (grid == null) return;

		CreateFleet();
		if (!PlaceFleet(grid))
		{
			DestroyFleet();
			Debug.LogError("[SPAWN] Aucune cellule disponible pour placer la flotte");
		}
	}

	private void CreateFleet()
	{
		for (int i = 0; i < _shipList.Count; i++)
		{
			ShipController shipController = Instantiate(_shipPrefab);
			shipController.Initialize(_shipList[i]);
			shipController.name = _shipList[i].Data.Name;
			_fleet.Add(shipController);
		}
	}
	private void DestroyFleet()
	{
		for (int i = _fleet.Count - 1; i >= 0; i--)
		{
			IGridOccupant ship = _fleet[i];
			_fleet.RemoveAt(i);
			Destroy(ship.Transform.gameObject);
		}
	}

	private bool PlaceFleet(IGridService grid = null)
	{
		grid ??= GridMap.Instance;
		bool isPlaced = false;
		MinMax<Vector2Int> rangeCell = _spawnZonePercent.ToAbsolute(grid.MapSize);
		List<Cell> cells = grid.GetCellsInRange(rangeCell.Min, rangeCell.Max);
		int spawnPadding = _spawnPadding;
		_fleet.Sort((a, b) => b.Size.CompareTo(a.Size));
		for (int padding = spawnPadding; padding >= 0; padding--)
		{
			if (TryPlaceFleet(_fleet, cells, padding, grid))
			{
				isPlaced = true;
				break;
			}
			else
				grid.ClearOccupantsCells(_fleet);
		}
		return isPlaced;
	}
	private bool TryPlaceFleet(List<IGridOccupant> ships, List<Cell> cellsToPlace, int padding = 0, IGridService grid = null)
	{
		grid ??= GridMap.Instance;

		HashSet<Vector2Int> cellsPadded = new();
		List<Cell> freeCells = grid.GetFreeCells(cellsToPlace);
		freeCells.Shuffle();
		byte direction = (byte)HexaMetrics.HexaDirection.NorthEast;
		for (int s = 0; s < ships.Count; s++)
		{
			bool shipPlaced = false;
			IGridOccupant ship = ships[s];

			for (int i = 0; i < freeCells.Count; i++)
			{
				Cell cell = freeCells[i];
				if (!cellsPadded.Contains(cell.GridPosition) && grid.TryPlaceOccupant(ship, cell.GridPosition, direction))
				{
					ship.SetGridPositionAndRotation(cell.GridPosition, direction);
					cellsPadded.AddRange(grid.GetNeighbors(ship.GetOccupiedCells(), padding));
					shipPlaced = true;
					break;
				}
			}
			if (!shipPlaced)
				return false;
		}

		return true;
	}
}
