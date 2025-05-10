using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDetectTarget : MonoBehaviour
{
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

        foreach (var detectable in subscribers)
        {
            MonoBehaviour mono = detectable as MonoBehaviour;
            if (mono == null) continue;

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(mono.transform.position);
            bool inView =
                viewportPos.z > 0 &&                    // devant la caméra
                viewportPos.x >= 0 && viewportPos.x <= 1 &&
                viewportPos.y >= 0 && viewportPos.y <= 1;

            if (inView)
            {
                newVisible.Add(detectable);
                if (!currentlyVisible.Contains(detectable))
                    detectable.OnEnter();
            }
        }

        // Détection des objets sortis de vision
        foreach (var previouslyVisible in currentlyVisible)
        {
            if (!newVisible.Contains(previouslyVisible))
                previouslyVisible.OnExit();
        }

        // Mise à jour de l’état
        currentlyVisible = newVisible;
    }
}