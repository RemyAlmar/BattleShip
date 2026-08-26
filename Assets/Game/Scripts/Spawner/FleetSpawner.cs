using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FleetSpawner : MonoBehaviour
{
	[Header("Configuration")]
	[SerializeField] private ShipController _shipPrefab;
	[SerializeField] private List<ShipData_SO> _shipList = new();

	[Header("Zone de Spawn (en % de la Map) donc rester de 0 à 100")]
	[SerializeField] private MinMax<Vector2Int> _spawnZonePercent = new(new(0, 0), new(100, 100));
	[SerializeField, Range(1, 5)] private int _spawnPadding = 1;

	public void SpawnFleet()
	{
		IGridService grid = GridMap.Instance;
		if (grid == null) return;

		// Récupère la zone de la map dans laquelle on veut spawn
		MinMax<Vector2Int> rangeCell = _spawnZonePercent.ToAbsolute(grid.MapSize);
		List<Cell> cells = grid.GetCellsInRange(rangeCell.Min, rangeCell.Max);
		HashSet<Vector2Int> padding = new();
		int spawnPadding = _spawnPadding;
		for (int i = 0; i < _shipList.Count; i++)
		{
			ShipController shipController = Instantiate(_shipPrefab);
			shipController.Initialize(_shipList[i]);
			List<Cell> freeCells = grid.GetFreeCells(cells);
			bool isPlaced = false;
			for (int sPadding = spawnPadding; sPadding >= 1; sPadding--)
			{
				for (int c = 0; c < freeCells.Count; c++)
				{
					Cell cell = freeCells[c];
					if (!padding.Contains(cell.GridPosition) && grid.TryPlaceOccupant(shipController, cell.GridPosition, (byte)HexaMetrics.HexaDirection.NorthEast))
					{
						padding.AddRange(grid.GetNeighbors(shipController.GetOccupiedCells(), _spawnPadding));
						isPlaced = true;
						break;
					}
				}
				if (isPlaced)
					break;
			}
		}
	}
}
