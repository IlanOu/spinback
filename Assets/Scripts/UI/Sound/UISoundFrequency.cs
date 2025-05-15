using UnityEngine;

public class UISoundFrequency : MonoBehaviour
{
    [HideInInspector] public static UISoundFrequency Instance { get; private set; }
    [SerializeField] private float maxHz = 22000f;
    [SerializeField] private VerticalSinusRenderer playerWave;
    [SerializeField] private VerticalSinusRenderer targetWave;
    private bool active = false;
    
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
    }

    public void Show(GameObject go)
    {
        if (active) return;
        if (subscribe != null) return;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        subscribe = go;
        active = true;
    }

    public void HandleUI(GameObject go, float playerHz, float targetHz)
    {
        if (!active) return;
        if (subscribe != go) return;

        playerWave.SetSinusProfile(playerHz / maxHz);
        targetWave.SetSinusProfile(targetHz / maxHz);
    }

    public void Hide(GameObject go)
    {
        if (!active) return;
        if (subscribe != go) return;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        subscribe = null;
        active = false;
    }
}
