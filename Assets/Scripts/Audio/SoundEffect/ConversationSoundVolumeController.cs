using UnityEngine;

public class ConversationSoundVolumeController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Settings")]
    [SerializeField, Tooltip("Volume par défaut du son (au minimum)"), Range(0f, 1f)]
    private float minVolume = 0.1f;
    [SerializeField, Tooltip("Marge du zoom avant d'augmenter le volume")]
    private float marginError = 0.1f;
    private float volume;
    ConversationSoundEffectController controller;


    void Start()
    {
        controller = GetComponent<ConversationSoundEffectController>();
        if (controller == null) Debug.LogError("ConversationSoundEffectController missing");
        volume = minVolume;  
    }

    void Update()
    {
        UpdateVolume();
    }

    void UpdateVolume()
    {
        audioSource.volume = volume;
    }

    public void OnZoom(float zoom, float minZoom, float maxZoom)
    {
        if (maxZoom - zoom < marginError) return;

        float t = Mathf.Clamp01(1f - (zoom - minZoom) / (maxZoom - minZoom));
        volume = Mathf.Lerp(minVolume, 1f, t);

        if (controller != null)
        {
            if (volume - minVolume > marginError)
            {
                controller.EnableSoundEffect();
            }
            else
            {
                controller.DisableSoundEffect();
            }
        }
    }
}