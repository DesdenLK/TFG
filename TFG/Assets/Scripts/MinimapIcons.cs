using UnityEngine;

public class MinimapIcons : MonoBehaviour
{
    public Camera minimapCamera;         
    public Transform player;             
    public RectTransform playerIcon;     
    public RectTransform startIcon;      
    public RectTransform endIcon;        
    private Vector3 startPoint;         
    private Vector3 endPoint;           

    public RectTransform minimapRect;    


    void Start()
    {
        startPoint = WaypointStorage.waypointStart;
        endPoint = WaypointStorage.waypointEnd;
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
        // Convertir posición del mundo a viewport (0–1)
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldTarget);

        // Limitar valores para que los íconos no se salgan del minimapa
        viewportPos.x = Mathf.Clamp01(viewportPos.x);
        viewportPos.y = Mathf.Clamp01(viewportPos.y);

        // Convertir a posición dentro del minimapRect
        Vector2 minimapSize = minimapRect.rect.size;
        Vector2 localPos = new Vector2(
            (viewportPos.x - 0.5f) * minimapSize.x,
            (viewportPos.y - 0.5f) * minimapSize.y
        );

        icon.anchoredPosition = localPos;

        // Rotar el ícono del jugador para que apunte en su dirección
        if (icon == playerIcon)
        {
            float yaw = player.eulerAngles.y;
            icon.localRotation = Quaternion.Euler(0, 0, -yaw);
        }
    }
}
