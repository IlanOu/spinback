using UnityEngine;

public class InvestigationReportInteraction : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private float outlineWidth = 0.005f;

    private Material material;

    void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (detectableGameObject.isLookingAt)
        {
            material.SetFloat("_OutlineWidth", outlineWidth);
        }
        else
        {
            material.SetFloat("_OutlineWidth", 0f);
        }
    }
}
