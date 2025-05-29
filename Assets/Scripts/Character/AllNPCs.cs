using System.Collections.Generic;
using UnityEngine;

public class AllNPCs : MonoBehaviour
{
    [HideInInspector] public static AllNPCs Instance;
    [HideInInspector] public List<GameObject> NPCs;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Find all gameobject with tag NPC
        NPCs = new List<GameObject>(GameObject.FindGameObjectsWithTag("NPC"));
    }
}
