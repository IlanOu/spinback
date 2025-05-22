using UnityEngine;

[RequireComponent(typeof(DetectableGameObject), typeof(Outline))]
public class OutlineCharacter : MonoBehaviour
{
    [SerializeField] private Outline outline;
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private float outlineWidth = 1.5f;
    [SerializeField] private bool alwaysVisible = false;

    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isZooming => cameraZoom.isZooming;

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
