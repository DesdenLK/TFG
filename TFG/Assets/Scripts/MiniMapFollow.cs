using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapFollow : MonoBehaviour
{
    private Transform target;

    public Transform firstPerson;
    public Transform vrPerson;

    public CameraModeManager cameraModeManager;
    public Camera minimapCamera;

    public float followThreshold = 0.01f; // Distància mínima per considerar que el jugador s'ha mogut
    public float zoomSpeed = 30f; // Velocitat de zoom de la càmera del minimapa
    public float minZoom = 10f;  // Mínim zoom de la càmera del minimapa
    public float maxZoom = 1000f; // Màxim zoom de la càmera del minimapa
    public float dragSpeed = 5f; // Velocitat de moviment del minimapa quan es fa drag

    public InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction zoomInAction;
    private InputAction zoomOutAction;

    private bool manualControl = false;
    private Vector3 lastTargetPosition;

    void Start()
    {
        // Obté el jugador en funció del mode de càmera actual
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


        // Mira que les accions d'entrada de la càmera en VR estan definides
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
        // Fixa la càmera del minimapa per sobre del jugador
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
        // Obté el valor del input de la roda del ratolí i les accions de zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        float zoomIn = zoomInAction?.ReadValue<float>() ?? 0f;
        float zoomOut = zoomOutAction?.ReadValue<float>() ?? 0f;
        float vrZoom = zoomIn - zoomOut;

        // Calcula el zoom total
        float totalZoom = scroll + vrZoom * Time.deltaTime * 2f;

        // Si hi ha accions de zoom, ajusta la càmera del minimapa
        if (Mathf.Abs(totalZoom) > 0.001f)
        {
            // Ajusta la mida de la càmera del minimapa
            float newSize = minimapCamera.orthographicSize - totalZoom * zoomSpeed;
            minimapCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }

        // Si es fa clic al botó del mig del ratolí, permet el moviment manual del minimapa
        if (Input.GetMouseButton(2))
        {
            manualControl = true;
            float dragX = -Input.GetAxis("Mouse X") * dragSpeed;
            float dragZ = -Input.GetAxis("Mouse Y") * dragSpeed;
            transform.position += new Vector3(dragX, 0, dragZ);
        }

        // Si s'esta prement el boto del controlador de moviment, permet el moviment manual del minimapa
        Vector2 moveInput = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (moveInput.magnitude > 0.1f)
        {
            manualControl = true;
            Vector3 drag = new Vector3(moveInput.x * 20, 0, moveInput.y * 20) * dragSpeed * Time.deltaTime;
            transform.position += drag;
        }

        // Si el jugador s'ha mogut més enllà del llindar, desactiva el control manual
        Vector3 playerDelta = target.position - lastTargetPosition;
        if (playerDelta.magnitude > followThreshold)
            manualControl = false;

        // Si no s'està fent control manual, segueix al jugador
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
