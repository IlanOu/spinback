using System.ComponentModel;
using UnityEngine;

public class StartMainGameManager : MonoBehaviour
{
    void Start()
    {
        GameSave gs = GameSave.Instance;
        if (!gs.cinematicIsPlayed || !gs.mainGameIsPlayedFirstTime)
            ClueDatabase.Instance.ClearDatabase();

        gs.StartMainGame();
    }
}
