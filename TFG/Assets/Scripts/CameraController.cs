using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public float rotationSpeed = 3f; // Velocidad de rotación de la cámara
    public float movementSpeed = 300f; // Velocidad de movimiento de la cámara

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
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            (EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null))
        {
            return;
        }


        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            horizontalRotation += Input.GetAxis("Mouse X") * rotationSpeed;
            verticalRotation -= Input.GetAxis("Mouse Y") * rotationSpeed;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            transform.eulerAngles = new Vector3(verticalRotation, horizontalRotation, 0);
        }
        else
        {
            Cursor.visible = true;
        }

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

        moveDirection = transform.right * horizontalMovement + transform.forward * verticalMovement + transform.up * upDownMovement;
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
    }
}
