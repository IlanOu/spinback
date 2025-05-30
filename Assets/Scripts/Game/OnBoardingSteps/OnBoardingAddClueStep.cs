using UnityEngine;

public class OnBoardingAddClueStep : OnBoardingStep
{
    public OnBoardingAddClueStep(OnboardingManager manager) : base(manager) {}
    public override OnBoardingStep NextStep() => new OnBoardingOpenReportStep(manager);

    public new void Hide()
    {
        throw new System.NotImplementedException();
    }

    public new void Show()
    {
        throw new System.NotImplementedException();
    }
}