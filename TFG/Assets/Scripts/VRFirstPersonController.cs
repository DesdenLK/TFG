using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class VRFirstPersonController : MonoBehaviour
{
    public InputActionProperty moveAction;
    public Transform headTransform;
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
        // Mira si hi ha un element UI seleccionat i no permet el moviment del jugador.
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


        // Comprova si el jugador està a terra utilitzant un Raycast.
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Llegeix l'entrada del jugador per al moviment.
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Calcula la direcció del moviment basant-se en la posició del cap del jugador.
        Vector3 forward = headTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        // Calcula la direcció del moviment basant-se en la posició del cap del jugador.
        Vector3 right = headTransform.right;
        right.y = 0f;
        right.Normalize();

        // Mou el jugador en la direcció del moviment.
        Vector3 move = forward * input.y + right * input.x;
        controller.Move(move * speed * Time.deltaTime);

        // Aplica la gravetat al jugador.
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
