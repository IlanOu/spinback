using UI.Report;

public class OnBoardingSeeClueStep : OnBoardingStep
{
    public OnBoardingSeeClueStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingToggleStep(manager);
    private ReportUI reportUI => manager.reportUI;

    public override void Show()
    {
        TooltipActivator.Instance.EnableTooltip(TooltipType.SeeClue);
        TooltipActivator.Instance.SubscribeToDeactivation(TooltipType.SeeClue, TooltipDisabled);
        reportUI.OnOpenReportUI += OnOpenReportUI;
    }

    private void TooltipDisabled() => manager.NextStep();

    private void OnOpenReportUI()
    {
        TooltipActivator.Instance.DisableTooltip(TooltipType.SeeClue);
    }

    public override void Hide()
    {
        reportUI.OnOpenReportUI -= OnOpenReportUI;
        TooltipActivator.Instance.UnsubscribeFromDeactivation(TooltipType.SeeClue, TooltipDisabled);
    }
}