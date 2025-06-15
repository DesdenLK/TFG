using UnityEngine;

public class MinimapIcons : MonoBehaviour
{
    public Camera minimapCamera;         
    private Transform player;

    public Transform vrPlayer;
    public Transform firstPersonPlayer;
    public RectTransform playerIcon;     
    public RectTransform startIcon;      
    public RectTransform endIcon;        
    private Vector3 startPoint;         
    private Vector3 endPoint;           

    public RectTransform minimapRect; 
    public CameraModeManager cameraModeManager;


    void Start()
    {
        // Obtenir el jugador en funci� del mode de c�mera actual
        if (cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson)
        {
            player = firstPersonPlayer;
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.VR)
        {
            player = vrPlayer;
        }
        else
        {
            player = firstPersonPlayer;
        }
        startPoint = WaypointStorage.waypointStart;
        endPoint = WaypointStorage.waypointEnd;
    }

    void Update()
    {
        if (cameraModeManager.currentMode == CameraModeManager.Mode.FirstPerson)
        {
            player = firstPersonPlayer;
        }
        else if (cameraModeManager.currentMode == CameraModeManager.Mode.VR)
        {
            player = vrPlayer;
        }
        else
        {
            player = firstPersonPlayer;
        }
    }

    void LateUpdate()
    {
        if (!minimapCamera || !playerIcon || !minimapRect)
            return;

        UpdateIconPosition(player.position, playerIcon);
        if (startPoint != null && startIcon != null)
            UpdateIconPosition(startPoint, startIcon);
        if (endPoint != null && endIcon != null)
            UpdateIconPosition(endPoint, endIcon);
    }

    void UpdateIconPosition(Vector3 worldTarget, RectTransform icon)
    {
        // Convertir la posici� del m�n a la posici� de la c�mera del minimapa
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldTarget);

        // Limitar la posici� a l'interval [0, 1] per evitar que les icones se surtin del minimapa
        viewportPos.x = Mathf.Clamp01(viewportPos.x);
        viewportPos.y = Mathf.Clamp01(viewportPos.y);

        // Convertir la posici� de la vista a la posici� local del minimapa
        Vector2 minimapSize = minimapRect.rect.size;
        Vector2 localPos = new Vector2(
            (viewportPos.x - 0.5f) * minimapSize.x,
            (viewportPos.y - 0.5f) * minimapSize.y
        );

        icon.anchoredPosition = localPos;

        // En cas que l'icona sigui la del jugador, ajustar la rotaci� per a que apunti en la direcci� correcta
        if (icon == playerIcon)
        {
            float yaw = player.eulerAngles.y;
            icon.localRotation = Quaternion.Euler(0, 0, -yaw);
        }
    }
}
