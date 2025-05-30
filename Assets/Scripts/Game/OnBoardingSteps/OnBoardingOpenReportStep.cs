using UnityEngine;

public class OnBoardingOpenReportStep : OnBoardingStep
{
    public OnBoardingOpenReportStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingToggleStep(manager);

    public override void Hide()
    {
        throw new System.NotImplementedException();
    }

    public override void Show()
    {
        throw new System.NotImplementedException();
    }
}