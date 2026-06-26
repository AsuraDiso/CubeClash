using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Features.Deck.Scripts
{
    public sealed class DeckDropZone : MonoBehaviour, IDropHandler
    {
        public event Action<CardView, PointerEventData> CardDropped;

        private bool _droppedThisDrag;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null || !eventData.pointerDrag.TryGetComponent(out CardView cardView))
                return;

            _droppedThisDrag = true;
            CardDropped?.Invoke(cardView, eventData);
        }

        public bool ConsumeDrop()
        {
            var dropped = _droppedThisDrag;
            _droppedThisDrag = false;
            return dropped;
        }

        public void ResetDropFlag() => _droppedThisDrag = false;
    }
}
