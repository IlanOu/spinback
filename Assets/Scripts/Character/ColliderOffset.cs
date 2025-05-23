using UnityEngine;

public class ColliderOffset : MonoBehaviour
{
    [SerializeField] private Collider targetCollider;
    [SerializeField] private Vector3 offset;

    private Vector3 initialCenter;

    void OnEnable()
    {
        if (targetCollider == null)
        {
            Debug.LogWarning($"{name} : Aucun collider assigné.");
            return;
        }

        if (targetCollider is BoxCollider box)
        {
            initialCenter = box.center;
            box.center += offset;
        }
        else if (targetCollider is SphereCollider sphere)
        {
            initialCenter = sphere.center;
            sphere.center += offset;
        }
        else if (targetCollider is CapsuleCollider capsule)
        {
            initialCenter = capsule.center;
            capsule.center += offset;
        }
        else
        {
            Debug.LogWarning($"{targetCollider.name} : Type de collider non supporté.");
        }
    }

    void OnDisable()
    {
        if (targetCollider == null) return;

        if (targetCollider is BoxCollider box)
        {
            box.center = initialCenter;
        }
        else if (targetCollider is SphereCollider sphere)
        {
            sphere.center = initialCenter;
        }
        else if (targetCollider is CapsuleCollider capsule)
        {
            capsule.center = initialCenter;
        }
    }
}