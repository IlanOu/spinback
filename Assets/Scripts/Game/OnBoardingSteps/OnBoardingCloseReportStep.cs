
using System.Collections;
using UI.Report;
using UnityEngine;

public class OnBoardingCloseReportStep : OnBoardingStep
{
    public OnBoardingCloseReportStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingFocusStep(manager);
    private ReportUI reportUI => manager.reportUI;

    public override void Show()
    {
        reportUI.OnCloseReportUI += OnCloseReportUI;

        TooltipActivator.Instance.EnableTooltip(TooltipType.ReportExplaination);
        manager.StartCoroutine(WaitBeforeCloseReportTooltip());
    }

    IEnumerator WaitBeforeCloseReportTooltip()
    {
        yield return new WaitForSeconds(3f);

        reportUI.CanCloseReport(true);
        reportUI.CanOpenReport(false);
        
        TooltipActivator.Instance.EnableTooltip(TooltipType.CloseReport);
        TooltipActivator.Instance.SubscribeToDeactivation(TooltipType.CloseReport, TooltipDisabled);
    }

    private void TooltipDisabled() => manager.NextStep();

    private void OnCloseReportUI()
    {
        TooltipActivator.Instance.DisableTooltip(TooltipType.CloseReport);
        TooltipActivator.Instance.DisableTooltip(TooltipType.ReportExplaination);
    }

    public override void Hide()
    {
        reportUI.OnCloseReportUI -= OnCloseReportUI;
        TooltipActivator.Instance.UnsubscribeFromDeactivation(TooltipType.CloseReport, TooltipDisabled);
        TooltipActivator.Instance.DisableTooltip(TooltipType.ReportExplaination);
    }

}