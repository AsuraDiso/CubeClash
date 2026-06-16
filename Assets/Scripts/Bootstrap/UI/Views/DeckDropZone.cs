using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bootstrap.UI.Views
{
    public sealed class DeckDropZone : MonoBehaviour, IDropHandler
    {
        public event Action<CardView, PointerEventData> OnCardDropped;

        private bool _droppedThisDrag;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null
                || !eventData.pointerDrag.TryGetComponent(out CardView cardView))
            {
                return;
            }

            _droppedThisDrag = true;
            OnCardDropped?.Invoke(cardView, eventData);
        }

        public bool ConsumeDrop()
        {
            var dropped = _droppedThisDrag;
            _droppedThisDrag = false;
            return dropped;
        }
    }
}
