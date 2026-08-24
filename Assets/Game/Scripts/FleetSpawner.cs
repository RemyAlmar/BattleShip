using System.Collections.Generic;
using UnityEngine;

public class FleetSpawner : MonoBehaviour
{
	[Header("Configuration")]
	[SerializeField] private ShipController _shipPrefab;
	[SerializeField] private List<ShipData_SO> _shipList = new();

	[Header("Zone de Spawn (en % de la Map)")]
	[SerializeField] private Vector2Int _spawnZoneMinPercent = new Vector2Int(0, 0);
	[SerializeField] private Vector2Int _spawnZoneMaxPercent = new Vector2Int(100, 30);

	public void SpawnFleet()
	{
		IGridService grid = GridMap.Instance;
		if (grid == null) return;

		// Copie et mélange la liste des bateaux pour ne pas toujours spawner les mêmes types en premier
		List<ShipData_SO> shuffledShips = new(_shipList);
		ShuffleList(shuffledShips);

		foreach (ShipData_SO data in shuffledShips)
		{
			ShipController shipInstance = Instantiate(_shipPrefab);
			shipInstance.Initialize(data);

			if (!TryFindValidSpawn(grid, shipInstance, out Vector2Int validOrigin, out byte validDir))
			{
				Debug.LogWarning($"[Spawner] Impossible de placer {data.name}, zone saturée !");
				Destroy(shipInstance.gameObject);
				continue;
			}

			grid.TryPlaceOccupant(shipInstance, validOrigin, validDir);
		}
	}

	private bool TryFindValidSpawn(IGridService grid, IGridOccupant occupant, out Vector2Int origin, out byte direction)
	{
		Vector2Int minCell = new Vector2Int(
			(grid.MapSize.x * _spawnZoneMinPercent.x) / 100,
			(grid.MapSize.y * _spawnZoneMinPercent.y) / 100
		);

		Vector2Int maxCell = new Vector2Int(
			(grid.MapSize.x * _spawnZoneMaxPercent.x) / 100,
			(grid.MapSize.y * _spawnZoneMaxPercent.y) / 100
		);

		// 1. On génère toutes les cases de la zone et ON LES MÉLANGE
		List<Vector2Int> candidatePositions = GetZonePositions(minCell, maxCell);
		ShuffleList(candidatePositions);

		// PASSE 1 : Avec Padding (Espace de sécurité)
		if (SearchZone(grid, occupant, candidatePositions, usePadding: true, out origin, out direction))
			return true;

		// PASSE 2 : Secours sans Padding si c'est blindé
		return SearchZone(grid, occupant, candidatePositions, usePadding: false, out origin, out direction);
	}

	private bool SearchZone(IGridService grid, IGridOccupant occupant, List<Vector2Int> candidatePositions, bool usePadding, out Vector2Int origin, out byte direction)
	{
		foreach (Vector2Int candidateOrigin in candidatePositions)
		{
			// Point de départ de direction aléatoire (entre 0 et 5)
			byte startDir = (byte)Random.Range(0, 6);

			for (int i = 0; i < 6; i++)
			{
				// Teste les 6 directions en partant du décalage aléatoire
				byte testDir = (byte)((startDir + i) % 6);

				IReadOnlyList<Vector2Int> shipCells = occupant.GetOccupiedCellsAt(candidateOrigin, testDir);
				HashSet<Vector2Int> cellsToCheck = usePadding ? GetCellsWithPadding(grid, shipCells) : new HashSet<Vector2Int>(shipCells);

				if (IsSpaceFree(grid, cellsToCheck, shipCells))
				{
					origin = candidateOrigin;
					direction = testDir;
					return true;
				}
			}
		}

		origin = Vector2Int.zero;
		direction = 0;
		return false;
	}

	private List<Vector2Int> GetZonePositions(Vector2Int min, Vector2Int max)
	{
		List<Vector2Int> positions = new();
		for (int y = min.y; y <= max.y; y++)
		{
			for (int x = min.x; x <= max.x; x++)
			{
				positions.Add(new Vector2Int(x, y));
			}
		}
		return positions;
	}

	private HashSet<Vector2Int> GetCellsWithPadding(IGridService grid, IReadOnlyList<Vector2Int> shipCells)
	{
		HashSet<Vector2Int> paddedCells = new(shipCells);

		foreach (Vector2Int cell in shipCells)
		{
			for (byte dir = 0; dir < 6; dir++)
			{
				Vector2Int neighborOffset = grid.GetDirection(dir, cell.y);
				paddedCells.Add(cell + neighborOffset);
			}
		}

		return paddedCells;
	}

	private bool IsSpaceFree(IGridService grid, IEnumerable<Vector2Int> cells, IReadOnlyList<Vector2Int> actualShipCells)
	{
		// 1. Les cases REELLES du bateau doivent OBLIGATOIREMENT être valides et sur la map
		foreach (Vector2Int cell in actualShipCells)
		{
			if (!grid.IsValidCell(cell.x, cell.y) || grid.IsCellOccupied(cell.x, cell.y))
				return false;
		}

		// 2. Les cases de PADDING (s'il y en a) ne doivent pas être occupées si elles sont sur la map
		foreach (Vector2Int cell in cells)
		{
			if (grid.IsValidCell(cell.x, cell.y) && grid.IsCellOccupied(cell.x, cell.y))
				return false;
		}

		return true;
	}

	// Fisher-Yates Shuffle genérique
	private void ShuffleList<T>(List<T> list)
	{
		for (int i = list.Count - 1; i > 0; i--)
		{
			int randomIndex = Random.Range(0, i + 1);
			(list[i], list[randomIndex]) = (list[randomIndex], list[i]);
		}
	}
}