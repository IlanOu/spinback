using UnityEngine;
using UnityEngine.Audio;

public class DryWetController : MonoBehaviour
{
    public AudioMixer mixer;

    [Range(-1f, 1f)]
    public float balance = 0f; // -1 = dry+, 0 = normal, +1 = wet+

    float maxValue = 2000f;

    void Update()
    {
        float dryDb;
        float wetDb;

        if (balance < 0f)
        {
            // Accentuer le dry : WetLevel reste à 0dB, on baisse DryLevel vers -80
            wetDb = 0f;
            dryDb = Mathf.Lerp(0f, -maxValue, -balance); // balance va de 0 à -1
        }
        else
        {
            // Accentuer le wet : DryLevel reste à 0dB, on baisse WetLevel vers -80
            dryDb = 0f;
            wetDb = Mathf.Lerp(0f, maxValue, balance); // balance va de 0 à +1
        }

        mixer.SetFloat("DryLevel", dryDb);
        mixer.SetFloat("WetLevel", wetDb);
    }
}
