using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Sensibilité")]
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float keyboardRotationSpeed = 60.0f;
    [SerializeField] private float smoothSpeed = 5.0f;

    [Header("Limites")]
    [SerializeField] private float minX = -90.0f; // Yaw
    [SerializeField] private float maxX = 90.0f;
    [SerializeField] private float minY = -90.0f; // Pitch
    [SerializeField] private float maxY = 90.0f;

    [Header("Contrôle")]
    [SerializeField] private bool useMouseControl = false;
    [SerializeField] private CursorManager cursorManager;

    private Vector2 currentRotation;
    private Vector2 targetRotation;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Trouver le CursorManager s'il n'est pas assigné
        if (cursorManager == null)
            cursorManager = FindObjectOfType<CursorManager>();

        if (cursorManager != null)
        {
            // S'abonner à l'événement de changement d'état du curseur
            cursorManager.OnCursorStateChanged += OnCursorStateChanged;
            
            // Synchroniser l'état initial
            useMouseControl = cursorManager.IsCursorHidden();
        }

        currentRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
        targetRotation = currentRotation;
    }

    void OnDestroy()
    {
        // Se désabonner de l'événement pour éviter les fuites de mémoire
        if (cursorManager != null)
        {
            cursorManager.OnCursorStateChanged -= OnCursorStateChanged;
        }
    }

    // Méthode appelée lorsque l'état du curseur change
    private void OnCursorStateChanged(bool isHidden)
    {
        useMouseControl = isHidden;
    }

    void Update()
    {
        // Vérifier si le mode de contrôle a changé
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            useMouseControl = !useMouseControl;
            if (cursorManager != null)
            {
                cursorManager.SetCursorHidden(useMouseControl);
            }
        }

        // Contrôle souris (optionnel)
        if (useMouseControl)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            targetRotation.x += mouseX;
            targetRotation.y -= mouseY;
        }

        // Contrôle ZQSD (toujours actif) (Utilisation des touches QASD)
        if (Input.GetKey(KeyCode.A))
            targetRotation.x -= keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            targetRotation.x += keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.W))
            targetRotation.y -= keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            targetRotation.y += keyboardRotationSpeed * Time.deltaTime;

        // Clamps
        targetRotation.x = Mathf.Clamp(targetRotation.x, minX, maxX);
        targetRotation.y = Mathf.Clamp(targetRotation.y, minY, maxY);

        // Lissage
        currentRotation = Vector2.Lerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        
        // Vérifier si le curseur est devenu visible (par exemple via Escape)
        if (useMouseControl && cursorManager != null && !cursorManager.IsCursorHidden())
        {
            useMouseControl = false;
        }
    }
    
    // Méthode publique pour activer/désactiver le contrôle par souris
    public void SetMouseControl(bool enable)
    {
        useMouseControl = enable;
        if (cursorManager != null)
        {
            cursorManager.SetCursorHidden(enable);
        }
    }
}
