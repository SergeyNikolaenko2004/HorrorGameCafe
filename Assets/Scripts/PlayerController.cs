using UnityEngine;

// Интерфейсы для различных систем
public interface IMovementController
{
    void Move(float horizontal, float vertical);
    Vector3 Velocity { get; }
}

public interface ICameraController
{
    void Rotate(float mouseX, float mouseY);
    Vector3 CameraForward { get; }
    Vector3 CameraPosition { get; }
}

public interface IInteractionController
{
    void CheckInteraction();
    bool CanInteract { get; set; }
}

public interface IInputService
{
    float GetHorizontalAxis();
    float GetVerticalAxis();
    float GetMouseX();
    float GetMouseY();
    bool GetInteractKeyDown();
}

// Настройки игрока
[System.Serializable]
public class PlayerSettings
{
    [field: SerializeField, Range(1f, 10f)] 
    public float WalkSpeed { get; private set; } = 3.0f;
    
    [field: SerializeField, Range(0.1f, 5f)]
    public float MouseSensitivity { get; private set; } = 2.0f;
    
    [field: SerializeField, Range(1f, 10f)]
    public float InteractionDistance { get; private set; } = 2f;
    
    [field: SerializeField]
    public LayerMask InteractionLayerMask { get; private set; } = -1;
}

// Реализация ввода
public class UnityInputService : IInputService
{
    public float GetHorizontalAxis() => Input.GetAxis("Horizontal");
    public float GetVerticalAxis() => Input.GetAxis("Vertical");
    public float GetMouseX() => Input.GetAxis("Mouse X");
    public float GetMouseY() => Input.GetAxis("Mouse Y");
    public bool GetInteractKeyDown() => Input.GetKeyDown(KeyCode.E);
}

// Контроллер движения
public class CharacterMovementController : IMovementController
{
    private readonly CharacterController _characterController;
    private readonly float _walkSpeed;
    
    public Vector3 Velocity { get; private set; }

    public CharacterMovementController(CharacterController characterController, float walkSpeed)
    {
        _characterController = characterController;
        _walkSpeed = walkSpeed;
    }

    public void Move(float horizontal, float vertical)
    {
        Vector3 move = _characterController.transform.right * horizontal + 
                      _characterController.transform.forward * vertical;
        
        Velocity = move * _walkSpeed;
        _characterController.Move(Velocity * Time.deltaTime);
    }
}

// Контроллер камеры
public class FirstPersonCameraController : ICameraController
{
    private readonly Transform _playerTransform;
    private readonly Transform _cameraTransform;
    private readonly float _mouseSensitivity;
    
    private float _cameraPitch;

    public Vector3 CameraForward => _cameraTransform.forward;
    public Vector3 CameraPosition => _cameraTransform.position;

    public FirstPersonCameraController(Transform playerTransform, Transform cameraTransform, float mouseSensitivity)
    {
        _playerTransform = playerTransform;
        _cameraTransform = cameraTransform;
        _mouseSensitivity = mouseSensitivity;
        _cameraPitch = 0f;
    }

    public void Rotate(float mouseX, float mouseY)
    {
        // Поворот игрока по горизонтали
        _playerTransform.Rotate(Vector3.up * mouseX * _mouseSensitivity);

        // Наклон камеры по вертикали
        _cameraPitch -= mouseY * _mouseSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -90f, 90f);
        _cameraTransform.localEulerAngles = Vector3.right * _cameraPitch;
    }
}

// Контроллер взаимодействий
public class RaycastInteractionController : IInteractionController
{
    private readonly ICameraController _cameraController;
    private readonly IInputService _inputService;
    private readonly float _interactionDistance;
    private readonly LayerMask _interactionLayerMask;
    
    public bool CanInteract { get; set; } = true;

    public RaycastInteractionController(ICameraController cameraController, IInputService inputService, 
                                      float interactionDistance, LayerMask interactionLayerMask)
    {
        _cameraController = cameraController;
        _inputService = inputService;
        _interactionDistance = interactionDistance;
        _interactionLayerMask = interactionLayerMask;
    }

    public void CheckInteraction()
    {
        if (!CanInteract || !_inputService.GetInteractKeyDown()) return;

        DrawDebugRay();

        if (Physics.Raycast(_cameraController.CameraPosition, _cameraController.CameraForward, 
            out RaycastHit hit, _interactionDistance, _interactionLayerMask))
        {
            TryInteractWithObject(hit);
        }
    }

    private void TryInteractWithObject(RaycastHit hit)
    {
        InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
        if (interactable != null)
        {
            interactable.Interact();
            Debug.Log($"Взаимодействие с: {interactable.itemName}");
        }
    }

    private void DrawDebugRay()
    {
        Debug.DrawRay(_cameraController.CameraPosition, _cameraController.CameraForward * _interactionDistance, Color.red);
    }
}

// Сервис управления курсором
public class CursorManager
{
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

// Основной контроллер игрока
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private PlayerSettings settings = new PlayerSettings();

    // Зависимости
    private IMovementController _movementController;
    private ICameraController _cameraController;
    private IInteractionController _interactionController;
    private IInputService _inputService;
    private CursorManager _cursorManager;

    // Компоненты Unity
    private CharacterController _characterController;
    private Camera _playerCamera;

    private void Awake()
    {
        InitializeComponents();
        InitializeDependencies();
        SetupCursor();
    }

    private void InitializeComponents()
    {
        _characterController = GetComponent<CharacterController>();
        _playerCamera = GetComponentInChildren<Camera>();

        if (_characterController == null)
            Debug.LogError("CharacterController не найден на игроке!");
        
        if (_playerCamera == null)
            Debug.LogError("Camera не найден в дочерних объектах игрока!");
    }

    private void InitializeDependencies()
    {
        _inputService = new UnityInputService();
        _cursorManager = new CursorManager();
        
        _movementController = new CharacterMovementController(_characterController, settings.WalkSpeed);
        _cameraController = new FirstPersonCameraController(transform, _playerCamera.transform, settings.MouseSensitivity);
        _interactionController = new RaycastInteractionController(_cameraController, _inputService, 
            settings.InteractionDistance, settings.InteractionLayerMask);
    }

    private void SetupCursor()
    {
        _cursorManager.LockCursor();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        HandleCameraRotation();
        HandleMovement();
        HandleInteraction();
    }

    private void HandleCameraRotation()
    {
        float mouseX = _inputService.GetMouseX();
        float mouseY = _inputService.GetMouseY();
        _cameraController.Rotate(mouseX, mouseY);
    }

    private void HandleMovement()
    {
        float horizontal = _inputService.GetHorizontalAxis();
        float vertical = _inputService.GetVerticalAxis();
        _movementController.Move(horizontal, vertical);
    }

    private void HandleInteraction()
    {
        _interactionController.CheckInteraction();
    }

    private void OnDestroy()
    {
        _cursorManager.UnlockCursor();
    }

    // Публичные методы для внешнего управления
    public void EnableInteraction() => _interactionController.CanInteract = true;
    public void DisableInteraction() => _interactionController.CanInteract = false;
    public void EnableCursor() => _cursorManager.UnlockCursor();
    public void DisableCursor() => _cursorManager.LockCursor();

    // Методы для тестирования и кастомной настройки
    public void SetMovementController(IMovementController controller) => _movementController = controller;
    public void SetCameraController(ICameraController controller) => _cameraController = controller;
    public void SetInteractionController(IInteractionController controller) => _interactionController = controller;
    public void SetInputService(IInputService service) => _inputService = service;
}