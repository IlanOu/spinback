using UnityEngine;
using static Anchors;

public class LabelBillboardScaler : MonoBehaviour
{
    public Camera targetCamera;
    public CameraZoom cameraZoom;

    [Header("Scale Settings")]
    public float scaleFactor = 1f;
    public float minScale = 0.5f;
    public float maxScale = 2f;

    [Header("Anchor Settings")]
    public AnchorType anchorType = AnchorType.BottomLeft;
    public Vector2 visualSize = new Vector2(1f, 0.5f); // taille initiale du label à 1x scale

    private Vector3 baseLocalPosition;

    void Start()
    {
        baseLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (cameraZoom == null)
            cameraZoom = targetCamera.GetComponent<CameraZoom>();

        if (cameraZoom == null)
        {
            Debug.LogWarning("LabelBillboardScaler: CameraZoom non trouvé sur la caméra.");
            return;
        }

        // Calcul du scale
        float t = Mathf.InverseLerp(cameraZoom.maxZoom, cameraZoom.minZoom, cameraZoom.currentZoom);
        float scale = Mathf.Lerp(maxScale, minScale, t) * scaleFactor;
        transform.localScale = Vector3.one * scale;

        // Appliquer offset selon ancrage pour compenser l'effet du scale
        Anchor anchor = Anchors.GetAnchor(anchorType);
        Vector3 offset = new Vector3(
            -(anchor.max.x - 0.5f) * visualSize.x * (scale - 1),
            -(anchor.max.y - 0.5f) * visualSize.y * (scale - 1),
            0
        );

        transform.localPosition = baseLocalPosition + offset;
    }
}