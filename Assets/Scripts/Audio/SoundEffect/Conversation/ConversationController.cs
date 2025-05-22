using UnityEngine;

[RequireComponent(typeof(ConversationVolumeController), typeof(AudioSource))]
public class ConversationController : MonoBehaviour
{
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private CameraZoom cameraZoom;
    [HideInInspector] public float normalSoundValue;
    [HideInInspector] public AudioSource audioSource;
    private ConversationVolumeController soundVolumeController;
    private float volumeMarginError = 0.1f;
    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isRewinding;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        soundVolumeController = GetComponent<ConversationVolumeController>();
    }

    void Start()
    {
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_1, OnJogNote);
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_2, OnJogNote);
        SetRandomNormalSoundValue();
    }

    void Update()
    {
        HandleCollider();
        if (isLookingAt && (audioSource.isPlaying || isRewinding || true) && HasCameraZoom() && IsZooming())
        {
            float volume = GetVolume();
            soundVolumeController.volume = volume;
            if (volume - soundVolumeController.minVolume > volumeMarginError) ConversationManager.Instance.EnableSoundEffect(this);
            else ConversationManager.Instance.DisableSoundEffect(this);
        }
        else
        {
            DisableSoundConversation();
        }
    }

    void DisableSoundConversation()
    {
        ConversationManager.Instance.DisableSoundEffect(this);
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
        sphereCollider.enabled = audioSource.isPlaying || (isRewinding && isLookingAt);
    }

    void OnJogNote(float value)
    {
        if (value < 1)
        {
            isRewinding = false;
        }
        else
        {
            isRewinding = true;
        }
    }

    public void SetRandomNormalSoundValue()
    {
        normalSoundValue = Random.Range(0.1f, 0.9f);
    }
}
