using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundVolumeController : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField, Tooltip("Volume par défaut du son (au minimum)"), Range(0f, 1f)]
    public float minVolume = 0.1f;
    private AudioSource audioSource;
    [HideInInspector] public float volume;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        volume = minVolume;  
    }

    void Update()
    {
        audioSource.volume = volume;
    }
}