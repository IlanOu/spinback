using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class UISoundFrequency : MonoBehaviour
{
    [HideInInspector] public static UISoundFrequency Instance { get; private set; }
    [SerializeField] private float maxHz = 22000f;
    [SerializeField] GameObject ui;
    private bool active = false;
    private int childCount => ui.transform.childCount;
    private float deltaHzStep => maxHz / childCount;
    
    // The first subscriber is the only that can handle the UI
    private GameObject subscribe;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Show(GameObject go)
    {
        if (active) return;
        if (subscribe != null) return;

        subscribe = go;
        active = true;
    }

    public void HandleUI(GameObject go, float hz)
    {
        if (!active) return;
        if (subscribe != go) return;
        
        int textureIndex = Mathf.Min(Mathf.RoundToInt(hz / deltaHzStep), childCount - 1);

        for (int i = 0; i < childCount; i++)
        {
            if (i == textureIndex)
            {
                // Enable
                ui.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                // Disable
                ui.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void Hide(GameObject go)
    {
        if (!active) return;
        if (subscribe != go) return;

        for (int i = 0; i < childCount; i++)
        {
            ui.transform.GetChild(i).gameObject.SetActive(false);
        }

        subscribe = null;
        active = false;
    }
}
