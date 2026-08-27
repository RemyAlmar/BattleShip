using UnityEngine;

public interface IGridMovable
{
	public Transform Transform { get; }
	public float CurrentSpeed { get; }
	public int TargetSpeed { get; }
	public void SetTargetSpeed(int target);
	public void CalculateSpeed(bool _haveToStop = false);
	public void Move();
	public void Stop();
}
