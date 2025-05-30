using UnityEngine;

public class OnBoardingAddClueStep : OnBoardingStep
{
    public OnBoardingAddClueStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingOpenReportStep(manager);
    private GameObject clue => manager.clue;

    public override void Show()
    {
        InteractableClue interactableClue = clue.GetComponent<InteractableClue>();
        if (interactableClue != null)
        {
            interactableClue.EnableInteractability();
        }
    }

    public override void Hide()
    {
        manager.NextStep();
    }
}