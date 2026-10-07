using System;
using UnityEngine;

namespace Game.Render.UI.Scripts
{
    public class DragProvider : MonoBehaviour, IDragProvider
    {
        [SerializeField] private Transform _draggableTransform;
        private IDraggable _draggable;
        [SerializeField] private float _sensitivity = 0.5f;
        [SerializeField] private bool _useSnapping;
        [SerializeField] private float _snapInterval = 60f;
        private float _currentAngle;

        public event Action<DragData, float> OnDragEvent;

        private void Awake()
        {
            if (_draggableTransform != null) _draggable = _draggableTransform.GetComponent<IDraggable>();
        }

        private void OnEnable()
        {
            _draggable.OnDragEvent += OnDrag;
        }

        private void OnDisable()
        {
            _draggable.OnDragEvent -= OnDrag;
        }

        private void OnDrag(DragData data)
        {
            switch (data.DragState)
            {
                case DragData.State.Begin:  DragStart(data); break;
                case DragData.State.Update: DragUpdate(data); break;
                case DragData.State.End:    DragEnd(data); break;
            }
        }

        private void DragStart(DragData data) { OnDragEvent?.Invoke(data, _currentAngle); }

        private void DragUpdate(DragData data)
        {
            Vector2 mouseDelta = data.EventData.delta;
            // Produit vectoriel 2D entre le vecteur (Centre -> Souris) et le vecteur Déplacement (Cross product en 2D)
            float angularDelta = data.LocalMousePos.x * mouseDelta.y - data.LocalMousePos.y * mouseDelta.x;

            _currentAngle += Mathf.Repeat(angularDelta * _sensitivity * Time.deltaTime, 360f);
            OnDragEvent?.Invoke(data, _currentAngle);
        }

        private void DragEnd(DragData data)
        {
            if (_useSnapping && _snapInterval > 0f)
            {
                float snappedAngle = Mathf.Round(_currentAngle / _snapInterval) * _snapInterval;
                _currentAngle = Mathf.Repeat(snappedAngle, 360f);
            }

            OnDragEvent?.Invoke(data, _currentAngle);
        }
    }
}