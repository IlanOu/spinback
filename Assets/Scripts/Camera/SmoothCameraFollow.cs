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
    [SerializeField] private KeyCode toggleMouseControlKey = KeyCode.Tab;

    private Vector2 currentRotation;
    private Vector2 targetRotation;
    private Camera mainCamera;
    private CursorCommandToken cursorToken;
    private bool manuallyControllingCursor = false;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;

        currentRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
        targetRotation = currentRotation;

        // Initialiser le contrôle de la souris
        if (useMouseControl && hideCursorInMouseMode)
        {
            HideCursor();
        }
    }

    void OnEnable()
    {
        // Réappliquer l'état du curseur si nécessaire
        if (useMouseControl && hideCursorInMouseMode && manuallyControllingCursor)
        {
            HideCursor();
        }
    }

    void OnDisable()
    {
        // Libérer le contrôle du curseur
        ReleaseCursorControl();
    }

    void OnDestroy()
    {
        // Libérer le contrôle du curseur
        ReleaseCursorControl();
    }

    void Update()
    {
        // Toggle du contrôle par souris
        if (Input.GetKeyDown(toggleMouseControlKey))
        {
            ToggleMouseControl();
        }

        // Utiliser la souris pour la rotation si le mode souris est actif
        if (useMouseControl)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            targetRotation.x += mouseX;
            targetRotation.y -= mouseY;
        }

        // Utiliser le clavier pour la rotation dans tous les cas
        if (Input.GetKey(KeyCode.A))
            targetRotation.x -= keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            targetRotation.x += keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.W))
            targetRotation.y -= keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            targetRotation.y += keyboardRotationSpeed * Time.deltaTime;

        // Limiter la rotation
        targetRotation.x = Mathf.Clamp(targetRotation.x, minX, maxX);
        targetRotation.y = Mathf.Clamp(targetRotation.y, minY, maxY);

        // Appliquer la rotation avec lissage
        currentRotation = Vector2.Lerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

        // Désactiver le contrôle par souris avec Escape
        if (useMouseControl && Input.GetKeyDown(KeyCode.Escape))
        {
            DisableMouseControl();
        }
    }

    public void ToggleMouseControl()
    {
        if (useMouseControl)
        {
            DisableMouseControl();
        }
        else
        {
            EnableMouseControl();
        }
    }

    public void EnableMouseControl()
    {
        useMouseControl = true;
        
        if (hideCursorInMouseMode)
        {
            HideCursor();
        }
    }

    public void DisableMouseControl()
    {
        useMouseControl = false;
        
        if (hideCursorInMouseMode)
        {
            ShowCursor();
        }
    }

    private void HideCursor()
    {
        // Libérer toute commande existante
        ReleaseCursorControl();
        
        // Créer une nouvelle commande pour cacher le curseur
        cursorToken = CursorManager.Instance.AddCommand("CameraControl", false, 10);
        manuallyControllingCursor = true;
    }

    private void ShowCursor()
    {
        // Libérer la commande de curseur
        ReleaseCursorControl();
    }

    private void ReleaseCursorControl()
    {
        if (cursorToken != null)
        {
            cursorToken.Dispose();
            cursorToken = null;
            manuallyControllingCursor = false;
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
