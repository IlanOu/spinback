using UnityEngine;

public class InteractableClue : MonoBehaviour
{
    [SerializeField] private Clue clue;
    [SerializeField] private DetectableGameObject detectableGameObject;
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

        MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_1, (input) => OnControllerButtonPressed());
        MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_2, (input) => OnControllerButtonPressed());
        MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_1, (input) => OnControllerButtonPressed());
        MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_2, (input) => OnControllerButtonPressed());
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
        if (isFocused)
        {
            ClueDatabase.Instance.AddClue(clue);
        }
    }
}
