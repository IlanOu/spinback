using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("Audio Mixer (optionnel)")]
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    [Header("Clips UI")]
    [SerializeField] private UISounds UISounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            UISounds.InitializeAudioSources(gameObject, audioMixerGroup);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Play(UIAudioName name)
    {
        if (!UISounds.audioSources.TryGetValue(name, out var source))
        {
            Debug.LogWarning($"Audio non trouvé pour : {name}");
            return;
        }

        if (source.isPlaying)
        {
            source.Stop(); // Pour éviter de rejouer un clip qui tourne déjà
        }

        source.Play();
    }

    // Méthodes raccourcies
    public void PlayTicPotentiometer() => Play(UIAudioName.TicPotentiometer);
    public void PlayOnCheckedMark() => Play(UIAudioName.OnCheckedMark);
}