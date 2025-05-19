using UnityEngine;

public class WorldSpaceCanvasController : MonoBehaviour
{
    public Camera firstPersonCam;
    public Camera thirdPersonCam;
    public Camera vrCam;

    public float firstPersonDistance = 1.5f;
    public float thirdPersonDistance = 3f;
    public float vrDistance = 2f;

    private Canvas canvas;

    void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    void LateUpdate()
    {
        Camera currentCam = GetActiveCamera();
        if (currentCam == null) return;

        if (currentCam == vrCam)
        {
            // Modo World Space para VR
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                canvas.worldCamera = vrCam;
            }

            float distance = vrDistance;
            transform.position = vrCam.transform.position + vrCam.transform.forward * distance;
            transform.rotation = Quaternion.LookRotation(transform.position - vrCam.transform.position);
        }
        else
        {
            // Modo Screen Space Overlay para 1a o 3a persona
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.transform.localScale = Vector3.one;
                canvas.worldCamera = null;
            }

            // Opcional: resetear posici�n y rotaci�n si quieres
            // transform.position = Vector3.zero;
            // transform.rotation = Quaternion.identity;
        }
    }

    Camera GetActiveCamera()
    {
        if (firstPersonCam != null && firstPersonCam.gameObject.activeInHierarchy)
            return firstPersonCam;

        if (thirdPersonCam != null && thirdPersonCam.gameObject.activeInHierarchy)
            return thirdPersonCam;

        if (vrCam != null && vrCam.gameObject.activeInHierarchy)
            return vrCam;

        return null;
    }
}
