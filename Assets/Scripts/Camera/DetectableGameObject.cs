using UnityEngine;

public enum ObjectType
{
    Conversation = 0, 
    Labeled = 1,
    InvestigationAttachment = 2,
    Character = 3,
    Other = 4
};

[RequireComponent(typeof(Collider))]
public class DetectableGameObject : MonoBehaviour
{
    
    [SerializeField] protected CameraDetectTarget cameraDetectTarget;

    [Header("Zone de détection (viewport)")]
    [SerializeField, Tooltip("Type de zone de détection pour avec des paramètres prédéfinis")]
    private ObjectType objectType = ObjectType.Other;
    [SerializeField] public Vector2 detectionCenter = new Vector2(0.5f, 0.5f); // Centre dans le viewport
    [SerializeField] public Vector2 detectionSize = new Vector2(0.1f, 0.1f);   // Demi-largeur et demi-hauteur (ellipse)
    [HideInInspector] public bool isLookingAt = false;
    [HideInInspector] public bool isObstructed = false;

    void Awake()
    {
        switch (objectType)
        {
            case ObjectType.Conversation:
                detectionCenter = new Vector2(0.5f, 0.5f);
                detectionSize = new Vector2(0.2f, 0.3f);
                break;
            case ObjectType.Labeled:
                detectionCenter = new Vector2(0.5f, 0.5f);
                detectionSize = new Vector2(0.1f, 0.1f);
                break;
            case ObjectType.Character:
                detectionCenter = new Vector2(0.5f, 0.7f);
                detectionSize = new Vector2(0.1f, 0.25f);
                break;
        }
    }

    void Start()
    {
        if (cameraDetectTarget == null)
        {
            Debug.LogError("CameraDetectTarget is not assigned.");
            return;
        }

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