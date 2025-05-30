using UnityEngine;

public class OnBoardingEyeStep : OnBoardingStep
{
    public OnBoardingEyeStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => null;

    public new void Hide()
    {
        throw new System.NotImplementedException();
    }

    public new void Show()
    {
        throw new System.NotImplementedException();
    }
}