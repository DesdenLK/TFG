using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

public class CameraModeManager : MonoBehaviour
{
    public GameObject thirdPersonCamera; // Tu cámara aérea
    public GameObject firstPersonPlayer; // Prefab o escena
    public GameObject vrPlayer;

    public enum Mode { ThirdPerson, FirstPerson, VR }
    public Mode currentMode = Mode.ThirdPerson;

    public GameObject miniMappanel;
    private bool vrEnabled = false;

    void Start()
    {
        SwitchMode(currentMode, Vector3.zero);
    }


    public void SwitchMode(Mode mode, Vector3 pos)
    {
        Mode previousMode = currentMode;
        currentMode = mode;

        thirdPersonCamera.SetActive(mode == Mode.ThirdPerson);

        firstPersonPlayer.SetActive(mode == Mode.FirstPerson);


        if (mode == Mode.FirstPerson)
        {
            if (vrEnabled)
            {
                StartCoroutine(DisableVR());
            }

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

        if (mode == Mode.VR)
        {
            StartCoroutine(EnableVR());

            if (vrEnabled)
            {
                vrPlayer.SetActive(mode == Mode.VR);
                miniMappanel.SetActive(true);
                Vector3 endPos = WaypointStorage.waypointEnd;


                // Posiciona al jugador VR en el inicio  
                Debug.Log("Positioning VR player: " + pos);
                var vrController = vrPlayer.GetComponent<CharacterController>();
                vrController.enabled = false;

                // Mover posición directamente  
                vrPlayer.transform.position = pos + Vector3.up * 1.6f;
                XROrigin origin = vrPlayer.GetComponent<XROrigin>();
                if (origin != null)
                {
                    Transform offsetTransform = origin.CameraFloorOffsetObject.transform;
                    Vector3 offsetPos = offsetTransform.localPosition;
                    offsetPos.y = 1.85f;
                    offsetTransform.localPosition = offsetPos;
                }

                // Opcional: haz que mire hacia el final  
                Vector3 direction = (endPos - pos).normalized;
                direction.y = 0; // Ignora inclinación si lo deseas  
                if (direction != Vector3.zero)
                {
                    vrPlayer.transform.rotation = Quaternion.LookRotation(direction);
                }

                // Activa el controlador de personaje  
                vrPlayer.GetComponent<CharacterController>().enabled = true;
            }
            else
            {
                currentMode = previousMode;
                thirdPersonCamera.SetActive(previousMode == Mode.ThirdPerson);

                firstPersonPlayer.SetActive(previousMode == Mode.FirstPerson);
            }
        }

        if (mode == Mode.ThirdPerson)
        {
            if (vrEnabled)
            {
                StartCoroutine(DisableVR());
            }
            miniMappanel.SetActive(false);
        }

    }

    IEnumerator EnableVR()
    {
        if (vrEnabled)
            yield break;

        Debug.Log("Inicializando XR...");

        // Inicia el proceso
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        int i = 0;
        while (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            if (i > 200) yield break;
            i++;
            Debug.LogWarning("No se pudo inicializar el XR Loader. VR no habilitado.");
            yield return null;
        }

        Debug.Log("Iniciando subsistemas XR...");
        XRGeneralSettings.Instance.Manager.StartSubsystems();
        yield return null;

        
        var subsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        XRDisplaySubsystem displaySubsystem = null;
        foreach (var s in subsystems)
        {
            if (s.running) // Esta vez ya debería estar activo si hay visor
            {
                displaySubsystem = s;
                break;
            }
        }

        if (displaySubsystem == null)
        {
            Debug.LogWarning("No se detectó un visor VR conectado. VR deshabilitado.");
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            yield break;
        }

        Debug.Log("Subsistema XRDisplay activo. VR habilitado.");
        vrPlayer.SetActive(true);

        XROrigin origin = vrPlayer.GetComponent<XROrigin>();
        if (origin != null && origin.CameraFloorOffsetObject != null)
        {
            Vector3 offsetPos = origin.CameraFloorOffsetObject.transform.localPosition;
            offsetPos.y = 1.85f;
            origin.CameraFloorOffsetObject.transform.localPosition = offsetPos;

            Debug.Log("Offset de cámara VR ajustado a: " + offsetPos.y);
        }
        else
        {
            Debug.LogWarning("No se encontró el CameraFloorOffsetObject.");
        }

        vrEnabled = true;
    }



    IEnumerator DisableVR()
    {
        Debug.Log("Desactivando VR...");
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        vrEnabled = false;
        yield return null;
    }
}
