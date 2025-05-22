using UnityEngine;
using UnityEngine.Audio;

public class ConversationEffectManager : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;

    [Header("Settings")]
    [SerializeField] private Vector2 hzRange = new Vector2(200, 1000);
    [SerializeField] private float normalHz = 22000f;
    [SerializeField] private Vector2 resonanceSettings = new Vector2(2f, 1f);

    public void Handle(float distance, bool enabled)
    {
        if (enabled)
        {
            float hz = GetHz(distance);

            if (distance > 0)
            {
                audioMixer.SetFloat("ConversationsLowPass", hz);
                audioMixer.SetFloat("ConversationsResonance", resonanceSettings.x);
            }
            else
            {
                audioMixer.SetFloat("ConversationsLowPass", normalHz);
                audioMixer.SetFloat("ConversationsResonance", resonanceSettings.y);
            }
        }
        else
        {
            audioMixer.SetFloat("ConversationsLowPass", hzRange.x);
            audioMixer.SetFloat("ConversationsResonance", resonanceSettings.y);
        }
    }

    float GetHz(float distance)
    {
        float t = Mathf.Clamp01(1f - distance);
        float curvedT = (float)Easings.CircularEaseIn(t);
        return Mathf.Lerp(hzRange.x, hzRange.y, curvedT);
    }
}
