using UnityEngine;

public class OnBoardingEyeStep : OnBoardingStep
{
    public OnBoardingEyeStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => null;

    public override void Hide()
    {
        throw new System.NotImplementedException();
    }

    public override void Show()
    {
        throw new System.NotImplementedException();
    }
}