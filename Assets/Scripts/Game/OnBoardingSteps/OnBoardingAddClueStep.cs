
public class OnBoardingAddClueStep : OnBoardingStep
{
    public OnBoardingAddClueStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingSeeClueStep(manager);
    private InteractableClue interactableClue => manager.interactableClue;
    private OutlineObject outlineClue => manager.outlineClue;
    private ClueInteractiveIcon interactiveIcon => manager.interactiveIcon;

    public override void Show()
    {
        interactableClue.EnableInteractability();
        interactableClue.OnClueAdded += manager.NextStep;

        interactiveIcon.EnableVisibility(true);

        outlineClue.EnableVisibility(true);

        TooltipActivator.Instance.EnableTooltip(TooltipType.AddClue);
    }

    public override void Hide()
    {
        interactableClue.OnClueAdded -= manager.NextStep;
        interactableClue.enabled = false;

        interactiveIcon.EnableVisibility(false);

        outlineClue.EnableVisibility(false);

        TooltipActivator.Instance.DisableAllTooltips();
    }
}