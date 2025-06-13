using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

public class CameraModeManager : MonoBehaviour
{
    public GameObject thirdPersonCamera; // Càmera en tercenra persona
    public GameObject firstPersonPlayer; // Càmera en primera persona
    public GameObject vrPlayer; // Càmera VR

    public enum Mode { ThirdPerson, FirstPerson, VR }
    public Mode currentMode = Mode.ThirdPerson;
    private Mode previousMode = Mode.ThirdPerson;

    public GameObject miniMappanel; // Panell del minimapa
    public bool vrEnabled = false;
    private Vector3 previousPos = Vector3.zero;

    void Start()
    {
        SwitchMode(currentMode, Vector3.zero);
    }


    public void SwitchMode(Mode mode, Vector3 pos)
    {
        previousMode = currentMode;
        previousPos = pos;
        currentMode = mode;

        thirdPersonCamera.SetActive(mode == Mode.ThirdPerson);

        firstPersonPlayer.SetActive(mode == Mode.FirstPerson);

        vrPlayer.SetActive(mode == Mode.VR);

        if (mode == Mode.FirstPerson)
        {
            // Desactiva VR si està actiu
            if (vrEnabled)
            {
                StartCoroutine(DisableVR());
            }

            // Activa el minimapa
            miniMappanel.SetActive(true);
            Vector3 endPos = WaypointStorage.waypointEnd;

            // Obté el component CharacterController y el desactiva per evitar problemes
            Debug.Log("Positioning player: " + pos);
            var controller = firstPersonPlayer.GetComponent<CharacterController>();
            controller.enabled = false;

            // Mou el jugador a la posició inicial amb un petit offset per evitar col·lisions
            firstPersonPlayer.transform.position = pos + Vector3.up * 1.6f;

            // Moure càmera per mirar en la direcció del final
            Vector3 direction = (endPos - pos).normalized;
            direction.y = 0; // Ignora inclinación si lo deseas
            if (direction != Vector3.zero)
            {
                firstPersonPlayer.transform.rotation = Quaternion.LookRotation(direction);
            }

            // Obté el controlador de la càmera en primera persona i reinicia la velocitat
            var fpController = firstPersonPlayer.GetComponent<FirstPersonController>();
            fpController.velocity = Vector3.zero;

            // Activa el controlador de personatge per permetre el moviment
            firstPersonPlayer.GetComponent<CharacterController>().enabled = true;
        }

        if (mode == Mode.VR)
        {
            // Activa el sistema VR mitjançant una corutina
            StartCoroutine(EnableVR());

            // Activa el minimapa
            miniMappanel.SetActive(true);
            Vector3 endPos = WaypointStorage.waypointEnd;


            // Obté el component CharacterController i el desactiva per evitar problemes
            Debug.Log("Positioning VR player: " + pos);
            var vrController = vrPlayer.GetComponent<CharacterController>();
            vrController.enabled = false;

            // Mou el jugador VR a la posició inicial amb un petit offset per evitar col·lisions
            vrPlayer.transform.position = pos + Vector3.up * 1.6f;
            XROrigin origin = vrPlayer.GetComponent<XROrigin>();
            if (origin != null)
            {
                Transform offsetTransform = origin.CameraFloorOffsetObject.transform;
                Vector3 offsetPos = offsetTransform.localPosition;
                offsetPos.y = 1.85f;
                offsetTransform.localPosition = offsetPos;
            }

            // Moure càmera VR per mirar en la direcció del final
            Vector3 direction = (endPos - pos).normalized;
            direction.y = 0; // Ignora la inclinació
            if (direction != Vector3.zero)
            {
                vrPlayer.transform.rotation = Quaternion.LookRotation(direction);
            }

            // Activa el controlador de personatge per permetre el moviment
            vrPlayer.GetComponent<CharacterController>().enabled = true;
        }

        if (mode == Mode.ThirdPerson)
        {
            // Desactiva VR si està actiu
            if (vrEnabled)
            {
                StartCoroutine(DisableVR());
            }
            // Desactiva el minimapa
            miniMappanel.SetActive(false);
        }

    }

    // Mètode per activar el sistema VR
    IEnumerator EnableVR()
    {
        // Comprova si ja està actiu
        if (vrEnabled)
            yield break;

        Debug.Log("Starting XR.");

        // Inicia el XR Loader
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        // Comprova si el XR Loader s'ha inicialitzat correctament
        int attempts = 0;
        while (XRGeneralSettings.Instance.Manager.activeLoader == null && attempts < 200)
        {
            attempts++;
            Debug.Log("Waiting for XR to load");
            yield return null;
        }

        // Si no s'ha inicialitzat, cancel·la el mode VR
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogWarning("Can't start VR mode. Changing to previous camera");
            vrEnabled = false;
            SwitchMode(previousMode, previousPos);
            yield break;
        }

        // Si s'ha inicialitzat, inicia els subsistemes XR
        Debug.Log("Start XR subsystems");
        XRGeneralSettings.Instance.Manager.StartSubsystems();

        // Activa el jugador VR
        vrPlayer.SetActive(true);
        yield return null;

        // Ajusta la posició de la càmera VR
        XROrigin origin = vrPlayer.GetComponent<XROrigin>();
        if (origin != null && origin.CameraFloorOffsetObject != null)
        {
            Vector3 offsetPos = origin.CameraFloorOffsetObject.transform.localPosition;
            offsetPos.y = 1.85f;
            origin.CameraFloorOffsetObject.transform.localPosition = offsetPos;
        }
        else
        {
            Debug.LogWarning("Could not set camera offset VR");
        }

        // Marca que VR està habilitat
        vrEnabled = true;

        Debug.Log("VR succesfully started");
    }

    // Mètode per desactivar el sistema VR
    IEnumerator DisableVR()
    {
        Debug.Log("Desactivando VR...");
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        vrEnabled = false;
        yield return null;
    }

    public bool isVREnabled()
    {
        return vrEnabled;
    }
}