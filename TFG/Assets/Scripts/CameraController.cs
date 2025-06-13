using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public float rotationSpeed = 3f; // Velocitat de rotaci� de la c�mera
    public float movementSpeed = 300f; //Velocitat de moviment de la c�mera
    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;
    private Vector3 moveDirection;

    private void Start()
    {
        horizontalRotation = transform.eulerAngles.y;
        verticalRotation = transform.eulerAngles.x;
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


        // Si tenim el el bot� dret del ratol� premut, permetem la rotaci� de la c�mera
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            horizontalRotation += Input.GetAxis("Mouse X") * rotationSpeed;
            verticalRotation -= Input.GetAxis("Mouse Y") * rotationSpeed;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Limitar la rotaci� vertical per evitar voltes completes

            transform.eulerAngles = new Vector3(verticalRotation, horizontalRotation, 0);
        }
        else
        {
            Cursor.visible = true;
        }

        // Moviment de la c�mera amb les tecles WASD i Q/E per pujar/baixar
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");
        float upDownMovement = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            upDownMovement = -1f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            upDownMovement = 1f;
        }

        // La posici� �s igual a la posici� actual m�s la direcci� de moviment multiplicada per la velocitat i el temps transcorregut
        moveDirection = transform.right * horizontalMovement + transform.forward * verticalMovement + transform.up * upDownMovement;
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
    }
}
