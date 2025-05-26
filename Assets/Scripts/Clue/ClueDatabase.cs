using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Clue
{
    public string id;
    public string title;
    public string description;
    [HideInInspector] public bool enabled = true;
}

[CreateAssetMenu(fileName = "ClueDatabase", menuName = "Save/Clues")]
public class ClueDatabase : ScriptableObject
{
    [SerializeField] public List<Clue> _clues = new();
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

    public void AddClue(Clue originalClue)
    {
        Clue clueCopy = new Clue
        {
            id = Guid.NewGuid().ToString(),
            title = originalClue.title,
            description = originalClue.description,
            enabled = true,
        };

        if (_clues.Exists(c => c.title == clueCopy.title && c.description == clueCopy.description))
        {
            Debug.Log("Clue already exists in the database.");
            return;
        }
        _clues.Add(clueCopy);
    }

    public void ClearDatabase()
    {
        _clues.Clear();
    }
}
