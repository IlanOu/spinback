
using System.Diagnostics;
using UI.Toggle;
using UnityEngine;
using UnityEngine.UI;

public class OnBoardingToggleStep : OnBoardingStep
{
    public OnBoardingToggleStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() =>  new OnBoardingReportInteractionStep(manager);
    private GameObject carousel => manager.carousel;
    private Button button;
    private GameObject activeItemInScene => manager.activeItemInScene;
    private GameObject disactiveItemInScene => manager.disactiveItemInScene;

    public override void Show()
    {
        button = carousel.GetComponentInChildren<SwitchHandler>().GetComponent<Button>();

        TooltipActivator.Instance.EnableTooltip(TooltipType.ToggleClue);
        TooltipActivator.Instance.EnableTooltip(TooltipType.ToggleClick);
        TooltipActivator.Instance.SubscribeToDeactivation(TooltipType.ToggleClick, TooltipDisabled);

        button.onClick.AddListener(OnToggleChanged);
    }
    
    private void TooltipDisabled() => manager.NextStep();

    private void OnToggleChanged()
    {
        activeItemInScene.SetActive(false);
        disactiveItemInScene.SetActive(true);
        button.interactable = false;
        TooltipActivator.Instance.DisableTooltip(TooltipType.ToggleClue);
        TooltipActivator.Instance.DisableTooltip(TooltipType.ToggleClick);
    }

    public override void Hide()
    {
        button.onClick.RemoveListener(OnToggleChanged);
        TooltipActivator.Instance.UnsubscribeFromDeactivation(TooltipType.ToggleClick, TooltipDisabled);
    }
}