using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Render.UI.Scripts
{
    public class UIDragObject : MonoBehaviour, IInteractibleUI, IBeginDragHandler, IDragHandler, IEndDragHandler, IDraggable
    {
        private RectTransform _rectTransform;
        public bool IsInteractingWithUI { get; private set; }
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : _rectTransform = GetComponent<RectTransform>();
        public event Action<DragData> OnDragEvent;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (RectTransform == null) return;
            IsInteractingWithUI = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera,
                out Vector2 localMousePos);
            DragData data = new(eventData, localMousePos, DragData.State.Begin);
            OnDragEvent?.Invoke(data);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransform == null) return;
            // Convertir la position de la souris dans le repère local de l'élément UI draggable (origine (0,0) au centre)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera,
                out Vector2 localMousePos);
            DragData data = new(eventData, localMousePos, DragData.State.Update);
            OnDragEvent?.Invoke(data);
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            if (RectTransform == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera,
                out Vector2 localMousePos);
            DragData data = new(eventData, localMousePos, DragData.State.End);
            OnDragEvent?.Invoke(data);
            IsInteractingWithUI = false;
        }
    }

    public struct DragData
    {
        public PointerEventData EventData;
        public Vector2 LocalMousePos;
        public State DragState;

        public DragData(PointerEventData eventData, Vector2 localMousePos, State dragState)
        {
            EventData = eventData;
            LocalMousePos = localMousePos;
            DragState = dragState;
        }

        public enum State
        {
            Begin,
            Update,
            End,
        }
    }
}