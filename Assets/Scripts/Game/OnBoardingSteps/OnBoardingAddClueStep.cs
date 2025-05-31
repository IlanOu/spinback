using UnityEngine;

public class OnBoardingAddClueStep : OnBoardingStep
{
    public OnBoardingAddClueStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingOpenReportStep(manager);
    private GameObject clue => manager.clue;
    private InteractableClue interactableClue;

    public override void Show()
    {
        interactableClue = clue.GetComponent<InteractableClue>();
        interactableClue.EnableInteractability();

        interactableClue.OnClueAdded += manager.NextStep;
    }

    public override void Hide()
    {
        interactableClue.OnClueAdded -= manager.NextStep;
        interactableClue.enabled = false;

        OutlineObject outlineObject = clue.GetComponent<OutlineObject>();
        outlineObject.EnableVisibility(false);
    }
}