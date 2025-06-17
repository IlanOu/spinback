using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class UISoundFrequency : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private float noiseAmplitude = 0.05f; // Maximum offset
    [SerializeField] private float noiseSpeed = 1f;         // Speed of noise variation
    [SerializeField] PlayableDirector director;

    [SerializeField] private float minSliderValue = 0.1f;
    
    private bool active = false;
    private float baseValue = 0f;
    private float noiseSeed;

    void Awake()
    {
        noiseSeed = Random.Range(0f, 1000f); // Random start for variety
    }

    private void Update()
    {
        if (!active) return;

        float time = (float)director.time * noiseSpeed;
        float noise = Mathf.PerlinNoise(noiseSeed, time) * 2f - 1f; // Range [-1,1]
        float noisyValue = Mathf.Clamp01(baseValue + noise * noiseAmplitude);

        slider.value = noisyValue;
    }

    public void Show()
    {
        if (active) return;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        active = true;
    }

    public void HandleUI(float distance)
    {
        if (!active) return;
        baseValue = Mathf.Clamp01(distance);
        baseValue = Mathf.Min(1f, baseValue + minSliderValue);
    }

    public void Hide()
    {
        if (!active) return;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        active = false;
    }
}