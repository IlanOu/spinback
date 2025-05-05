using DefaultNamespace;
using Minis;
using UnityEngine;
using UnityEngine.Audio;

public class ConversationSoundEffectController : MonoBehaviour
{
    public AudioMixer mixer;

    [Range(0f, 1f), Header("Valeur contrôlée entre 10 et 22000 Hz")]
    public float balance = 0f;

    [SerializeField, Range(0f, 1f), Header("Valeur à trouver pour avoir un son normal")]
    private float normalSoundValue = 0.5f;

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
    private float minHz => hzSettings.x;
    private float maxHz => hzSettings.y;
    private float marginError = 0.1f;


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
        if (soundEffectEnabled)
        {
            float hz;

            // Calcul de l'écart absolu par rapport à la zone normale
            float distanceFromNormal = Mathf.Abs(balance - normalSoundValue);

            if (distanceFromNormal > marginError)
            {
                // Normalisation inversée
                float t = Mathf.Clamp01(1f - distanceFromNormal);

                // Application d'une courbe exponentielle
                float curvedT = Mathf.Pow(t, exponent);

                // Interpolation avec sensibilité forte près de normalSoundValue
                hz = Mathf.Lerp(minHz, maxHz, curvedT);
            }
            else
            {
                // Dans la zone normale : pas d'effet
                hz = maxHz;
            }

            mixer.SetFloat("MusicLowPass", musicHzSettings);
            mixer.SetFloat("CrowdLowPass", crowdHzSettings);
            mixer.SetFloat("ConversationLowPass", hz);
        }
        else
        {
            mixer.SetFloat("MusicLowPass", maxHz);
            mixer.SetFloat("CrowdLowPass", maxHz);
            mixer.SetFloat("ConversationLowPass", conversationHzSettings);
        }
    }
    
    void OnMidiValue(MidiInput input)
    {
        balance = input.Value * (1f / 127f) - 1f;
    }

}