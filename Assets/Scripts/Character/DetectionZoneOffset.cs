using System.Collections;
using UnityEngine;

public class DetectionZoneOffset : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private Vector2 newDetectionCenter;
    [SerializeField] private Vector2 newDetectionSize;

    private Vector2 originalDetectionCenter;
    private Vector2 originalDetectionSize;

    void OnEnable()
    {
        StartCoroutine(WaitToSetOriginalDetectionZone());
    }

    void OnDisable()
    {
        // detectableGameObject.detectionCenter = originalDetectionCenter;
        // detectableGameObject.detectionSize = originalDetectionSize;

        Debug.Log(originalDetectionCenter);
    }

    IEnumerator WaitToSetOriginalDetectionZone()
    {
        yield return new WaitForSecondsRealtime(0.01f);

        // originalDetectionCenter = detectableGameObject.detectionCenter;
        // originalDetectionSize = detectableGameObject.detectionSize;

        // detectableGameObject.detectionCenter = newDetectionCenter;
        // detectableGameObject.detectionSize = newDetectionSize;
    }
}
