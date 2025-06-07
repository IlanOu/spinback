using UnityEngine;

public class ReportIcon : MonoBehaviour
{
    public static ReportIcon Instance;
    [SerializeField] private GameObject openIcon;
    [SerializeField] private GameObject closeIcon;
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
        openIcon.SetActive(false);
        closeIcon.SetActive(false);
        alertIcon.SetActive(true);
    }

    public void ShowOpenIcon()
    {
        openIcon.SetActive(true);
        closeIcon.SetActive(false);
        alertIcon.SetActive(false);
    }

    public void ShowCloseIcon()
    {
        openIcon.SetActive(false);
        closeIcon.SetActive(true);
        alertIcon.SetActive(false);
    }
}
