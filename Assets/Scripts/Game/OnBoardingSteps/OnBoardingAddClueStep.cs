
using UnityEditor;
using UnityEngine;

public class OnBoardingAddClueStep : OnBoardingStep
{
    public OnBoardingAddClueStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => new OnBoardingSeeClueStep(manager);
    private InteractableClue interactableClue => manager.interactableClue;
    private OutlineObject outlineClue => manager.outlineClue;
    private ClueInteractiveIcon interactiveIcon => manager.interactiveIcon;
    private GameObject activeItemInScene => manager.activeItemInScene;
    private GameObject disactiveItemInScene => manager.disactiveItemInScene;

    public override void Show()
    {
        interactableClue.EnableInteractability();
        interactableClue.OnClueAdded += OnClueAdded;

        interactiveIcon.EnableVisibility(true);

        outlineClue.EnableVisibility(true);

        TooltipActivator.Instance.EnableTooltip(TooltipType.AddClue);
        TooltipActivator.Instance.SubscribeToDeactivation(TooltipType.AddClue, TooltipDisabled);
    }

    private void TooltipDisabled() => manager.NextStep();

    private void OnClueAdded()
    {
        activeItemInScene.SetActive(true);
        TooltipActivator.Instance.DisableTooltip(TooltipType.AddClue);
    }

    public override void Hide()
    {
        interactableClue.OnClueAdded -= OnClueAdded;
        interactableClue.enabled = false;

        interactiveIcon.EnableVisibility(false);

        outlineClue.EnableVisibility(false);

        TooltipActivator.Instance.UnsubscribeFromDeactivation(TooltipType.AddClue, TooltipDisabled);
    }
}