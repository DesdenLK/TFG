using System;
using UnityEngine;

public class CameraModeManager : MonoBehaviour
{
    public GameObject thirdPersonCamera; // Tu cámara aérea
    public GameObject firstPersonPlayer; // Prefab o escena

    public enum Mode { ThirdPerson, FirstPerson, VR }
    public Mode currentMode = Mode.ThirdPerson;

    public GameObject miniMappanel;

    void Start()
    {
        SwitchMode(currentMode, Vector3.zero);
    }


    public void SwitchMode(Mode mode, Vector3 pos)
    {
        currentMode = mode;

        thirdPersonCamera.SetActive(mode == Mode.ThirdPerson);

        firstPersonPlayer.SetActive(mode == Mode.FirstPerson);

        if (mode == Mode.FirstPerson)
        {

            miniMappanel.SetActive(true);
            Vector3 endPos = WaypointStorage.waypointEnd;

            // Posiciona al jugador en el inicio
            Debug.Log("Positioning player: " + pos);
            var controller = firstPersonPlayer.GetComponent<CharacterController>();
            controller.enabled = false;

            // Mover posición directamente
            firstPersonPlayer.transform.position = pos + Vector3.up * 1.6f;

            // Opcional: haz que mire hacia el final
            Vector3 direction = (endPos - pos).normalized;
            direction.y = 0; // Ignora inclinación si lo deseas
            if (direction != Vector3.zero)
            {
                firstPersonPlayer.transform.rotation = Quaternion.LookRotation(direction);
            }

            var fpController = firstPersonPlayer.GetComponent<FirstPersonController>();
            fpController.velocity = Vector3.zero;

            // Activa el controlador de personaje
            firstPersonPlayer.GetComponent<CharacterController>().enabled = true;
        }

        if (mode == Mode.ThirdPerson)
        {
            miniMappanel.SetActive(false);
        }

    }
}
