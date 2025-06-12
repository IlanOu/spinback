using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum ClueIconType { Command, Validate }
[Serializable]
public class ClueIcon
{
    public ClueIconType type;
    public Texture2D texture;
    public Renderer renderer;
}

public class ClueInteractiveIcon : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private InteractableClue interactableClue;
    [SerializeField] private GameObject interactive;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private string commandText = "Ajouter cet élément à mon témoignage";
    [SerializeField] private string validateText = "Élément ajouté";
    [SerializeField] private bool enableVisibility = true;
    [SerializeField] private bool alwaysVisible = false;
    [SerializeField] private float visibleDuration = 3f;
    [SerializeField] private ClueIcon[] clueIcons;
    private bool forceVisibility = false;
    private bool canChangeState = true;
    private bool isEnable = false;
    private bool isAdded = false;

    private CameraZoom cameraZoom;
    private float zoomValue = 0.5f;
    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isZooming => cameraZoom != null && cameraZoom.IsZooming(zoomValue);
    private Clue clue => interactableClue.clue;

    void Start()
    {
        cameraZoom = Camera.main.GetComponent<CameraZoom>();

        CameraZoomSettings settings = GlobalCameraSettings.Instance.GetSettings<CameraZoomSettings>(ObjectType.Object);
        zoomValue = settings.zoomValue;

        foreach (var clue in clueIcons)
        {
            if (clue.renderer != null && clue.texture != null)
            {
                Material matInstance = new Material(clue.renderer.sharedMaterial);
                matInstance.mainTexture = clue.texture;
                clue.renderer.material = matInstance;
            }
        }

        interactableClue.OnClueAdded += ClueAdded;
    }

    void Update()
    {
        if (canChangeState)
        {
            bool shouldBeVisible = (isLookingAt && isZooming) || alwaysVisible || forceVisibility;
            bool hasClue = clue != null;
            bool isNotAdded = !clue.isAdded;

            if (enableVisibility && shouldBeVisible && hasClue && isNotAdded)
            {
                EnableInteractive();
            }
            else
            {
                DisableInteractive();
            }
        }
    }

    public void DisableInteractive()
    {
        if (interactive != null && isEnable)
        {
            isEnable = false;
            interactive.SetActive(false);
        }
    }

    public void EnableInteractive()
    {
        if (interactive != null && !isEnable)
        {
            isEnable = true;
            interactive.SetActive(true);
            EnableCommand();
        }
    }

    public void EnableCommand()
    {
        if (isAdded) return;
        text.text = commandText;
        SetIconVisibility(ClueIconType.Command);
    }

    public void EnableValidate()
    {
        text.text = validateText;
        SetIconVisibility(ClueIconType.Validate);
    }

    void ClueAdded()
    {
        isAdded = true;
        EnableValidate();
        StartCoroutine(WaitBeforeDisableVisibility());
    }

    private void SetIconVisibility(ClueIconType visibleType)
    {
        foreach (var icon in clueIcons)
        {
            if (icon.renderer != null)
            {
                icon.renderer.gameObject.SetActive(icon.type == visibleType);
            }
        }
    }

    private IEnumerator WaitBeforeDisableVisibility()
    {
        canChangeState = false;
        yield return new WaitForSeconds(visibleDuration);
        canChangeState = true;
        enableVisibility = false;
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
