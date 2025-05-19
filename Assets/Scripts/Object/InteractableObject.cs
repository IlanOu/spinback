using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] public float outlineSize = 15f;
    [SerializeField] public GameObject label3D;
    [SerializeField] private CameraZoom cameraZoom;
    [HideInInspector] public IInteractableState currentState;
    [HideInInspector] public Material material;
    public bool IsLookingAt => detectableGameObject.isLookingAt;
    public bool alwaysShowLabel = false;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        currentState = new OutlineInteractableState(this);
    }

    void Update()
    {
        currentState.Handle();
    }

    public bool IsZooming()
    {
        if (cameraZoom == null) return false;
        return cameraZoom.isZooming;
    }

    public void UpdateState(IInteractableState newState)
    {
        currentState = newState;
    }
}
