using UnityEngine;
using UnityEngine.EventSystems;

public class FirstPersonController : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Transform cam;
    private float verticalLookRotation = 0f;

    public Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        controller.enabled = true;
    }

    void Update()
    {
        // Comprovar si hi ha algun element de la UI seleccionat
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            (
                EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.Dropdown>() != null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_Dropdown>() != null
            ))
        {
            return;
        }

        if (!controller.enabled)
        {
            velocity = Vector3.zero;
        }

        // Comprovar si el jugador està a terra
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Moviment del jugador amb les tecles WASD
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        // Aplicar la gravetat
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Rota la càmera amb el botó dret del ratolí
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalLookRotation -= mouseY; // Invertir la rotació vertical per a una experiència més natural
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f); // Limitar la rotació vertical per evitar voltes completes
            cam.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0); 
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
