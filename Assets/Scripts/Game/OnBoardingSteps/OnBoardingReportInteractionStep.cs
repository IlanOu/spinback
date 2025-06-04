
public class OnBoardingReportInteractionStep : OnBoardingStep
{
    public OnBoardingReportInteractionStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => null;

    public override void Show()
    {
        TooltipActivator.Instance.EnableTooltip(TooltipType.ToggleClue);
        TooltipActivator.Instance.EnableTooltip(TooltipType.EyeClue);
    }

    public override void Hide()
    {
        TooltipActivator.Instance.DisableAllTooltips();
    }
}