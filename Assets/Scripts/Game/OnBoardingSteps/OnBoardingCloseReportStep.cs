
public class OnBoardingCloseReportStep : OnBoardingStep
{
    public OnBoardingCloseReportStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingFocusStep(manager);
    private ReportUI reportUI => manager.reportUI;

    public override void Show()
    {
        reportUI.OnCloseReportUI += manager.NextStep;

        TooltipActivator.Instance.EnableTooltip(TooltipType.ReportExplaination);
        TooltipActivator.Instance.EnableTooltip(TooltipType.CloseReport);
    }

    public override void Hide()
    {
        reportUI.OnCloseReportUI -= manager.NextStep;

        TooltipType[] exceptions = new TooltipType[] { TooltipType.CloseReport };
        TooltipActivator.Instance.DisableAllTooltips(exceptions);
    }

}