using UnityEngine;

public class OnBoardingOpenReportStep : OnBoardingStep
{
    public OnBoardingOpenReportStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingToggleStep(manager);

    public new void Hide()
    {
        throw new System.NotImplementedException();
    }

    public new void Show()
    {
        throw new System.NotImplementedException();
    }
}