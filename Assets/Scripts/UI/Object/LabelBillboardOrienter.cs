using UnityEngine;

public class LabelBillboardOrienter : MonoBehaviour
{
    [SerializeField] private bool lockRotationX = false;
    [SerializeField] private bool lockRotationY = false;
    [SerializeField] private bool lockRotationZ = false;
    private Camera targetCamera;

    void OnEnable()
    {
        targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward, targetCamera.transform.rotation * Vector3.up);
        transform.rotation = Quaternion.Euler(
            lockRotationX ? transform.rotation.eulerAngles.x : transform.rotation.eulerAngles.x,
            lockRotationY ? transform.rotation.eulerAngles.y : transform.rotation.eulerAngles.y,
            lockRotationZ ? transform.rotation.eulerAngles.z : transform.rotation.eulerAngles.z
        );
    }
}