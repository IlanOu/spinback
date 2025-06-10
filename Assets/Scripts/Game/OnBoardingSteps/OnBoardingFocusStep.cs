
public class OnBoardingFocusStep : OnBoardingStep
{
    public OnBoardingFocusStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingAddClueStep(manager);
    private bool mouseTooltipEnabled = true;
    private bool sliderTooltipEnabled = true;

    public override void Show()
    {
        TooltipActivator.Instance.SubscribeToDeactivation(TooltipType.Mouse, DisableMouseTooltip);
        TooltipActivator.Instance.SubscribeToDeactivation(TooltipType.Slider, DisableSliderTooltip);

        TooltipActivator.Instance.EnableTooltip(TooltipType.Mouse);
        TooltipActivator.Instance.EnableTooltip(TooltipType.Slider);
    }

    public override void Handle()
    {
        if (!mouseTooltipEnabled && !sliderTooltipEnabled)
        {
            EndStep();
        }
    }

    void EndStep()
    {
        manager.NextStep();
    }

    void DisableMouseTooltip()
    {
        mouseTooltipEnabled = false;
    }

    void DisableSliderTooltip()
    {
        sliderTooltipEnabled = false;
    }

    public override void Hide()
    {
        TooltipActivator.Instance.UnsubscribeFromDeactivation(TooltipType.Mouse, DisableMouseTooltip);
        TooltipActivator.Instance.UnsubscribeFromDeactivation(TooltipType.Slider, DisableSliderTooltip);
    }
}