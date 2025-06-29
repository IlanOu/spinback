using UnityEngine;

[RequireComponent(typeof(DetectableGameObject), typeof(Outline))]
public class OutlineObject : MonoBehaviour
{
    [SerializeField] private Outline outline;
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private float outlineWidth = 1f;
    [SerializeField] private bool enableVisibility = true;
    [SerializeField] private bool alwaysVisible = false;
    [SerializeField] private InteractableClue clue;
    private bool forceVisibility = false;

    private CameraZoom cameraZoom;
    private float zoomValue = 0.5f;
    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isZooming => cameraZoom != null && cameraZoom.IsZooming(zoomValue);

    void Start()
    {
        cameraZoom = Camera.main.GetComponent<CameraZoom>();

        CameraZoomSettings settings = GlobalCameraSettings.Instance.GetSettings<CameraZoomSettings>(ObjectType.Object);
        zoomValue = settings.zoomValue;

        if (clue != null)
        {
            clue.OnClueAdded += () => EnableVisibility(false);
        }
    }

    void Update()
    {
        if ((((isLookingAt && isZooming) || alwaysVisible) && enableVisibility) || forceVisibility)
        {
            outline.OutlineWidth = outlineWidth;
        }
        else
        {
            outline.OutlineWidth = 0f;
        }
    }

    public void DisableAlwaysVisible()
    {
        alwaysVisible = false;
    }

    public void ForceVisibility(bool visible)
    {
        forceVisibility = visible;
    }

    public void EnableVisibility(bool visible)
    {
        enableVisibility = visible;
    }
}
