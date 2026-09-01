using UnityEngine;
using UnityEngine.EventSystems;

public class DragTransform : MonoBehaviour, IDragReceiver
{
	[SerializeField] private float _sensitivity = 0.5f;
	[SerializeField] private bool _useSnapping = false;
	[SerializeField] private float _snapInterval = 60f;
	private float _currentAngle = 0f;

	public void OnDragStart(PointerEventData eventData, Vector2 localMousePos)
	{
	}

	public void OnDragUpdate(PointerEventData eventData, Vector2 localMousePos)
	{
		Vector2 mouseDelta = eventData.delta;

		// Produit vectoriel 2D entre le vecteur (Centre -> Souris) et le vecteur Déplacement (Cross product en 2D)
		float angularDelta = (localMousePos.x * mouseDelta.y) - (localMousePos.y * mouseDelta.x);

		_currentAngle += Mathf.Repeat(angularDelta * _sensitivity * Time.deltaTime, 360f);

		transform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
	}

	public void OnDragEnd(PointerEventData eventData, Vector2 localMousePos)
	{
		if (_useSnapping && _snapInterval > 0f)
		{
			float snappedAngle = Mathf.Round(_currentAngle / _snapInterval) * _snapInterval;
			_currentAngle = Mathf.Repeat(snappedAngle, 360f);
		}

		transform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
	}
}
