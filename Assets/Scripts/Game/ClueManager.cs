using UnityEngine;

public class ClueManager : MonoBehaviour
{
    void Awake()
    {
        ClueDatabase db = ClueDatabase.Instance;
        if (!db.isInstanciated)
        {
            db.isInstanciated = true;
            db.ClearDatabase();
        }
    }
}
