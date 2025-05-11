using UnityEngine;

public class LabelBillboardOrienter : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        transform.forward = targetCamera.transform.forward;
    }
}