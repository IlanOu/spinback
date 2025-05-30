using UnityEngine;

public class OnBoardingToggleStep : OnBoardingStep
{
    public OnBoardingToggleStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingEyeStep(manager);

    public override void Hide()
    {
        throw new System.NotImplementedException();
    }


    public override void Show()
    {
        throw new System.NotImplementedException();
    }
}