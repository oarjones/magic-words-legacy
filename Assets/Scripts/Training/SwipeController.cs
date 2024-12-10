using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //private Vector2 dragStartPos;

    //public static Vector2 dragDirection;
    //public static float dragDistance;

    private Vector2 dragStartPos;

    public static SwipeDirection dragDirection = SwipeDirection.None;
    public static float dragDistance;

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPos = eventData.position;
    }





    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 dragEndPos = eventData.position;
        float dragDeltaX = dragEndPos.x - dragStartPos.x;

        dragDirection = GetSwipeDirection(dragDeltaX);
        dragDistance = Mathf.Abs(dragDeltaX);

        Debug.Log("Dirección del desplazamiento: " + dragDirection);
        Debug.Log("Cantidad del desplazamiento: " + dragDistance);
    }


    private SwipeDirection GetSwipeDirection(float deltaX)
    {
        if (deltaX > 0)
        {
            return SwipeDirection.Right;
        }
        else if (deltaX < 0)
        {
            return SwipeDirection.Left;
        }
        return SwipeDirection.None;
    }

}
