using UnityEngine;
using UnityEngine.Audio;

public class ReverbSendControl : MonoBehaviour
{
    public AudioMixer mixer;

    // 0 = dry, 1 = full wet
    [Range(0f, 1f)]
    public float wetAmount = 0f;

    void Update()
    {
        // -80 dB = muet, 0 dB = plein volume
        float dB = Mathf.Lerp(-80f, 0f, wetAmount);
        mixer.SetFloat("ReverbSend", dB);
    }
}
