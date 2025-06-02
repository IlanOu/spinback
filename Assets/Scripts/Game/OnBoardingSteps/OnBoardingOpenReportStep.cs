using UnityEngine;

public class OnBoardingOpenReportStep : OnBoardingStep
{
    public OnBoardingOpenReportStep(OnboardingManager manager) : base(manager) {}
    // public override OnBoardingStep NextStep() => null;
    public override OnBoardingStep NextStep() => new OnBoardingToggleStep(manager);
    private ReportUI reportUI => manager.reportUI;
    private ReportIcon reportIcon;

    public override void Show()
    {
        reportUI.OnOpenReportUI += manager.NextStep;

        reportIcon = reportUI.gameObject.GetComponent<ReportIcon>();
        reportIcon.ShowAlertIcon();
    }

    public override void Hide()
    {
        reportUI.OnOpenReportUI -= manager.NextStep;
    }

}