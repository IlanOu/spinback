using UnityEngine;

public class InteractableClue : MonoBehaviour
{
    [SerializeField] private Clue clue;
    [SerializeField] private DetectableGameObject detectableGameObject;
    private bool isInteractable = false;
    private CameraZoom cameraZoom;
    private float zoomValue;
    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isZooming => cameraZoom.IsZooming(zoomValue);
    private bool isFocused => isLookingAt && isZooming;

    void Start()
    {
        cameraZoom = Camera.main.GetComponent<CameraZoom>();

        CameraZoomSettings settings = GlobalCameraSettings.Instance.GetSettings<CameraZoomSettings>(detectableGameObject.objectType);
        zoomValue = settings.zoomValue;

        MidiBinding.Instance.Subscribe(MidiBind.BROWSER_BUTTON, (input) => OnControllerButtonPressed());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            OnControllerButtonPressed();
        }
    }

    void OnControllerButtonPressed()
    {
        if (isFocused && isInteractable)
        {
            if (ClueDatabase.Instance.AddClue(clue))
            {
                PopupUI.Instance.Show(clue.popup);
            }
        }
    }

    public void ToggleInteractability()
    {
        isInteractable = !isInteractable;
    }

    public void EnableInteractability()
    {
        isInteractable = true;
    }

    public void DisableInteractability()
    {
        isInteractable = false;
    }
}
