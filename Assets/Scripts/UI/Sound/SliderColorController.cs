using UnityEngine;
using UnityEngine.UI;

public class SliderColorController : MonoBehaviour
{
    public Slider slider;
    public Image fillImage;

    [Header("Color Gradient")]
    public Gradient colorGradient;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (fillImage == null && slider.fillRect != null)
            fillImage = slider.fillRect.GetComponent<Image>();

        slider.onValueChanged.AddListener(UpdateSliderColor);
        UpdateSliderColor(slider.value);
    }

    void UpdateSliderColor(float value)
    {
        if (fillImage == null || colorGradient == null) return;
        fillImage.color = colorGradient.Evaluate(value);
    }
}