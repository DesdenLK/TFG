using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

public class CameraModeManager : MonoBehaviour
{
    public GameObject thirdPersonCamera;
    public GameObject firstPersonPlayer;
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

        // Desactiva todos los modos antes de activar el nuevo
        thirdPersonCamera.SetActive(false);
        firstPersonPlayer.SetActive(false);
        vrPlayer.SetActive(false);

        if (mode == Mode.FirstPerson)
        {
            if(vrEnabled) StartCoroutine(DisableVR());

            miniMappanel.SetActive(true);
            Vector3 endPos = WaypointStorage.waypointEnd;

            firstPersonPlayer.SetActive(true);
            var controller = firstPersonPlayer.GetComponent<CharacterController>();
            controller.enabled = false;

            firstPersonPlayer.transform.position = pos + Vector3.up * 1.6f;

            Vector3 direction = (endPos - pos).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                firstPersonPlayer.transform.rotation = Quaternion.LookRotation(direction);
            }

            var fpController = firstPersonPlayer.GetComponent<FirstPersonController>();
            fpController.velocity = Vector3.zero;

            controller.enabled = true;
        }
        else if (mode == Mode.VR)
        {
            StartCoroutine(SwitchToVR(pos, previousMode));
        }
        else if (mode == Mode.ThirdPerson)
        {
            if (vrEnabled) StartCoroutine(DisableVR());

            thirdPersonCamera.SetActive(true);
            miniMappanel.SetActive(false);
        }
    }

    IEnumerator SwitchToVR(Vector3 pos, Mode fallbackMode)
    {
        yield return EnableVR();

        if (!vrEnabled)
        {
            Debug.LogWarning("No se pudo activar VR. Volviendo al modo anterior.");
            currentMode = fallbackMode;
            SwitchMode(fallbackMode, pos);
            yield break;
        }

        miniMappanel.SetActive(true);
        Vector3 endPos = WaypointStorage.waypointEnd;

        vrPlayer.SetActive(true);
        var controller = vrPlayer.GetComponent<CharacterController>();
        controller.enabled = false;

        vrPlayer.transform.position = pos + Vector3.up * 1.6f;

        XROrigin origin = vrPlayer.GetComponent<XROrigin>();
        if (origin != null && origin.CameraFloorOffsetObject != null)
        {
            var offsetTransform = origin.CameraFloorOffsetObject.transform;
            Vector3 offsetPos = offsetTransform.localPosition;
            offsetPos.y = 1.85f;
            offsetTransform.localPosition = offsetPos;

            yield return null; // Espera un frame para aplicar cambios

            Debug.Log("Offset actual de la cámara: " + offsetTransform.localPosition.y);
        }
        else
        {
            Debug.LogWarning("No se encontró el CameraFloorOffsetObject.");
        }

        Vector3 direction = (endPos - pos).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            vrPlayer.transform.rotation = Quaternion.LookRotation(direction);
        }

        controller.enabled = true;
    }

    IEnumerator EnableVR()
    {
        if (vrEnabled)
            yield break;

        Debug.Log("Inicializando XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        int i = 0;
        while (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            if (i++ > 200)
            {
                Debug.LogWarning("Timeout inicializando XR Loader.");
                yield break;
            }
            yield return null;
        }

        XRGeneralSettings.Instance.Manager.StartSubsystems();
        yield return null;

        var subsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        XRDisplaySubsystem displaySubsystem = subsystems.Find(s => s.running);

        if (displaySubsystem == null)
        {
            Debug.LogWarning("No se detectó visor VR. Cancelando VR.");
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            vrEnabled = false;
            yield break;
        }

        vrEnabled = true;
        Debug.Log("VR habilitado correctamente.");
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
