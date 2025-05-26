using UnityEngine;

[RequireComponent(typeof(DetectableGameObject))]
public class NameCharacter : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private GameObject nameText;
    [SerializeField] private bool alwaysVisible = false;

    private CameraZoom cameraZoom;
    private float zoomValue;
    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isZooming => cameraZoom.IsZooming(zoomValue);

    void Start()
    {
        cameraZoom = Camera.main.GetComponent<CameraZoom>();

        CameraZoomSettings settings = GlobalCameraSettings.Instance.GetSettings<CameraZoomSettings>(ObjectType.Character);
        zoomValue = settings.zoomValue;
    }

    void Update()
    {
        if ((isLookingAt && isZooming) || alwaysVisible)
        {
            nameText.SetActive(true);
        }
        else
        {
            nameText.SetActive(false);
        }
    }

    public void DisableAlwaysVisible()
    {
        alwaysVisible = false;
    }
}
