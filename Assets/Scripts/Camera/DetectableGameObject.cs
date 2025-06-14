using UnityEngine;

public class DetectableGameObject : MonoBehaviour
{
    public bool isLookingAt { get; private set; } = false;

    private CameraDetectTarget cameraDetectTarget;

    void Start()
    {
        cameraDetectTarget = Camera.main.GetComponent<CameraDetectTarget>();
        if (cameraDetectTarget != null)
        {
            cameraDetectTarget.Subscribe(this);
        }
    }

    public void OnEnter()
    {
        if (!isLookingAt)
        {
            isLookingAt = true;
            // Debug.Log($"{gameObject.name} is being looked at.");
        }
    }

    public void OnExit()
    {
        if (isLookingAt)
        {
            isLookingAt = false;
            // Debug.Log($"{gameObject.name} is no longer being looked at.");
        }
    }

    void OnDestroy()
    {
        if (cameraDetectTarget != null)
        {
            cameraDetectTarget.Unsubscribe(this);
        }
    }
}