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
    private float targetZoom;
    private Camera mainCamera;
    private float speed;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    void OnEnable()
    {
        speed = zoomSmoothSpeed;
        
        currentZoom = mainCamera.fieldOfView;
        targetZoom = maxZoom;

        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_1, OnMidiZoom);
        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_2, OnMidiZoom);
    }

    void Update()
    {
        HandleZoom();
        HandleSpeed();
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
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, speed * Time.deltaTime);
        mainCamera.fieldOfView = currentZoom;
    }

    void HandleSpeed()
    {
        if (currentZoom == maxZoom || currentZoom == minZoom)
        {
            speed = zoomSmoothSpeed;
        }
    }

    void OnMidiZoom(float value)
    {
        targetZoom = Mathf.Lerp(minZoom, maxZoom, 1 - value);
    }

    public bool IsZooming(float value)
    {
        float expectedZoom = Mathf.Lerp(minZoom, maxZoom, 1 - value);
        return expectedZoom > currentZoom;
    }

    public void SetTargetZoom(float zoom)
    {
        targetZoom = zoom;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}