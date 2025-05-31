using UnityEngine;

public class TooltipActivator : MonoBehaviour
{
    [SerializeField] public GameObject mouseTooltip;
    [SerializeField] public GameObject potentiometerTooltip;
    [SerializeField] public GameObject sliderTooltip;
    [SerializeField] public GameObject jogWheelTooltip;
    private Vector3 lastMousePosition;

    void Start()
    {
        // Mouse
        lastMousePosition = Input.mousePosition;

        // Potentiometer
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_1, (float value) => DisablePotentiometerTooltip());
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_2, (float value) => DisablePotentiometerTooltip());

        // Slider
        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_1, (float value) => DisableSliderTooltip());
        MidiBinding.Instance.Subscribe(MidiBind.TEMPO_FADER_2, (float value) => DisableSliderTooltip());

        // Jog wheel
        MidiBinding.Instance.Subscribe(MidiBind.JOG_ROLL_1, (float value) => DisableJogWheelTooltip());
        MidiBinding.Instance.Subscribe(MidiBind.JOG_ROLL_2, (float value) => DisableJogWheelTooltip());
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_ROLL_1, (float value) => DisableJogWheelTooltip());
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_ROLL_2, (float value) => DisableJogWheelTooltip());
    }

    void Update()
    {
        // If the mouse has moved, disable the tooltip
        if (mouseTooltip.activeSelf && Input.mousePosition != lastMousePosition)
        {
            DisableMouseTooltip();
        }
    }
    
    void DisableMouseTooltip()
    {
        if (mouseTooltip != null)
        {
            mouseTooltip.SetActive(false);
        }
    }

    void DisablePotentiometerTooltip()
    {
        if (potentiometerTooltip != null)
        {
            potentiometerTooltip.SetActive(false);
        }
    }

    void DisableSliderTooltip()
    {
        if (sliderTooltip != null)
        {
            sliderTooltip.SetActive(false);
        }
    }

    void DisableJogWheelTooltip()
    {
        if (jogWheelTooltip != null)
        {
            jogWheelTooltip.SetActive(false);
        }
    }

    public void EnableMouseTooltip()
    {
        if (mouseTooltip != null)
        {
            mouseTooltip.SetActive(true);
        }
    }

    public void EnablePotentiometerTooltip()
    {
        if (potentiometerTooltip != null)
        {
            potentiometerTooltip.SetActive(true);
        }
    }

    public void EnableSliderTooltip()
    {
        if (sliderTooltip != null)
        sliderTooltip.SetActive(true);
    }

    public void EnableJogWheelTooltip()
    {
        if (jogWheelTooltip != null)
        {
            jogWheelTooltip.SetActive(true);
        }
    }
}
