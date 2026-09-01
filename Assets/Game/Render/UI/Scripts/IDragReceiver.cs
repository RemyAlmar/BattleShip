
using UnityEngine;
using UnityEngine.EventSystems;
public interface IDragReceiver
{
	public void OnDragStart(PointerEventData eventData, Vector2 localMousePos);
	public void OnDragUpdate(PointerEventData eventData, Vector2 localMousePos);
	public void OnDragEnd(PointerEventData eventData, Vector2 localMousePos);
}