using Minis;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundEffectController : MonoBehaviour
{
    /** Serialized variables */
    [SerializeField] private string lowPassVariableName;

    [Range(0f, 1f), Header("Valeur contrôlée entre 10 et 22000 Hz")]
    public float balance = 0f;

    [SerializeField, Range(0f, 1f), Header("Valeur à trouver pour avoir un son normal")]
    private float normalSoundValue = 0.5f;

    /** Settings */
    [Header("Settings")]

    [SerializeField, Tooltip("Valeur minimale et maximale de l'effet sonore")]
    private Vector2 hzSettings = new Vector2(10f, 22000f);
    
    [SerializeField, Tooltip("Valeur du son étouffé des conversations (Hz)")]
    private float conversationHzSettings = 200f;

    [SerializeField, Tooltip("Valeur du son étouffé de la musique (Hz)")]
    private float musicHzSettings = 800f;
    
    [SerializeField, Tooltip("Valeur du son étouffé de la foule (Hz)")]
    private float crowdHzSettings = 2000f;

    [SerializeField] private float exponent = 4f;
    [SerializeField] private bool soundEffectEnabled = false;

    /** Dynamic variables */
    private AudioMixer mixer => audioSource.outputAudioMixerGroup.audioMixer;
    private float minHz => hzSettings.x;
    private float maxHz => hzSettings.y;
    private float distanceFromNormal => Mathf.Abs(balance - normalSoundValue);

    /** Private variables */
    private float marginError = 0.1f;
    private AudioSource audioSource;
    private float hz;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        SetRandomNormalSoundValue();
        MidiBinding.Instance.Subscribe(MidiBind.GAIN_POT_1, OnMidiValue);
    }

    public void SetRandomNormalSoundValue()
    {
        normalSoundValue = Random.Range(0f, 1f);
    }

    void Update()
    {
        UpdateHzValue();
        UpdateLowPass();
        UpdateDisplay();
    }

    void UpdateHzValue()
    {
        if (distanceFromNormal > marginError)
        {
            hz = GetHzFromValue(distanceFromNormal);
        }
        else
        {
            // Dans la zone normale : pas d'effet
            hz = maxHz;
        }
    }

    void UpdateLowPass()
    {
        if (soundEffectEnabled)
        {
            mixer.SetFloat(lowPassVariableName, hz);
        }
        else
        {
            mixer.SetFloat(lowPassVariableName, conversationHzSettings);
        }
    }

    void UpdateDisplay()
    {
        if (soundEffectEnabled)
        {
            float balanceHz = GetHzFromValue(balance);
            float normalSoundValueHz = GetHzFromValue(normalSoundValue);
            UISoundFrequency.Instance.Show(gameObject);
            
            if (distanceFromNormal > marginError) UISoundFrequency.Instance.HandleUI(gameObject, balanceHz, normalSoundValueHz);
            else UISoundFrequency.Instance.HandleUI(gameObject, normalSoundValueHz, normalSoundValueHz);
        }
        else
        {
            UISoundFrequency.Instance.Hide(gameObject);
        }
    }
    
    void OnMidiValue(float value)
    {
        balance = value;
    }

    public void EnableSoundEffect()
    {
        if (soundEffectEnabled) return;
        soundEffectEnabled = true;
        SetRandomNormalSoundValue();

        mixer.SetFloat("MusicLowPass", musicHzSettings);
        mixer.SetFloat("CrowdLowPass", crowdHzSettings);
    }

    public void DisableSoundEffect()
    {
        if (!soundEffectEnabled) return;
        soundEffectEnabled = false;

        mixer.SetFloat("MusicLowPass", maxHz);
        mixer.SetFloat("CrowdLowPass", crowdHzSettings);
    }

    float GetHzFromValue(float value)
    {
        value = Mathf.Max(0f, value);
        value = Mathf.Min(1f, value);
        float t = Mathf.Clamp01(1f - value);
        float curvedT = Mathf.Pow(t, exponent);
        return Mathf.Lerp(minHz, maxHz, curvedT);
    }

}