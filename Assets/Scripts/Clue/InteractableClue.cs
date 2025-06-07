using System;
using JetBrains.Annotations;
using UI.Report;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableClue : MonoBehaviour
{
    public Action OnClueAdded;
    [SerializeField] private Clue clue;
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private bool isInteractable = false;
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

        MidiBinding.Instance.Subscribe(MidiBind.BROWSER_BUTTON, OnControllerButtonPressed);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddClue();
        }
    }

    void OnControllerButtonPressed(float value)
    {
        if (value == 1)
        {
            AddClue();
        }
    }

    void AddClue()
    {
        if (isFocused && isInteractable)
        {
            if (ClueDatabase.Instance.AddClue(clue))
            {
                // PopupUI.Instance.Show(clue.popup);
                ReportIcon.Instance.ShowAlertIcon();
                OnClueAdded?.Invoke();
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
