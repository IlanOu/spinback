using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UISoundFrequency : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float deltaStep = 2000f;
    [SerializeField] GameObject ui;
    private AudioMixer mixer;

    void Start()
    {
        mixer = audioSource.outputAudioMixerGroup.audioMixer;
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
        
        int textureIndex = Mathf.Min(Mathf.CeilToInt(-dryDb / deltaStep), ui.transform.childCount - 1);

        for (int i = 0; i < ui.transform.childCount; i++)
        {
            ui.transform.GetChild(i).gameObject.SetActive(i == textureIndex);
        }
    }

    void HideUI()
    {
        for (int i = 0; i < ui.transform.childCount; i++)
        {
            ui.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
