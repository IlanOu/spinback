using UnityEngine;

public class InvestigationReportUI : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private GameObject investigationReportUI;

    private bool isShowing = false;

    private void Update()
    {
        HandleShowInvestigationReport();
    }

    void HandleShowInvestigationReport()
    {
        bool isClicking = Input.GetKeyDown(KeyCode.I);

        if (detectableGameObject.isLookingAt && !isShowing && isClicking)
        {
            investigationReportUI.SetActive(true);
            isShowing = true;
            return;
        }

        if (isShowing && isClicking)
        {
            investigationReportUI.SetActive(false);
            isShowing = false;
            return;
        }
    }
}
