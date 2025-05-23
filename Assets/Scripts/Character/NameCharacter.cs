using UnityEngine;

[RequireComponent(typeof(DetectableGameObject))]
public class NameCharacter : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private GameObject nameText;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField, Range(0f, 1f)] private float zoomValue = 0.5f;
    [SerializeField] private bool alwaysVisible = false;

    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isZooming => cameraZoom.IsZooming(zoomValue);

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
