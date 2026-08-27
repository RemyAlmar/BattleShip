using UnityEngine;

public interface IGridTickable
{
	public Transform Transform { get; }
	public void Tick(int tick);
}