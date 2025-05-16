using UnityEngine;

public class DetectableGameObject : MonoBehaviour
{
    private enum ObjectType { Conversation, InteractableObject, Other };
    [SerializeField] protected CameraDetectTarget cameraDetectTarget;

    [Header("Zone de détection (viewport)")]
    [SerializeField, Tooltip("Type de zone de détection pour avec des paramètres prédéfinis")]
    private ObjectType objectType = ObjectType.Other;
    [SerializeField] public Vector2 detectionCenter = new Vector2(0.5f, 0.5f); // Centre dans le viewport
    [SerializeField] public Vector2 detectionSize = new Vector2(0.1f, 0.1f);   // Demi-largeur et demi-hauteur (ellipse)
    [HideInInspector] public bool isLookingAt = false;

    protected void Start()
    {
        if (cameraDetectTarget == null)
        {
            Debug.LogError("CameraDetectTarget is not assigned.");
            return;
        }

        cameraDetectTarget.Subscribe(this);

        switch (objectType)
        {
            case ObjectType.Conversation:
                detectionCenter = new Vector2(0.5f, 0.5f);
                detectionSize = new Vector2(0.2f, 0.3f);
                break;
            case ObjectType.InteractableObject:
                detectionCenter = new Vector2(0.5f, 0.5f);
                detectionSize = new Vector2(0.1f, 0.1f);
                break;
        }
    }

    public void OnEnter()
    {
        if (!isLookingAt)
        {
            isLookingAt = true;
            Debug.Log("<color=green>LOOKING AT</color> " + gameObject.name);
        }
    }

    public void OnExit()
    {
        if (isLookingAt)
        {
            isLookingAt = false;
            Debug.Log("<color=red>Not looking at</color> " + gameObject.name);
        }
    }
}