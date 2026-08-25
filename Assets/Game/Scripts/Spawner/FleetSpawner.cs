using System.Collections.Generic;
using UnityEngine;

public class FleetSpawner : MonoBehaviour
{
	[Header("Configuration")]
	[SerializeField] private ShipController _shipPrefab;
	[SerializeField] private List<ShipData_SO> _shipList = new();

	[Header("Zone de Spawn (en % de la Map) donc rester de 0 à 100")]
	[SerializeField] private MinMax<Vector2Int> _spawnZonePercent = new(new(0, 0), new(100, 100));

	public void SpawnFleet()
	{
		IGridService grid = GridMap.Instance;
		if (grid == null) return;

		// Récupère la zone de la map dans laquelle on veut spawn
		MinMax<Vector2Int> rangeCell = _spawnZonePercent.ToAbsolute(grid.MapSize);
		List<Cell> cells = grid.GetCellsInRange(rangeCell.Min, rangeCell.Max);

		for (int i = 0; i < _shipList.Count; i++)
		{
			ShipController shipController = Instantiate(_shipPrefab);
			shipController.Initialize(_shipList[i]);
			List<Cell> freeCells = grid.GetFreeCells(cells);

			for (int c = 0; c < freeCells.Count; c++)
			{
				Cell cell = freeCells[c];
				if (grid.TryPlaceOccupant(shipController, cell.GridPosition, (byte)HexaMetrics.HexaDirection.NorthEast))
					break;
			}
		}
	}
}
