using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Sensibilité")]
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float keyboardRotationSpeed = 60.0f;
    [SerializeField] private float smoothSpeed = 5.0f;

    [Header("Limites")]
    [SerializeField] private float minX = -90.0f;
    [SerializeField] private float maxX = 90.0f;
    [SerializeField] private float minY = -90.0f;
    [SerializeField] private float maxY = 90.0f;

    [Header("Contrôle")]
    [SerializeField] private bool useMouseControl = false;
    [SerializeField] private bool hideCursorInMouseMode = true;
    [SerializeField] private CursorManager cursorManager;

    private Vector2 currentRotation;
    private Vector2 targetRotation;
    private Camera mainCamera;

    private bool cursorWasVisiblePriorToScript;
    private CursorLockMode previousLockModePriorToScript;
    private bool isSubscribedToCursorManager = false;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;

        currentRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
        targetRotation = currentRotation;

        if (cursorManager == null)
            cursorManager = FindObjectOfType<CursorManager>();

        if (cursorManager != null)
        {
            SubscribeToCursorManagerEvents();
            useMouseControl = cursorManager.IsCursorHidden();
        }
        else
        {
            cursorWasVisiblePriorToScript = Cursor.visible;
            previousLockModePriorToScript = Cursor.lockState;
            UpdateInternalCursorState();
        }
    }

    void OnEnable()
    {
        if (cursorManager != null && !isSubscribedToCursorManager)
        {
            SubscribeToCursorManagerEvents();
            useMouseControl = cursorManager.IsCursorHidden();
        }
        else if (cursorManager == null)
        {
            UpdateInternalCursorState();
        }
    }

    void OnDisable()
    {
        if (cursorManager != null && isSubscribedToCursorManager)
        {
            UnsubscribeFromCursorManagerEvents();
        }
        else if (cursorManager == null)
        {
            if (Application.isPlaying)
            {
                Cursor.visible = cursorWasVisiblePriorToScript;
                Cursor.lockState = previousLockModePriorToScript;
            }
        }
    }

    void OnDestroy()
    {
        if (cursorManager != null && isSubscribedToCursorManager)
        {
            UnsubscribeFromCursorManagerEvents();
        }
    }

    private void SubscribeToCursorManagerEvents()
    {
        if (cursorManager == null || isSubscribedToCursorManager) return;
        cursorManager.OnCursorStateChanged += HandleCursorManagerStateChanged;
        isSubscribedToCursorManager = true;
    }

    private void UnsubscribeFromCursorManagerEvents()
    {
        if (cursorManager == null || !isSubscribedToCursorManager) return;
        cursorManager.OnCursorStateChanged -= HandleCursorManagerStateChanged;
        isSubscribedToCursorManager = false;
    }

    private void HandleCursorManagerStateChanged(bool isHidden)
    {
        useMouseControl = isHidden;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetMouseControl(!useMouseControl);
        }

        if (useMouseControl)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            targetRotation.x += mouseX;
            targetRotation.y -= mouseY;
        }

        if (Input.GetKey(KeyCode.A))
            targetRotation.x -= keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            targetRotation.x += keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.W))
            targetRotation.y -= keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            targetRotation.y += keyboardRotationSpeed * Time.deltaTime;

        targetRotation.x = Mathf.Clamp(targetRotation.x, minX, maxX);
        targetRotation.y = Mathf.Clamp(targetRotation.y, minY, maxY);

        currentRotation = Vector2.Lerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

        if (useMouseControl && Input.GetKeyDown(KeyCode.Escape))
        {
            SetMouseControl(false);
        }

        if (cursorManager != null && useMouseControl && !cursorManager.IsCursorHidden())
        {
            useMouseControl = false;
        }
    }

    private void UpdateInternalCursorState()
    {
        if (cursorManager != null) return;

        if (useMouseControl && hideCursorInMouseMode)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SetMouseControl(bool enable)
    {
        useMouseControl = enable;
        if (cursorManager != null)
        {
            cursorManager.SetCursorHidden(useMouseControl);
        }
        else
        {
            UpdateInternalCursorState();
        }
    }

    public bool IsMouseControlActive()
    {
        return useMouseControl;
    }
    
    public void SetTargetRotation(Vector2 rotation)
    {
        targetRotation = rotation;
    }
}
