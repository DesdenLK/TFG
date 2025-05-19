using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapFollow : MonoBehaviour
{
    private Transform target;

    public Transform firstPerson;
    public Transform vrPerson;

    public CameraModeManager cameraModeManager;
    public Camera minimapCamera;

    public float followThreshold = 0.01f;
    public float zoomSpeed = 30f;
    public float minZoom = 10f;
    public float maxZoom = 1000f;
    public float dragSpeed = 5f;

    public InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction zoomInAction;
    private InputAction zoomOutAction;

    private bool manualControl = false;
    private Vector3 lastTargetPosition;

    void Start()
    {
        if (cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson)
        {
            target = firstPerson;
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.VR)
        {
            target = vrPerson;
        }
        else
        {
            target = firstPerson;
        }


        if (target == null || minimapCamera == null)
        {
            Debug.LogError("Minimap target o cámara no asignados.");
            enabled = false;
            return;
        }

        lastTargetPosition = target.position;


        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Minimap", true);
            zoomInAction = map.FindAction("ZoomIn");
            zoomOutAction = map.FindAction("ZoomOut");
            moveAction = map.FindAction("Move");

            zoomInAction?.Enable();
            zoomOutAction?.Enable();
            moveAction?.Enable();
        }
    }

    void Update()
    {

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        if (cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson)
        {
            target = firstPerson;
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.VR)
        {
            target = vrPerson;
        }
        else
        {
            target = firstPerson;
        }
    }

    void LateUpdate()
    {

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        float zoomIn = zoomInAction?.ReadValue<float>() ?? 0f;
        float zoomOut = zoomOutAction?.ReadValue<float>() ?? 0f;
        float vrZoom = zoomIn - zoomOut;

        float totalZoom = scroll + vrZoom * Time.deltaTime * 2f;

        if (Mathf.Abs(totalZoom) > 0.001f)
        {
            float newSize = minimapCamera.orthographicSize - totalZoom * zoomSpeed;
            minimapCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }


        if (Input.GetMouseButton(2))
        {
            manualControl = true;
            float dragX = -Input.GetAxis("Mouse X") * dragSpeed;
            float dragZ = -Input.GetAxis("Mouse Y") * dragSpeed;
            transform.position += new Vector3(dragX, 0, dragZ);
        }


        Vector2 moveInput = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (moveInput.magnitude > 0.1f)
        {
            manualControl = true;
            Vector3 drag = new Vector3(moveInput.x * 20, 0, moveInput.y * 20) * dragSpeed * Time.deltaTime;
            transform.position += drag;
        }


        Vector3 playerDelta = target.position - lastTargetPosition;
        if (playerDelta.magnitude > followThreshold)
            manualControl = false;

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
