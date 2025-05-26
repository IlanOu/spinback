using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VignetteController : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private Vector2 vignetteSettings; // x = intensity, y = smoothness
    [SerializeField] private float fadeDuration = 0.5f;

    private Vignette vignette;
    private Vector2 originalVignette;
    private Coroutine currentFadeCoroutine;

    void Awake()
    {
        if (volume.profile.TryGet(out vignette))
        {
            originalVignette = new Vector2(
                vignette.intensity.value,
                vignette.smoothness.value
            );
        }
        else
        {
            Debug.LogWarning("Vignette not found in volume profile.");
        }
    }

    public void Enable()
    {
        if (vignette == null) return;
        StartFade(vignetteSettings);
    }

    public void Disable()
    {
        if (vignette == null) return;
        StartFade(originalVignette);
    }

    private void StartFade(Vector2 targetValues)
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeVignette(targetValues));
    }

    private IEnumerator FadeVignette(Vector2 target)
    {
        float elapsed = 0f;

        float startIntensity = vignette.intensity.value;
        float startSmoothness = vignette.smoothness.value;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            vignette.intensity.value = Mathf.Lerp(startIntensity, target.x, t);
            vignette.smoothness.value = Mathf.Lerp(startSmoothness, target.y, t);

            yield return null;
        }

        // Snap to target values at the end
        vignette.intensity.value = target.x;
        vignette.smoothness.value = target.y;
        currentFadeCoroutine = null;
    }
}