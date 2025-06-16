using System.ComponentModel;
using UnityEngine;

public class ClueManager : MonoBehaviour
{
    [SerializeField] ClueDatabase db;
    void Awake()
    {
        db = ClueDatabase.Instance;
        if (!db.isInstanciated)
        {
            db.ClearDatabase();
        }
    }
}
