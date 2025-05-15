using UnityEngine;

public class InteractableObject : MonoBehaviour, IDetectableGameObject
{
    [SerializeField] private CameraDetectTarget cameraDetectTarget;
    [SerializeField] public float outlineSize = 15f;
    [SerializeField] public GameObject label3D;
    private CameraZoom cameraZoom;
    [HideInInspector] public bool isLookingAt = false;
    [HideInInspector] public IInteractableState currentState;
    [HideInInspector] public Material material;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        if (cameraDetectTarget == null)
        {
            Debug.LogError("CameraDetectTarget is not assigned.");
            return;
        }
        cameraZoom = cameraDetectTarget.gameObject.GetComponent<CameraZoom>();
        cameraDetectTarget.Subscribe(this);
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

    public void OnEnter()
    {
        isLookingAt = true;
    }

    public void OnExit()
    {
        isLookingAt = false;
    }
}
