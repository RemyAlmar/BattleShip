using System.Collections.Generic;
using UnityEngine;


namespace GridUtility
{
	public static class HexGridUtility
	{
		/// <summary>
		/// Calcule la trajectoire la plus directe entre deux cases sur la grille hexagonale.
		/// </summary>
		public static List<Vector2Int> GetLine(Vector2Int start, Vector2Int end)
		{
			Vector3Int cubeStart = OffsetToCube(start);
			Vector3Int cubeEnd = OffsetToCube(end);

			int distance = CubeDistance(cubeStart, cubeEnd);
			List<Vector2Int> line = new(distance + 1);

			for (int i = 0; i <= distance; i++)
			{
				float t = distance == 0 ? 0f : (float)i / distance;

				// Interpolation linéaire en coordonnées cubiques
				Vector3 floatCube = Vector3.Lerp(cubeStart, cubeEnd, t);

				line.Add(CubeToOffset(CubeRound(floatCube)));
			}

			return line;
		}

		/// <summary>
		/// Convertit les coordonnées 2D (Offset Odd-R) en coordonnées cubiques virtuelles (Q, R, S).
		/// </summary>
		/// <param name="hex">Position sur la grille (X = Colonne, Y = Ligne impaire décalée).</param>
		/// <returns>Coordonnées 3D virtuelles sous contrainte Q + R + S = 0.</returns>
		private static Vector3Int OffsetToCube(Vector2Int hex)
		{
			int q = hex.x - (hex.y - (hex.y & 1)) / 2;
			int r = hex.y;
			int s = -q - r;
			return new Vector3Int(q, r, s);
		}

		/// <summary>
		/// Convertit les coordonnées cubiques virtuelles (Q, R, S) en coordonnées 2D (Offset Odd-R).
		/// </summary>
		/// <param name="cube">Coordonnées 3D virtuelles où X=Q, Y=R, Z=S.</param>
		/// <returns>Position 2D dans le tableau de la grille.</returns>
		private static Vector2Int CubeToOffset(Vector3Int cube)
		{
			int x = cube.x + (cube.y - (cube.y & 1)) / 2;
			int y = cube.y;
			return new Vector2Int(x, y);
		}

		private static int CubeDistance(Vector3Int a, Vector3Int b) => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;

		private static Vector3Int CubeRound(Vector3 frac)
		{
			int q = Mathf.RoundToInt(frac.x);
			int r = Mathf.RoundToInt(frac.y);
			int s = Mathf.RoundToInt(frac.z);

			float qDiff = Mathf.Abs(q - frac.x);
			float rDiff = Mathf.Abs(r - frac.y);
			float sDiff = Mathf.Abs(s - frac.z);

			if (qDiff > rDiff && qDiff > sDiff)
				q = -r - s;
			else if (rDiff > sDiff)
				r = -q - s;
			else
				s = -q - r;

			return new Vector3Int(q, r, s);
		}
	}

	public static class SquareGridUtility
	{
		/// <summary>
		/// Trajectoire directe avec diagonales (8 directions).
		/// </summary>
		public static List<Vector2Int> GetLineChebyshev(Vector2Int start, Vector2Int end)
		{
			int distance = DistanceChebyshev(start, end);
			List<Vector2Int> line = new(distance + 1);

			for (int i = 0; i <= distance; i++)
			{
				float t = distance == 0 ? 0f : (float)i / distance;

				// En C#, Vector2 acceptant Vector2Int implicitement :
				Vector2 floatPos = Vector2.Lerp(start, end, t);

				line.Add(new Vector2Int(
					Mathf.RoundToInt(floatPos.x),
					Mathf.RoundToInt(floatPos.y)
				));
			}

			return line;
		}
		/// <summary>
		/// Trajectoire en escalier progressif (4 directions strictes).
		/// </summary>
		public static List<Vector2Int> GetLineManhattan(Vector2Int start, Vector2Int end)
		{
			List<Vector2Int> line = new();

			int dx = Mathf.Abs(end.x - start.x);
			int dy = Mathf.Abs(end.y - start.y);

			int stepX = start.x < end.x ? 1 : -1;
			int stepY = start.y < end.y ? 1 : -1;

			int currentX = start.x;
			int currentY = start.y;

			line.Add(new Vector2Int(currentX, currentY));

			// Accumulated error pour décider s'il faut avancer en X ou en Y
			int err = dx - dy;

			while (currentX != end.x || currentY != end.y)
			{
				int e2 = 2 * err;

				// On avance sur X ou Y, mais jamais les deux en même temps !
				if (e2 > -dy)
				{
					err -= dy;
					currentX += stepX;
					line.Add(new Vector2Int(currentX, currentY));
				}
				else if (e2 < dx)
				{
					err += dx;
					currentY += stepY;
					line.Add(new Vector2Int(currentX, currentY));
				}
			}

			return line;
		}

		/// <summary>
		/// Distance de Chebyshev (compte les diagonales comme 1 déplacement).
		/// </summary>
		public static int DistanceChebyshev(Vector2Int a, Vector2Int b) => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

		/// <summary>
		/// Distance de Manhattan (4 directions strictes).
		/// </summary>
		public static int DistanceManhattan(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

}