using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

[RequireComponent(typeof(RawImage))]
public class VerticalSinusRenderer : MonoBehaviour
{
    [Header("Sinus Settings")]
    [SerializeField] private float amplitude = 40f;
    [SerializeField] private float frequency = 2f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private Color lineColor = Color.green;

    [Header("Range Settings")]
    [SerializeField] private Vector2 amplitudeRange = new Vector2(20f, 60f);
    [SerializeField] private Vector2 frequencyRange = new Vector2(1f, 4f);
    [SerializeField] private Vector2 speedRange = new Vector2(0.5f, 3f);

    [Header("Timeline")]
    [SerializeField] private PlayableDirector timeline;

    private Texture2D texture;
    private RawImage rawImage;
    private float height;
    private float width;
    private float maxAmplitude;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        RectTransform rectTransform = GetComponent<RectTransform>();
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;
        maxAmplitude = width / 2f;

        texture = new Texture2D((int)width, (int)height);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        rawImage.texture = texture;
        rawImage.color = lineColor;
    }

    void Update()
    {
        float time = (float)(timeline != null ? timeline.time : 0f);
        DrawSinus(time);
    }

    void DrawSinus(float time)
    {
        // Nettoyer tous les pixels
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }

        // Dessiner la sinusoïde
        for (int y = 0; y < height; y++)
        {
            float t = y / height;
            float phase = t * Mathf.PI * 2 * frequency + time * speed * Mathf.PI * 2;
            float x = width / 2f + Mathf.Sin(phase) * Mathf.Min(amplitude, maxAmplitude);
            int px = Mathf.Clamp(Mathf.RoundToInt(x), 0, (int)width - 1);
            texture.SetPixel(px, y, lineColor);
        }

        texture.Apply();
    }

    public void SetSinusProfile(float seed)
    {
        float scaledSeed = seed * 5f;

        float tempAmplitude = Mathf.PerlinNoise(scaledSeed + 0.1f, 0.1f);
        float tempFrequency = Mathf.PerlinNoise(scaledSeed + 0.5f, 0.5f);
        float tempSpeed = Mathf.PerlinNoise(scaledSeed + 0.9f, 0.9f);

        amplitude = Mathf.Lerp(amplitudeRange.x, amplitudeRange.y, tempAmplitude);
        frequency = Mathf.Lerp(frequencyRange.x, frequencyRange.y, tempFrequency);
        speed = Mathf.Lerp(speedRange.x, speedRange.y, tempSpeed);
    }
}