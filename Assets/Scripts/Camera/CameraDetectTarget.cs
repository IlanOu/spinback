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
            if (detectable is MonoBehaviour mono && mono.GetComponent<Collider>() == null)
                Debug.LogWarning($"[CameraDetectTarget] {mono.name} n'a pas de collider : Collider.Raycast() ne fonctionnera pas.");

            subscribers.Add(detectable);
        }
    }

    public void Unsubscribe(DetectableGameObject detectable)
    {
        subscribers.Remove(detectable);
    }

    void Update()
    {
        UpdateVisibleTargets();
    }

    private void UpdateVisibleTargets()
    {
        foreach (var detectable in subscribers)
        {
            if (detectable == null || detectable is not MonoBehaviour mono)
                continue;

            Vector3 worldPos = mono.transform.position;
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPos);

            // Objet visible dans le viewport
            bool isInView = viewportPos.z > 0 &&
                            viewportPos.x >= 0f && viewportPos.x <= 1f &&
                            viewportPos.y >= 0f && viewportPos.y <= 1f;

            if (!isInView)
            {
                detectable.OnExit();
                continue;
            }

            Vector3 camPos = mainCamera.transform.position;
            Vector3 dirToTarget = worldPos - camPos;
            float distance = dirToTarget.magnitude;
            dirToTarget.Normalize();

            if (mono.TryGetComponent(out Collider collider))
            {
                Ray ray = new Ray(camPos, dirToTarget);
                if (!collider.Raycast(ray, out RaycastHit hitInfo, distance))
                {
                    detectable.OnExit();
                    continue;
                }
            }
            else
            {
                detectable.OnExit();
                continue;
            }

            // Détection par ellipse
            Vector2 delta = new Vector2(viewportPos.x, viewportPos.y) - detectable.detectionCenter;
            Vector2 normalized = new Vector2(delta.x / detectable.detectionSize.x, delta.y / detectable.detectionSize.y);
            float ellipseDistance = normalized.sqrMagnitude;

            if (ellipseDistance <= 1f)
            {
                detectable.OnEnter();
            }
            else
            {
                detectable.OnExit();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (mainCamera == null || subscribers == null) return;

        foreach (var detectable in subscribers)
        {
            if (detectable == null || detectable is not MonoBehaviour mono)
                continue;

            Vector3 worldPos = mono.transform.position;
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPos);

            if (viewportPos.z <= 0) continue;

            // Ligne vers la cible
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(mainCamera.transform.position, worldPos);

            // Point rouge à la position détectée dans le viewport
            Gizmos.color = Color.red;
            Vector3 point = mainCamera.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, 1f));
            Gizmos.DrawSphere(point, 0.02f);

            // Zone de détection (ellipse projetée)
            DrawEllipse(detectable.detectionCenter, detectable.detectionSize);
        }
    }

    private void DrawEllipse(Vector2 center, Vector2 radius, int segments = 32)
    {
        Gizmos.color = Color.green;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;
            float x = center.x + radius.x * Mathf.Cos(angle);
            float y = center.y + radius.y * Mathf.Sin(angle);
            Vector3 viewport = new Vector3(x, y, 1f);
            points[i] = mainCamera.ViewportToWorldPoint(viewport);
        }
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
}