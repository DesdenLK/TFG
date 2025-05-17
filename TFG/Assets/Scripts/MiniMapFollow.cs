using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform target;
    public Camera minimapCamera; // <- La cámara que hace de minimapa

    public float followThreshold = 0.01f;
    public float zoomSpeed = 100f;
    public float minZoom = 10f;
    public float maxZoom = 10000f;
    public float dragSpeed = 50f;


    private bool manualControl = false;
    private Vector3 lastTargetPosition;

    void Start()
    {
        if (target == null || minimapCamera == null)
        {
            Debug.LogError("Minimap target o cámara no asignados.");
            enabled = false;
            return;
        }

        lastTargetPosition = target.position;
    }

    private void Update()
    {
        // Vista fija desde arriba
        transform.rotation = Quaternion.identity;
        transform.Rotate(90f, 0f, 0f);
    }

    void LateUpdate()
    {
        // Zoom con la rueda del ratón (ajustando el tamaño ortográfico)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = minimapCamera.orthographicSize - scroll * zoomSpeed;
            minimapCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }

        // Arrastrar con botón medio
        if (Input.GetMouseButton(2))
        {
            manualControl = true;
            float dragX = -Input.GetAxis("Mouse X") * dragSpeed;
            float dragZ = -Input.GetAxis("Mouse Y") * dragSpeed;
            transform.position += new Vector3(dragX, 0, dragZ);
        }

        // Volver a seguir si el jugador se mueve
        Vector3 playerDelta = target.position - lastTargetPosition;
        if (playerDelta.magnitude > followThreshold)
        {
            manualControl = false;
        }

        // Si no está en control manual, seguir al jugador
        if (!manualControl)
        {
            Vector3 camPos = transform.position;
            camPos.x = target.position.x;
            camPos.z = target.position.z;
            transform.position = camPos;
        }

        lastTargetPosition = target.position;
    }
}
