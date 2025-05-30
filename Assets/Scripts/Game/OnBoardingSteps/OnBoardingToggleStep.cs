using UnityEngine;

public class OnBoardingToggleStep : OnBoardingStep
{
    public OnBoardingToggleStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingEyeStep(manager);

    public new void Hide()
    {
        throw new System.NotImplementedException();
    }


    public new void Show()
    {
        throw new System.NotImplementedException();
    }
}