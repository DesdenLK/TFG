using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour//, IPointerDownHandler, IDragHandler, IScrollHandler
{
    public Camera minimapCamera;
    public float zoomSpeed = 10f;
    public float minZoom = 10f;
    public float maxZoom = 100f;
    public float panSpeed = 0.5f;

    //public MiniMapFollow followScript;

    //private Vector3 dragOrigin;

    //public void OnScroll(PointerEventData eventData)
    //{
    //    if (minimapCamera.orthographic)
    //    {
    //        float scroll = eventData.scrollDelta.y;
    //        float newSize = minimapCamera.orthographicSize - scroll * zoomSpeed * Time.deltaTime;
    //        minimapCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);

    //        if (followScript != null)
    //            followScript.isManualControl = true;
    //    }
    //}

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    RectTransformUtility.ScreenPointToWorldPointInRectangle(
    //        transform as RectTransform,
    //        eventData.position,
    //        eventData.pressEventCamera,
    //        out dragOrigin
    //    );

    //    if (followScript != null)
    //        followScript.isManualControl = true;
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    Vector3 currentPos;
    //    RectTransformUtility.ScreenPointToWorldPointInRectangle(
    //        transform as RectTransform,
    //        eventData.position,
    //        eventData.pressEventCamera,
    //        out currentPos
    //    );

    //    Vector3 delta = dragOrigin - currentPos;
    //    dragOrigin = currentPos;

    //    minimapCamera.transform.position += new Vector3(delta.x, 0f, delta.y) * panSpeed;

    //    if (followScript != null)
    //        followScript.isManualControl = true;
    //}
}

