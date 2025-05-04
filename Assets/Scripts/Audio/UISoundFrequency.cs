using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UISoundFrequency : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float deltaStep = 2000f;
    [SerializeField] GameObject ui;
    [SerializeField] private Texture2D[] texture2Ds;
    private AudioMixer mixer;
    private RawImage image;

    void Start()
    {
        mixer = audioSource.outputAudioMixerGroup.audioMixer;
        image = ui.GetComponent<RawImage>();
        HideUI();
    }

    void Update() 
    {
        if (audioSource.isPlaying) 
        {
            ShowUI();
        }
        else 
        {
            HideUI();
        }
    }

    void ShowUI()
    {
        mixer.GetFloat("DryLevel", out float dryDb);

        int textureIndex = Mathf.Min(Mathf.CeilToInt(-dryDb / deltaStep), texture2Ds.Length - 1);
        image.texture = texture2Ds[textureIndex];

        image.enabled = true;
    }

    void HideUI()
    {
        image.enabled = false;
        image.texture = null;
    }
}
