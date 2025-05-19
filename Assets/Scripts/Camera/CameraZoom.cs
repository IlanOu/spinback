using Minis;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float zoomStep = 50f;
    [SerializeField] public float minZoom = 10f;
    [SerializeField] public float maxZoom = 60f;
    [SerializeField] private float zoomSmoothSpeed = 100f;

    public float currentZoom;
    [HideInInspector] public bool isZooming => Mathf.Abs(maxZoom - currentZoom) > zoomMarginError;
    private float targetZoom;
    private float zoomMarginError = 1f;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    void Start()
    {
        currentZoom = mainCamera.fieldOfView;
        targetZoom = currentZoom;

        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_1, OnMidiZoom);
    }

    void Update()
    {
        HandleZoom();
    }

    void HandleZoom()
    {
        // Flèche haut = zoom avant = FOV diminue
        if (Input.GetKey(KeyCode.UpArrow))
            targetZoom -= zoomStep * Time.deltaTime;

        // Flèche bas = zoom arrière = FOV augmente
        if (Input.GetKey(KeyCode.DownArrow))
            targetZoom += zoomStep * Time.deltaTime;

        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSmoothSpeed * Time.deltaTime);
        mainCamera.fieldOfView = currentZoom;
    }

    void OnMidiZoom(float value)
    {
        targetZoom = Mathf.Lerp(minZoom, maxZoom, 1 - value);
    }
}