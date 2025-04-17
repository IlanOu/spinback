using UnityEngine;
using UnityEngine.Audio;

public class WetDryBalanceController : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [Range(-1f, 1f)]
    [SerializeField] float wetDryBalance = 0f;

    void Update()
    {
        // Transforme -1..1 en 0..1
        float wetAmount = Mathf.Clamp01((wetDryBalance + 1f) / 2f);
        float dryAmount = 1f - wetAmount;

        float dryDb = Mathf.Lerp(-80f, 0f, dryAmount);
        float wetDb = Mathf.Lerp(-80f, 0f, wetAmount);

        mixer.SetFloat("DryLevel", dryDb);
        mixer.SetFloat("WetLevel", wetDb);
    }
}
