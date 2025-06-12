using UnityEngine;

[CreateAssetMenu(fileName = "Clue", menuName = "Save/Clue")]
public class Clue : ScriptableObject
{
    [HideInInspector] public string id;
    public string title;
    public string description;
    public int points;
    [HideInInspector] public bool isAdded = false;
    [HideInInspector] public bool enabled = true;
    [HideInInspector] public bool isNew = true;

    public void Reset()
    {
        isAdded = false;
        enabled = true;
        isNew = true;
    }
}
