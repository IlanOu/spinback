using UnityEngine;

public class DetectableGameObject : MonoBehaviour
{
    [SerializeField] protected CameraDetectTarget cameraDetectTarget;
    [SerializeField] public Vector2 detectionZone = new Vector2(0.5f, 0.5f);
    [SerializeField] public float detectionPrecision = 0.1f;
    [HideInInspector] public bool isLookingAt = false;

    protected void Start()
    {
        if (cameraDetectTarget == null)
        {
            Debug.LogError("CameraDetectTarget is not assigned.");
            return;
        }
        cameraDetectTarget.Subscribe(this);
    }

    public void OnEnter()
    {
        isLookingAt = true;
        Debug.Log("Looking at " + gameObject.name);
    }

    public void OnExit()
    {
        isLookingAt = false;
        Debug.Log("Not looking at " + gameObject.name);
    }
}