using System.Collections.Generic;
using UnityEngine;

public class ConversingNPCs : MonoBehaviour
{
    [SerializeField] private List<GameObject> npcs = new List<GameObject>();

    public List<GameObject> GetNPCs()
    {
        return npcs;
    }
}
