using UnityEngine;

public class TooltipActivator : MonoBehaviour
{
    [SerializeField] private GameObject potentiometerTooltip;
    [SerializeField] private GameObject sliderTooltip;
    [SerializeField] private GameObject jogWheelTooltip;

    void Start()
    {
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
