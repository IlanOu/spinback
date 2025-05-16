using DefaultNamespace;
using Minis;
using UnityEngine;

public class InvestigationReportUI : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private GameObject investigationReportUI;

    private bool isShowing = false;
    private bool isClicking = false;

    void Start()
    {
        MidiBindingRegistry.Instance.Bind(ActionEnum.InvestigationReport, (input) => OnClicked());
    }

    private void Update()
    {
        isClicking = Input.GetKeyDown(KeyCode.I);
        HandleShowInvestigationReport();
    }

    void HandleShowInvestigationReport()
    {
        if (detectableGameObject.isLookingAt && !isShowing && isClicking)
        {
            ShowUI();
            return;
        }

        if (isShowing && isClicking)
        {
            HideUI();
            return;
        }
    }

    void ShowUI()
    {
        investigationReportUI.SetActive(true);
        isShowing = true;
    }

    void HideUI()
    {
        investigationReportUI.SetActive(false);
        isShowing = false;
    }

    void OnClicked()
    {
        if (isShowing)
        {
            HideUI();
        }
        else
        {
            ShowUI();
        }
    }
}
