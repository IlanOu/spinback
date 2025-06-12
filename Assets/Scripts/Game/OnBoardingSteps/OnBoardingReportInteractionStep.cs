
using UI.Report;
using UnityEngine.UI;

public class OnBoardingReportInteractionStep : OnBoardingStep
{
    public OnBoardingReportInteractionStep(OnboardingManager manager) : base(manager) { }
    public override OnBoardingStep NextStep() => null;
    private Button validateButton => manager.validateButton;
    private ReportUI reportUI => manager.reportUI;

    public override void Show()
    {
        TooltipActivator.Instance.EnableTooltip(TooltipType.ValidateReport);
        TooltipActivator.Instance.SubscribeToDeactivation(TooltipType.ValidateReport, TooltipDisabled);

        validateButton.onClick.AddListener(OnButtonClicked);
    }
    
    private void TooltipDisabled() => manager.NextStep();

    private void OnButtonClicked()
    {
        TooltipActivator.Instance.DisableTooltip(TooltipType.ValidateReport);
    }

    public override void Hide()
    {
        reportUI.CanCloseReport(true);
        validateButton.onClick.RemoveListener(manager.NextStep);
        TooltipActivator.Instance.UnsubscribeFromDeactivation(TooltipType.ValidateReport, TooltipDisabled);
    }
}