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
        if (EventSystem.current != null &&
        EventSystem.current.currentSelectedGameObject != null &&
        (EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null ||
        EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null))
        {
            return;
        }
        if (!controller.enabled)
        {
            velocity = Vector3.zero;
        }

        // Comprobar si el jugador está en el suelo
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Movimiento del jugador
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        // Aplicar gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Solo rotar si botón derecho
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
            cam.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
