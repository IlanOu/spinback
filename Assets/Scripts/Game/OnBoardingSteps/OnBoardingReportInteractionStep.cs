
using UnityEngine.UI;

public class OnBoardingReportInteractionStep : OnBoardingStep
{
    public OnBoardingReportInteractionStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => null;
    private Button validateButton => manager.validateButton;

    public override void Show()
    {
        TooltipActivator.Instance.EnableTooltip(TooltipType.ToggleClue);
        TooltipActivator.Instance.EnableTooltip(TooltipType.EyeClue);
        TooltipActivator.Instance.EnableTooltip(TooltipType.ValidateReport);

        validateButton.onClick.AddListener(manager.NextStep);
    }

    public override void Hide()
    {
        TooltipActivator.Instance.DisableAllTooltips();
        
        validateButton.onClick.RemoveListener(manager.NextStep);
    }
}