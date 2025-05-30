using UnityEngine;

public class OnBoardingFocusStep : OnBoardingStep
{
    public OnBoardingFocusStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingAddClueStep(manager);
    private TooltipActivator tooltipActivator => manager.tooltipActivator;

    public override void Show()
    {
        tooltipActivator.EnableSliderTooltip();

        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_1, OnSlider);
        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_2, OnSlider);
    }

    void OnSlider(float value) => EndStep();
    
    void EndStep()
    {
        MidiBinding.Instance.Unsubscribe(MidiBind.TEMPO_FADER_1, OnSlider);
        MidiBinding.Instance.Unsubscribe(MidiBind.TEMPO_FADER_2, OnSlider);
        manager.NextStep();
    }
}