using UnityEngine;

public interface IInteractibleUI
{
	public RectTransform RectTransform { get; }
	public bool IsInteractingWithUI { get; }
}
