using UnityEngine;

public class CameraModeManager : MonoBehaviour
{
    public GameObject thirdPersonCamera; // Tu cámara aérea
    public GameObject firstPersonPlayer; // Prefab o escena

    public enum Mode { ThirdPerson, FirstPerson, VR }
    public Mode currentMode = Mode.ThirdPerson;

    void Start()
    {
        SwitchMode(currentMode);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchMode(Mode.ThirdPerson);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && WaypointStorage.IsValidWaypoint(WaypointStorage.waypointStart) &&
            WaypointStorage.IsValidWaypoint(WaypointStorage.waypointEnd))
        {
            Debug.Log("Switching to First Person Mode");
            Debug.Log("Waypoint Start: " + WaypointStorage.waypointStart);
            Debug.Log("Waypoint End: " + WaypointStorage.waypointEnd);
            SwitchMode(Mode.FirstPerson);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void SwitchMode(Mode mode)
    {
        currentMode = mode;

        thirdPersonCamera.SetActive(mode == Mode.ThirdPerson);
        firstPersonPlayer.SetActive(mode == Mode.FirstPerson);

        if (mode == Mode.FirstPerson && WaypointStorage.IsValidWaypoint(WaypointStorage.waypointStart) &&
            WaypointStorage.IsValidWaypoint(WaypointStorage.waypointEnd))
        {
            Vector3 startPos = WaypointStorage.waypointStart;
            Vector3 endPos = WaypointStorage.waypointEnd;

            // Posiciona al jugador en el inicio
            firstPersonPlayer.transform.position = startPos + Vector3.up * 3f;

            // Opcional: haz que mire hacia el final
            Vector3 direction = (endPos - startPos).normalized;
            direction.y = 0; // Ignora inclinación si lo deseas
            if (direction != Vector3.zero)
            {
                firstPersonPlayer.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}
