using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MoveCursor : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    public float position;
    private RectTransform rectTransform;
    private RectTransform parentContainer;
    private float width => parentContainer.rect.width;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentContainer = transform.parent.GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.anchoredPosition = new Vector3(position * width, 0, 0);
    }
}
