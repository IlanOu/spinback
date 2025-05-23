using UnityEngine;

[RequireComponent(typeof(DetectableGameObject), typeof(Outline))]
public class OutlineCharacter : MonoBehaviour
{
    [SerializeField] private Outline outline;
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField, Range(0f, 1f)] private float zoomValue = 0.5f;
    [SerializeField] private float outlineWidth = 1f;
    [SerializeField] private bool alwaysVisible = false;

    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isZooming => cameraZoom.IsZooming(zoomValue);

    void Update()
    {
        if ((isLookingAt && isZooming) || alwaysVisible)
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
}
