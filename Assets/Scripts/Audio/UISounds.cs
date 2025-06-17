using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum UIAudioName
    {
        TicPotentiometer,
        OnCheckedMark
    }

[Serializable]
public class UIAudioClip
{
    public UIAudioName name;
    public AudioClip clip;
    [Range(0f, 1f)]public float volume = 1;
}

[CreateAssetMenu(fileName = "UISounds", menuName = "Sounds/UISounds")]
public class UISounds : ScriptableObject
{
    [SerializeField] private UIAudioClip[] uiAudioClips;
    public Dictionary<UIAudioName, AudioSource> audioSources;

    public void InitializeAudioSources(GameObject gameObject, AudioMixerGroup audioMixerGroup)
    {
        audioSources = new Dictionary<UIAudioName, AudioSource>();

        foreach (var item in uiAudioClips)
        {
            if (item.clip == null)
            {
                Debug.LogWarning($"Clip manquant pour {item.name}");
                continue;
            }

            if (audioSources.ContainsKey(item.name))
            {
                Debug.LogWarning($"Doublon de nom audio : {item.name}");
                continue;
            }

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = item.clip;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = item.volume;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.volume = 1f;
            source.outputAudioMixerGroup = audioMixerGroup;

            audioSources[item.name] = source;
        }
    }
}
