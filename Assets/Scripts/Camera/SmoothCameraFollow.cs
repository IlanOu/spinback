using DefaultNamespace;
using Minis;
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Sensibilité")]
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float smoothSpeed = 5.0f;
    
    [Header("Limites")]
    [SerializeField] private float minX = -90.0f;
    [SerializeField] private float maxX = 90.0f;
    [SerializeField] private float minY = -90.0f;
    [SerializeField] private float maxY = 90.0f;
    
    [Header("Zoom")]
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 60f;
    [SerializeField] private float zoomSmoothSpeed = 5f;
    
    private Vector2 currentRotation;
    private Vector2 targetRotation;
    private float currentZoom;
    private float targetZoom;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        currentRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
        targetRotation = currentRotation;
        
        // Initialiser le zoom
        if (mainCamera != null) currentZoom = mainCamera.fieldOfView;
        targetZoom = currentZoom;
        
        MidiBindingRegistry.Instance.Bind(ActionEnum.Zoom, OnMidiZoom);
    }
    
    private void OnMidiZoom(MidiInput input)
    {
        Debug.Log("MidiZoom: " + input.Value);
        
        float normalizedValue = 1 - (input.Value / 127f);
        targetZoom = Mathf.Lerp(minZoom, maxZoom, normalizedValue);
    }
    
    void Update()
    {
        // Rotation de la caméra
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        targetRotation.x += mouseX;
        targetRotation.y -= mouseY;
        
        targetRotation.x = Mathf.Clamp(targetRotation.x, minX, maxX);
        targetRotation.y = Mathf.Clamp(targetRotation.y, minY, maxY);
        
        currentRotation = Vector2.Lerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
        
        transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        
        // Appliquer le zoom de façon lisse
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSmoothSpeed * Time.deltaTime);
        mainCamera.fieldOfView = currentZoom;

        UpdateZoomUsingKeyboard();
        DetectLookTarget();
    }

    void UpdateZoomUsingKeyboard()
    {
        targetZoom += Input.GetAxis("Vertical") * -mouseSensitivity;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }

    private void DetectLookTarget()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // centre de l'écran
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject target = hit.collider.gameObject;
            Debug.Log("Regarde : " + target.name + " (" + target.tag + ")");

            // Exemple : si tu veux agir selon le tag
            if (target.CompareTag("Conversation"))
            {
                ConversationSoundVolumeController controller = target.GetComponent<ConversationSoundVolumeController>();
                if (controller != null)
                {
                    controller.OnZoom(targetZoom, minZoom, maxZoom);
                }
            }
        }
    }
}