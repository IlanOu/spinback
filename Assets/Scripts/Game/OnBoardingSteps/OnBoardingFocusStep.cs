using UnityEngine;

public class OnBoardingFocusStep : OnBoardingStep
{
    public OnBoardingFocusStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingAddClueStep(manager);
    private TooltipActivator tooltipActivator => manager.tooltipActivator;

    private bool mouseTooltipEnabled => tooltipActivator.mouseTooltip.activeSelf;
    private bool sliderTooltipEnabled => tooltipActivator.sliderTooltip.activeSelf;

    public override void Show()
    {
        tooltipActivator.EnableMouseTooltip();
        tooltipActivator.EnableSliderTooltip();
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
}