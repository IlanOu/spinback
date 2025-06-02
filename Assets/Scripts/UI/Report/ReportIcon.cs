using UnityEngine;

public class ReportIcon : MonoBehaviour
{
    public static ReportIcon Instance;
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject alertIcon;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowAlertIcon()
    {
        icon.SetActive(false);
        alertIcon.SetActive(true);
    }

    public void HideAlertIcon()
    {
        icon.SetActive(true);
        alertIcon.SetActive(false);
    }
}
