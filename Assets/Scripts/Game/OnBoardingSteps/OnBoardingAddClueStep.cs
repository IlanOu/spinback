
public class OnBoardingAddClueStep : OnBoardingStep
{
    public OnBoardingAddClueStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => null;
    private InteractableClue interactableClue => manager.interactableClue;
    private OutlineObject outlineClue => manager.outlineClue;

    public override void Show()
    {
        interactableClue.EnableInteractability();
        interactableClue.OnClueAdded += manager.NextStep;

        outlineClue.EnableVisibility(true);

        TooltipActivator.Instance.EnableTooltip(TooltipType.AddClue);
    }

    public override void Hide()
    {
        interactableClue.OnClueAdded -= manager.NextStep;
        interactableClue.enabled = false;

        outlineClue.EnableVisibility(false);

        TooltipActivator.Instance.DisableAllTooltips();
    }
}