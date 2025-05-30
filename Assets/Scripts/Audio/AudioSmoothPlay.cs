using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSmoothPlay : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.time = 0;
        audioSource.volume = 0;
    }

    public void Play(float duration = 1f)
    {
        audioSource.Play();
        StartCoroutine(SmoothPlay(duration));
    }

    IEnumerator SmoothPlay(float duration = 1f)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = timer / duration;
            yield return null;
        }
        audioSource.volume = 1f;
    }
}
