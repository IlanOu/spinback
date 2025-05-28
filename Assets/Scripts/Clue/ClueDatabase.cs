using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Clue
{
    [HideInInspector] public string id;
    public string popup;
    public string text;
    public string hours;
    [HideInInspector] public bool enabled = true;
}

[CreateAssetMenu(fileName = "ClueDatabase", menuName = "Save/Clues")]
public class ClueDatabase : ScriptableObject
{
    [SerializeField] private List<Clue> _clues = new();
    public List<Clue> Clues => _clues;

    private static ClueDatabase _instance;
    public static ClueDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ClueDatabase>("ClueDatabase");
                if (_instance == null)
                {
                    Debug.LogError("ClueDatabase asset not found in Resources!");
                    return null;
                }
                _instance.ClearDatabase();
            }
            return _instance;
        }
    }

    public bool AddClue(Clue originalClue)
    {
        Clue clueCopy = new Clue
        {
            id = Guid.NewGuid().ToString(),
            popup = originalClue.popup,
            hours = originalClue.hours,
            text = originalClue.text,
            enabled = true,
        };

        if (_clues.Exists(c => c.popup == clueCopy.popup && c.hours == clueCopy.hours && c.text == clueCopy.text))
        {
            Debug.Log("Clue already exists in the database.");
            return false;
        }
        _clues.Add(clueCopy);
        return true;
    }

    public void ClearDatabase()
    {
        _clues.Clear();
    }
}
