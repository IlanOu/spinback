using UnityEngine;

[RequireComponent(typeof(SoundEffectController), typeof(SoundVolumeController), typeof(AudioSource))]
public class ConversationController : MonoBehaviour
{
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private CameraZoom cameraZoom;
    private SoundEffectController soundEffectController;
    private SoundVolumeController soundVolumeController;
    private AudioSource audioSource;
    private float volumeMarginError = 0.1f;
    private bool isLookingAt => detectableGameObject.isLookingAt;

    void Awake()
    {
        soundEffectController = GetComponent<SoundEffectController>();
        soundVolumeController = GetComponent<SoundVolumeController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleCollider();
        if (isLookingAt && audioSource.isPlaying && HasCameraZoom() && IsZooming())
        {
            float volume = GetVolume();
            soundVolumeController.volume = volume;
            if (volume - soundVolumeController.minVolume > volumeMarginError) soundEffectController.EnableSoundEffect();
            else soundEffectController.DisableSoundEffect();
        }
        else
        {
            DisableSoundConversation();
        }
    }

    void DisableSoundConversation()
    {
        soundEffectController.DisableSoundEffect();
        soundVolumeController.volume = soundVolumeController.minVolume;
    }

    bool HasCameraZoom()
    {
        return cameraZoom != null;
    }

    bool IsZooming()
    {
        if (cameraZoom == null) return false;
        return cameraZoom.isZooming;
    }

    float GetVolume()
    {
        float t = Mathf.Clamp01(1f - (cameraZoom.currentZoom - cameraZoom.minZoom) / (cameraZoom.maxZoom - cameraZoom.minZoom));
        float volume = Mathf.Lerp(soundVolumeController.minVolume, 1f, t);
        return volume;
    }

    void HandleCollider()
    {
        sphereCollider.enabled = audioSource.isPlaying;
    }
}
