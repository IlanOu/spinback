using UnityEngine;

public abstract class OnBoardingStep
{
    protected OnboardingManager manager;
    public OnBoardingStep(OnboardingManager manager)
    {
        this.manager = manager;
    }

    public abstract OnBoardingStep NextStep();
    public virtual void Show() { }
    public virtual void Handle() {}
    public virtual void Hide() { }
}