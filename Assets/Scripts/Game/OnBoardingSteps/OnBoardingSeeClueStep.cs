
public class OnBoardingSeeClueStep : OnBoardingStep
{
    public OnBoardingSeeClueStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingReportInteractionStep(manager);
    private ReportUI reportUI => manager.reportUI;

    public override void Show()
    {
        TooltipActivator.Instance.EnableTooltip(TooltipType.SeeClue);
        reportUI.OnOpenReportUI += manager.NextStep;
    }

    public override void Hide()
    {
        reportUI.OnOpenReportUI -= manager.NextStep;
        TooltipActivator.Instance.DisableAllTooltips();
    }
}