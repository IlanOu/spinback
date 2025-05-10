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

    private Vector2 currentRotation;
    private Vector2 targetRotation;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;

        currentRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
        targetRotation = currentRotation;
    }

    void Update()
    {
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
    }
}