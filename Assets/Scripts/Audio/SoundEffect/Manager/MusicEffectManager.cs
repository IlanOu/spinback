using UnityEngine;
using UnityEngine.Audio;

public class MusicEffectManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] musicSources;
    [SerializeField] private AudioMixer audioMixer;

    [Header("Settings")]
    [SerializeField] private Vector2 musicVolumeSettings = new Vector2(0.5f, 1f);
    [SerializeField] private Vector2 musicLowPassSettings = new Vector2(800f, 22000f);
    [SerializeField] private Vector2 resonanceSettings = new Vector2(2f, 1f);

    public void Enable()
    {
        audioMixer.SetFloat("MusicLowPass", musicLowPassSettings.x);
        audioMixer.SetFloat("MusicResonance", resonanceSettings.x);
        foreach (AudioSource source in musicSources)
        {
            source.volume = musicVolumeSettings.x;
        }
    }

    public void Disable()
    {
        audioMixer.SetFloat("MusicLowPass", musicLowPassSettings.y);
        audioMixer.SetFloat("MusicResonance", resonanceSettings.y);
        foreach (AudioSource source in musicSources)
        {
            source.volume = musicVolumeSettings.y;
        }
    }
}
