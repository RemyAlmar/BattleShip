using System;

namespace Game.Render.UI.Scripts
{
    public interface IDragProvider
    {
        public event Action<DragData, float> OnDragEvent;
    }
}