using UnityEngine;

[CreateAssetMenu(fileName = "GameSave", menuName = "Save/GameSave")]
public class GameSave : ScriptableObject
{
    public bool cinematicIsPlayed = false;
    public bool mainGameIsPlayedFirstTime = false;
    public bool mainGameIsPlayedSecondTime = false;

    private static GameSave _instance;
    public static GameSave Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameSave>("GameSave");
                if (_instance == null)
                {
                    Debug.LogError("GameSave asset not found in Resources!");
                    return null;
                }
            }
            return _instance;
        }
    }

    public void StartCinematic()
    {
        cinematicIsPlayed = true;
        mainGameIsPlayedFirstTime = false;
        mainGameIsPlayedSecondTime = false;
    }

    public void StartMainGame()
    {
        if (mainGameIsPlayedFirstTime) mainGameIsPlayedSecondTime = true;
        else mainGameIsPlayedFirstTime = true;
    }

    public void Clear()
    {
        cinematicIsPlayed = false;
        mainGameIsPlayedFirstTime = false;
        mainGameIsPlayedSecondTime = false;
    }
}
