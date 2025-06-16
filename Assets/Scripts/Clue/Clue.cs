using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Clue", menuName = "Save/Clue")]
public class Clue : ScriptableObject
{
    [HideInInspector] public string id;
    public string title;
    public string description;
    public int points;
    public bool isAdded = false;
    public bool enabled = true;
    public bool isNew = true;

    public void Reset()
    {
        isAdded = false;
        enabled = true;
        isNew = true;
        Debug.Log("Clue '" + title + "' reset with value: " + isAdded);
    }
}
