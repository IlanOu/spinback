using DefaultNamespace;
using Minis;
using UnityEngine;
using UnityEngine.Audio;

public class DryWetController : MonoBehaviour
{
    public AudioMixer mixer;

    [Header("Valeur contrôlée entre -1 (wet) et 0 (dry normal)")]
    [Range(-1f, 0f)]
    public float balance = 0f;

    [SerializeField, Range(-1f, 0f)]
    private float normalSoundValue = 0f;

    [Tooltip("Zone autour de la valeur normale où le son reste neutre (DryLevel = 0)")]
    [SerializeField]
    private float marginError = 0.1f;

    [Tooltip("Amplitude maximale de l'effet dry en dB (plus c'est grand, plus l'effet est fort)")]
    [SerializeField]
    private float maxDryDb = 2000f;

    void Start()
    {
        // Le son "normal" est défini dans la plage -1 à 0
        normalSoundValue = Random.Range(-1f, 0f);
        
        MidiBindingRegistry.Instance.Bind(ActionEnum.DryWetController, OnMidiValue);
    }

    void Update()
    {
        float dryDb = 0f;

        // Calcul de l'écart absolu par rapport à la zone normale
        float distanceFromNormal = Mathf.Abs(balance - normalSoundValue);

        if (distanceFromNormal > marginError)
        {
            // Hors de la zone normale : effet dry proportionnel à la distance
            float excess = distanceFromNormal - marginError;

            // Plus on s’éloigne, plus le dryDb est bas (effet plus fort)
            float t = Mathf.InverseLerp(0f, 1f - marginError, excess); // normalise sur [0,1]
            dryDb = Mathf.Lerp(0f, -maxDryDb, t);
        }
        else
        {
            // Dans la zone normale : pas d'effet
            dryDb = 0f;
        }

        mixer.SetFloat("DryLevel", dryDb);
    }
    
    void OnMidiValue(MidiInput input)
    {
        balance = input.Value * (1f / 127f) - 1f;
    }

}