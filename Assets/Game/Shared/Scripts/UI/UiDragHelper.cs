using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Shared.Scripts.UI
{
    public static class UiDragHelper
    {
        public static void Begin(CanvasGroup canvasGroup, Transform transform, Canvas dragCanvas, float dragAlpha)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = dragAlpha;

            transform.SetParent(dragCanvas.transform, true);
            transform.SetAsLastSibling();
        }

        public static void Move(RectTransform rectTransform, PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var worldPoint))
                return;

            rectTransform.position = worldPoint;
        }

        public static void End(CanvasGroup canvasGroup)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
    }
}
