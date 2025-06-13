using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CanvasBehaviour : MonoBehaviour
{
    private GameObject canvas;

    void Start()
    {
        canvas = gameObject;
    }

    // Activa o desactiva la visibilitat del canvas
    public void toggleCanvasVisibility()
    {
        canvas.SetActive(!canvas.activeSelf);
    }
}
