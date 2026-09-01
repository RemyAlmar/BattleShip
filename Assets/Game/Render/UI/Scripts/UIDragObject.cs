using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragObject : MonoBehaviour, IInteractibleUI, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField] private Transform _targetTransform;
	private RectTransform _rectTransform;
	private IDragReceiver _draggableObject;
	public RectTransform RectTransform { get { return _rectTransform != null ? _rectTransform : _rectTransform = GetComponent<RectTransform>(); } }
	public bool IsInteractingWithUI { get; private set; }


	private void Awake()
	{
		_draggableObject = _targetTransform.GetComponentInChildren<IDragReceiver>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (_targetTransform == null || RectTransform == null) return;
		IsInteractingWithUI = true;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localMousePos);
		_draggableObject.OnDragStart(eventData, localMousePos);
	}
	public void OnDrag(PointerEventData eventData)
	{
		if (_targetTransform == null || RectTransform == null) return;
		// Convertir la position de la souris dans le repère local de l'élément UI draggable (origine (0,0) au centre)
		RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localMousePos);
		_draggableObject.OnDragUpdate(eventData, localMousePos);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (_targetTransform == null || RectTransform == null) return;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localMousePos);
		_draggableObject.OnDragEnd(eventData, localMousePos);
		IsInteractingWithUI = false;
	}
}