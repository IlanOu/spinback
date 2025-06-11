using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClueDatabase", menuName = "Save/ClueDatabase")]
public class ClueDatabase : ScriptableObject
{
    [SerializeField] private List<Clue> _clues = new();
    public List<Clue> Clues => _clues;
    public List<Clue> AddedClues => _clues.FindAll(c => c.isAdded);

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

    public bool AddClue(Clue clue)
    {
        if (clue.isAdded) return false;

        clue.isAdded = true;
        return true;
    }

    public void ClearDatabase()
    {
        foreach (var clue in _clues)
        {
            clue.isAdded = false;
            clue.enabled = true;
            clue.isNew = true;
        }
    }

    public int GetMaxPoints()
    {
        int maxPoints = 0;
        foreach (var clue in _clues)
        {
            if (clue.points > 0)
            {
                maxPoints += clue.points;
            }
        }
        return maxPoints;
    }

    public int GetTotalPoints()
    {
        int totalPoints = 0;
        foreach (var clue in _clues)
        {
            if (clue.enabled)
            {
                totalPoints += clue.points;
            }
        }
        return totalPoints;
    }
}
