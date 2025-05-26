using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DetectableGameObject : MonoBehaviour
{
    [SerializeField] public ObjectType objectType = ObjectType.Other;

    [Header("Zone de détection (viewport)")]
    [Tooltip("Type de zone de détection pour avec des paramètres prédéfinis")]
    [HideInInspector] public Vector2 detectionCenter = new Vector2(0.5f, 0.5f); // Centre dans le viewport
    [HideInInspector] public Vector2 detectionSize = new Vector2(0.1f, 0.1f);   // Demi-largeur et demi-hauteur (ellipse)
    [HideInInspector] public bool isLookingAt = false;
    [HideInInspector] public bool isObstructed = false;
    private CameraDetectTarget cameraDetectTarget;

    void Awake()
    {
        DetectionZoneSettings settings = GlobalCameraSettings.Instance.GetSettings<DetectionZoneSettings>(objectType);
        detectionCenter = settings.detectionCenter;
        detectionSize = settings.detectionSize;
    }

    void Start()
    {
        cameraDetectTarget = Camera.main.GetComponent<CameraDetectTarget>();

        cameraDetectTarget.Subscribe(this);
    }

    public void OnEnter()
    {
        if (!isLookingAt)
        {
            isLookingAt = true;
        }
    }

    public void OnExit()
    {
        if (isLookingAt)
        {
            isLookingAt = false;
        }

        if (isObstructed)
        {
            isObstructed = false;
        }
    }

    public void OnObstructed(bool obstructed)
    {
        if (obstructed == isObstructed) return;

        isObstructed = obstructed;
    }
}