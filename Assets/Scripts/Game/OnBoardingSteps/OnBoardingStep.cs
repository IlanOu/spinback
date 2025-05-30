using UnityEngine;

public abstract class OnBoardingStep
{
    protected OnboardingManager manager;
    public OnBoardingStep(OnboardingManager manager)
    {
        this.manager = manager;
    }
    
    public abstract OnBoardingStep NextStep();
    public void Show() { }
    public void Hide() { }
}