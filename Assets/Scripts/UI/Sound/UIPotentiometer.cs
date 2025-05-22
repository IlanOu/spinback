using UnityEngine;

public class UIPotentiometer : MonoBehaviour
{
    [SerializeField] private Vector2 rotationRange;
    [SerializeField, Range(0f, 1f)] private float value;
    void Start()
    {
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_1, OnControl);
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_2, OnControl);
    }

    void Update()
    {
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(rotationRange.x, rotationRange.y, value));
    }

    void OnControl(float value)
    {
        this.value = value;
    }
}
