using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDetectTarget : MonoBehaviour
{
    private Camera mainCamera;
    private List<DetectableGameObject> subscribers = new();

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    public void Subscribe(DetectableGameObject subscriber)
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
        foreach (var detectable in subscribers)
        {
            if (detectable is not MonoBehaviour mono) continue;

            Vector3 worldPos = mono.transform.position;
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPos);

            // Test si l’objet est devant la caméra et dans le viewport [0,1]
            bool isInView = 
                viewportPos.z > 0 &&
                viewportPos.x >= 0 && viewportPos.x <= 1 &&
                viewportPos.y >= 0 && viewportPos.y <= 1;

            if (!isInView)
                continue;

            // Raycast pour vérifier qu'aucun obstacle ne masque l’objet
            Vector3 camPos = mainCamera.transform.position;
            Vector3 dirToTarget = (worldPos - camPos).normalized;
            float distance = Vector3.Distance(camPos, worldPos);

            if (Physics.Raycast(camPos, dirToTarget, out RaycastHit hit, distance))
            {
                if (hit.transform != mono.transform)
                    continue;
            }

            // Détection valide → appel OnDetect avec la position viewport (x,y)
            Vector2 centered = new Vector2(viewportPos.x, viewportPos.y) - detectable.detectionZone;
            float distanceFromCenter = centered.magnitude - 1; // - 1 est necessaire
            if (distanceFromCenter <= detectable.detectionPrecision)
            {
                detectable.OnEnter();
            }
            else
            {
                detectable.OnExit();
            }
        }
    }
}