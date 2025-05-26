using UnityEngine;

[RequireComponent(typeof(ConversationVolumeController), typeof(AudioSource))]
public class ConversationController : MonoBehaviour
{
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private DetectableGameObject detectableGameObject;
    [HideInInspector] public float normalSoundValue;
    [HideInInspector] public AudioSource audioSource;
    private ConversationVolumeController soundVolumeController;
    private CameraZoom cameraZoom;
    private float zoomValue;
    private float volumeMarginError = 0.1f;
    private bool isLookingAt => detectableGameObject.isLookingAt;
    private bool isZooming => cameraZoom != null && cameraZoom.IsZooming(zoomValue);
    private bool isFocused => isLookingAt && isZooming;
    private bool isRewinding;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        soundVolumeController = GetComponent<ConversationVolumeController>();
    }

    void Start()
    {
        cameraZoom = Camera.main.GetComponent<CameraZoom>();

        CameraZoomSettings settings = GlobalCameraSettings.Instance.GetSettings<CameraZoomSettings>(ObjectType.Conversation);
        zoomValue = settings.zoomValue;

        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_1, OnJogNote);
        MidiBinding.Instance.Subscribe(MidiBind.JOG_BUTTON_2, OnJogNote);
        
        SetRandomNormalSoundValue();
    }

    void Update()
    {
        HandleCollider();
        if (isFocused && (audioSource.isPlaying || isRewinding))
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
