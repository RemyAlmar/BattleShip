using System;

namespace Game.Render.UI.Scripts
{
    public interface IDraggable
    {
        public event Action<DragData> OnDragEvent;
    }
}