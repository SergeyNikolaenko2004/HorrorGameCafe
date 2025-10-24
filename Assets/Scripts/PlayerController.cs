using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float mouseSensitivity = 2.0f;

    private CharacterController characterController;
    private Camera playerCamera;
    private float cameraPitch = 0f;

    [Header("Interaction")]
    public float interactionDistance = 2f;
    public LayerMask interactionLayerMask;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Захватываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleInteraction();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * walkSpeed * Time.deltaTime);
    }

    void HandleInteraction()
    {
        // Рисуем луч для дебага (видно в Scene View)
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance, Color.red);

        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionDistance, interactionLayerMask))
            {
                InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    interactable.Interact();
                    Debug.Log("Взаимодействие с: " + interactable.itemName);
                }
            }
        }
    }
}
