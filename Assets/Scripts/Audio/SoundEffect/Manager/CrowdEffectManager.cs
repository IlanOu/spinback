using UnityEngine;
using UnityEngine.Audio;

public class CrowdEffectManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] crowdSources;
    [SerializeField] private AudioMixer audioMixer;

    [Header("Settings")]
    [SerializeField] private Vector2 crowdVolumeSettings = new Vector2(1f, 1f);
    [SerializeField] private Vector2 crowdLowPassSettings = new Vector2(2000f, 2000f);

    public void Enable()
    {
        audioMixer.SetFloat("CrowdLowPass", crowdLowPassSettings.y);
        foreach (AudioSource source in crowdSources)
        {
            source.volume = crowdVolumeSettings.y;
        }
    }

    public void Disable()
    {
        audioMixer.SetFloat("CrowdLowPass", crowdLowPassSettings.x);
        foreach (AudioSource source in crowdSources)
        {
            source.volume = crowdVolumeSettings.x;
        }
    }
}
