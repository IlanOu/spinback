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

    public void Subscribe(DetectableGameObject detectable)
    {
        if (!subscribers.Contains(detectable))
        {
            subscribers.Add(detectable);
        }
    }

    public void Unsubscribe(DetectableGameObject detectable)
    {
        subscribers.Remove(detectable);
    }

    void Update()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        // Réinitialiser tous les objets à "non regardé"
        foreach (var detectable in subscribers)
        {
            if (detectable != null)
                detectable.OnExit();
        }

        // Traiter tous les objets touchés
        foreach (RaycastHit hit in hits)
        {
            DetectableGameObject detected = hit.collider.GetComponent<DetectableGameObject>();
            if (detected != null)
            {
                detected.OnEnter();
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Gizmos.DrawRay(ray.origin, ray.direction * 100f);
    }
}