using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
public enum TooltipType { Mouse, Potentiometer, Slider, JogWheel, OpenReport, ReportExplaination, CloseReport, AddClue };
public class TooltipActivator : MonoBehaviour
{
    public static TooltipActivator Instance;
    [SerializeField] public GameObject mouseTooltip;
    [SerializeField] public GameObject potentiometerTooltip;
    [SerializeField] public GameObject sliderTooltip;
    [SerializeField] public GameObject jogWheelTooltip;
    [SerializeField] public GameObject openReportTooltip;
    [SerializeField] public GameObject reportExplainationTooltip;
    [SerializeField] public GameObject closeReportTooltip;
    [SerializeField] public GameObject addClueTooltip;
    private Vector3 lastMousePosition;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Mouse
        lastMousePosition = Input.mousePosition;

        // Potentiometer
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_1, (float value) => DisableTooltip(TooltipType.Potentiometer));
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_2, (float value) => DisableTooltip(TooltipType.Potentiometer));

        // Slider
        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_1, (float value) => DisableTooltip(TooltipType.Slider));
        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_2, (float value) => DisableTooltip(TooltipType.Slider));

        // Jog wheel
        MidiBinding.Instance.Subscribe(MidiBind.JOG_ROLL_1, (float value) => DisableTooltip(TooltipType.JogWheel));
        MidiBinding.Instance.Subscribe(MidiBind.JOG_ROLL_2, (float value) => DisableTooltip(TooltipType.JogWheel));
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_ROLL_1, (float value) => DisableTooltip(TooltipType.JogWheel));
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_ROLL_2, (float value) => DisableTooltip(TooltipType.JogWheel));
    }

    void Update()
    {
        // If the mouse has moved, disable the tooltip
        if (mouseTooltip.activeSelf && Input.mousePosition != lastMousePosition)
        {
            DisableTooltip(TooltipType.Mouse);
        }
    }

    public void EnableTooltip(TooltipType type)
    {
        UpdateTooltip(type, true);
    }

    public void DisableTooltip(TooltipType type)
    {
        UpdateTooltip(type, false);
    }

    public void UpdateTooltip(TooltipType type, bool enabled)
    {
        switch (type)
        {
            case TooltipType.Mouse:
                if (mouseTooltip != null)
                {
                    mouseTooltip.SetActive(enabled);
                }
                break;
            case TooltipType.Potentiometer:
                if (potentiometerTooltip != null)
                {
                    potentiometerTooltip.SetActive(enabled);
                }
                break;
            case TooltipType.Slider:
                if (sliderTooltip != null)
                {
                    sliderTooltip.SetActive(enabled);
                }
                break;
            case TooltipType.JogWheel:
                if (jogWheelTooltip != null)
                {
                    jogWheelTooltip.SetActive(enabled);
                }
                break;
            case TooltipType.OpenReport:
                if (openReportTooltip != null)
                {
                    openReportTooltip.SetActive(enabled);
                }
                break;
            case TooltipType.ReportExplaination:
                if (reportExplainationTooltip != null)
                {
                    reportExplainationTooltip.SetActive(enabled);
                }
                break;
            case TooltipType.CloseReport:
                if (closeReportTooltip != null)
                {
                    closeReportTooltip.SetActive(enabled);
                }
                break;
            case TooltipType.AddClue:
                if (addClueTooltip != null)
                {
                    addClueTooltip.SetActive(enabled);
                }
                break;
        }
    }

    public void DisableAllTooltips(TooltipType[] exceptions = null)
    {
        List<TooltipType> exceptionsList = new List<TooltipType>();
        if (exceptions != null)
        {
            exceptionsList.AddRange(exceptions);
        }

        foreach (TooltipType type in System.Enum.GetValues(typeof(TooltipType)))
        {
            if (exceptionsList.Contains(type)) continue;
            DisableTooltip(type);
        }
    }
}
