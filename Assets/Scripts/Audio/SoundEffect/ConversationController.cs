using UnityEngine;

[RequireComponent(typeof(SoundEffectController), typeof(SoundVolumeController), typeof(AudioSource))]
public class ConversationController : MonoBehaviour, IDetectableGameObject
{
    [SerializeField] private CameraDetectTarget cameraDetectTarget;
    private SoundEffectController soundEffectController;
    private SoundVolumeController soundVolumeController;
    private AudioSource audioSource;
    private CameraZoom cameraZoom;
    private bool isLookingAt = false;
    private float volumeMarginError = 0.1f;

    void Awake()
    {
        soundEffectController = GetComponent<SoundEffectController>();
        soundVolumeController = GetComponent<SoundVolumeController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (cameraDetectTarget == null)
        {
            Debug.LogError("CameraDetectTarget is not assigned.");
            return;
        }
        cameraZoom = cameraDetectTarget.gameObject.GetComponent<CameraZoom>();
        cameraDetectTarget.Subscribe(this);
    }

    void Update()
    {
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

    public void OnEnter()
    {
        isLookingAt = true;
    }

    public void OnExit()
    {
        isLookingAt = false;
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
        float volume = Mathf.Lerp(cameraZoom.minZoom, 1f, t);
        return volume;
    }
}
