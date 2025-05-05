using DefaultNamespace;
using Minis;
using UnityEngine;
using UnityEngine.Audio;

public class ConversationSoundEffectController : MonoBehaviour
{
    /** Serialized variables */
    [SerializeField] private AudioSource audioSource;
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
    private float marginError = 0.1f;

    /** Private variables */
    private float hz;


    void Start()
    {
        SetRandomNormalSoundValue();   
        MidiBindingRegistry.Instance.Bind(ActionEnum.DryWetController, OnMidiValue);
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
        float distanceFromNormal = Mathf.Abs(balance - normalSoundValue);

        if (distanceFromNormal > marginError)
        {
            float t = Mathf.Clamp01(1f - distanceFromNormal);
            float curvedT = Mathf.Pow(t, exponent);
            hz = Mathf.Lerp(minHz, maxHz, curvedT);
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
            mixer.SetFloat("MusicLowPass", musicHzSettings);
            mixer.SetFloat("CrowdLowPass", crowdHzSettings);
            mixer.SetFloat(lowPassVariableName, hz);
        }
        else
        {
            mixer.SetFloat("MusicLowPass", maxHz);
            mixer.SetFloat("CrowdLowPass", crowdHzSettings);
            mixer.SetFloat(lowPassVariableName, conversationHzSettings);
        }
    }

    void UpdateDisplay()
    {
        if (soundEffectEnabled)
        {
            UISoundFrequency.Instance.Show(gameObject);
            UISoundFrequency.Instance.HandleUI(gameObject, hz);
        }
        else
        {
            UISoundFrequency.Instance.Hide(gameObject);
        }
    }
    
    void OnMidiValue(MidiInput input)
    {
        balance = input.Value * (1f / 127f) - 1f;
    }

    public void EnableSoundEffect()
    {
        if (soundEffectEnabled) return;
        soundEffectEnabled = true;
        SetRandomNormalSoundValue();
    }

    public void DisableSoundEffect()
    {
        if (!soundEffectEnabled) return;
        soundEffectEnabled = false;
    }

}