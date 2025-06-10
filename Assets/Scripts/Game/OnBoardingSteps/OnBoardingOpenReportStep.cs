
using UI.Report;
using UnityEditor;

public class OnBoardingOpenReportStep : OnBoardingStep
{
    public OnBoardingOpenReportStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingCloseReportStep(manager);
    private ReportUI reportUI => manager.reportUI;
    private ReportIcon reportIcon;

    public override void Show()
    {
        reportUI.OnOpenReportUI += OnOpenReportUI;

        TooltipActivator.Instance.EnableTooltip(TooltipType.OpenReport);
        TooltipActivator.Instance.SubscribeToDeactivation(TooltipType.OpenReport, TooltipDisabled);

        reportIcon = reportUI.gameObject.GetComponent<ReportIcon>();
        reportIcon.ShowAlertIcon();
    }

    private void TooltipDisabled() => manager.NextStep();

    private void OnOpenReportUI()
    {
        TooltipActivator.Instance.DisableTooltip(TooltipType.OpenReport);
    }

    public override void Hide()
    {
        reportUI.OnOpenReportUI -= OnOpenReportUI;
        TooltipActivator.Instance.UnsubscribeFromDeactivation(TooltipType.OpenReport, TooltipDisabled);
    }
}