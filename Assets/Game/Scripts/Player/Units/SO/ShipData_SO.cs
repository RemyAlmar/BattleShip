using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipData", menuName = "Scriptable Objects/Game/Units/Ship")]
public class ShipData_SO : ScriptableObject
{
	[SerializeField] private ShipData _data;
	public ShipData Data => _data;
	private int _id = -1;
	public int Id
	{
		get
		{
			if (_id == -1)
				_id = Animator.StringToHash(Data.Name);
			return _id;
		}
	}
	public Mesh Mesh;
	public Material Material;
	public Color Color = Color.white;
}

[Serializable]
public struct ShipData
{
	public string Name;
	[Min(0), Tooltip("Taille en cellule du navire")] public int Size;
	[Min(0)] public int SpeedMax;
	[Min(0), Tooltip("Taux d'accéleration par tour")] public float AccelerationRate;
	[Min(0), Tooltip("Taux de decéleration par tour")] public float DecelerationRate;
	[Min(0), Tooltip("Taux de freinage par tour")] public float Braking;
	[Min(0), Tooltip("Nombre de cran max pour tourner par tour")] public int TurnMax;
	[Range(0, 100), Tooltip("Pourcentage de chance d'avoir un surrégime selon la vitesse")] public float RiskFactor;

}