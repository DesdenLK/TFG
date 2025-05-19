using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class VRFirstPersonController : MonoBehaviour
{
    public InputActionProperty moveAction; // Asigna "XRI LeftHand/Move" en el inspector
    public Transform headTransform;        // Asigna la Main Camera (dentro del XR Origin)
    public float speed = 1.5f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Bloqueo de movimiento si se está escribiendo en un campo de texto
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            (EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null ||
             EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null))
        {
            return;
        }

        // Verifica si está en el suelo
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Leer movimiento del joystick
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Mover en la dirección de la cabeza
        Vector3 forward = headTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = headTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = forward * input.y + right * input.x;
        controller.Move(move * speed * Time.deltaTime);

        // Aplicar gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
