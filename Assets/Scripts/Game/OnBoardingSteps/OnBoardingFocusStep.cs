using UnityEngine;

public class OnBoardingFocusStep : OnBoardingStep
{
    public OnBoardingFocusStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingAddClueStep(manager);

    public new void Hide()
    {
        throw new System.NotImplementedException();
    }

    public new void Show()
    {
        throw new System.NotImplementedException();
    }
}