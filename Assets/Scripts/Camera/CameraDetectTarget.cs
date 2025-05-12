using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDetectTarget : MonoBehaviour
{
    [SerializeField] private Vector2 detectionZoneSize = new Vector2(0.5f, 0.5f);

    private Camera mainCamera;
    private List<IDetectableGameObject> subscribers = new(); // objets enregistrés
    private HashSet<IDetectableGameObject> currentlyVisible = new(); // objets dans le champ actuel

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    public void Subscribe(IDetectableGameObject subscriber)
    {
        if (!subscribers.Contains(subscriber))
        {
            subscribers.Add(subscriber);
        }
    }

    void Update()
    {
        UpdateVisibleTargets();
    }

    void UpdateVisibleTargets()
    {
        HashSet<IDetectableGameObject> newVisible = new();

        // Calcul des limites de la zone centrée dans le viewport
        float halfWidth = detectionZoneSize.x / 2f;
        float halfHeight = detectionZoneSize.y / 2f;
        float minX = 0.5f - halfWidth;
        float maxX = 0.5f + halfWidth;
        float minY = 0.5f - halfHeight;
        float maxY = 0.5f + halfHeight;

        foreach (var detectable in subscribers)
        {
            if (detectable is not MonoBehaviour mono) continue;

            Vector3 worldPos = mono.transform.position;
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPos);

            bool inView =
                viewportPos.z > 0 &&
                viewportPos.x >= minX && viewportPos.x <= maxX &&
                viewportPos.y >= minY && viewportPos.y <= maxY;

            if (!inView)
                continue;

            Vector3 camPos = mainCamera.transform.position;
            Vector3 dirToTarget = (worldPos - camPos).normalized;
            float distance = Vector3.Distance(camPos, worldPos);

            if (Physics.Raycast(camPos, dirToTarget, out RaycastHit hit, distance))
            {
                if (hit.transform != mono.transform)
                    continue;
            }

            newVisible.Add(detectable);

            if (!currentlyVisible.Contains(detectable))
                detectable.OnEnter();
        }

        foreach (var previouslyVisible in currentlyVisible)
        {
            if (!newVisible.Contains(previouslyVisible))
                previouslyVisible.OnExit();
        }

        currentlyVisible = newVisible;
    }

    void OnDrawGizmos()
    {
        if (mainCamera == null || !mainCamera.enabled)
            return;

        float halfWidth = detectionZoneSize.x / 2f;
        float halfHeight = detectionZoneSize.y / 2f;

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0.5f - halfWidth, 0.5f - halfHeight, mainCamera.nearClipPlane + 0.5f));
        Vector3 bottomRight = mainCamera.ViewportToWorldPoint(new Vector3(0.5f + halfWidth, 0.5f - halfHeight, mainCamera.nearClipPlane + 0.5f));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(0.5f + halfWidth, 0.5f + halfHeight, mainCamera.nearClipPlane + 0.5f));
        Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0.5f - halfWidth, 0.5f + halfHeight, mainCamera.nearClipPlane + 0.5f));

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}