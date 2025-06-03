
public class OnBoardingFocusStep : OnBoardingStep
{
    public OnBoardingFocusStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingAddClueStep(manager);
    private bool mouseTooltipEnabled => TooltipActivator.Instance.mouseTooltip.activeSelf;
    private bool sliderTooltipEnabled => TooltipActivator.Instance.sliderTooltip.activeSelf;

    public override void Show()
    {
        TooltipActivator.Instance.EnableTooltip(TooltipType.Mouse);
        TooltipActivator.Instance.EnableTooltip(TooltipType.Slider);
    }

    public override void Handle()
    {
        if (mouseTooltipEnabled && sliderTooltipEnabled)
        {
            EndStep();
        }
    }

    void EndStep()
    {
        manager.NextStep();
    }

    public override void Hide()
    {
        TooltipActivator.Instance.DisableAllTooltips();
    }
}