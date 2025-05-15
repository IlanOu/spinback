using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VibratingLine : MonoBehaviour
{
    [SerializeField] private int points = 100;
    [SerializeField] private float width = 20f;
    [SerializeField] private float maxAmplitude = 2f;
    [SerializeField] private float speed = 5f;
    [SerializeField, Range(0, 100)] private int fixedPointsPercent = 1;

    [SerializeField] private float vibratingValueA = 4f;

    private LineRenderer line;
    private float offsetTime;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = points;
        offsetTime = Random.Range(0, 1000);
    }

    void Update()
    {
        float distance = Mathf.Abs(vibratingValueA);
        float vibration = Mathf.Clamp01(distance); // 0 à 1

        Vector3[] positions = new Vector3[points];
        float step = width / (points - 1);

        for (int i = 0; i < points; i++)
        {
            float x = -width / 2f + i * step;
            float y = 0f;

            // Laisser les bords fixes
            float t = i / (float)(points - 1); // de 0 à 1
            float percentValue = fixedPointsPercent/100;
            if (t > percentValue && t < 1 - percentValue)
            {
                float noise = Mathf.PerlinNoise(i * 0.1f, (Time.time + offsetTime) * speed);
                float offset = (noise - 0.5f) * 2f * maxAmplitude * vibration;
                y = offset;
            }

            positions[i] = new Vector3(x, y, 0f);
        }

        line.SetPositions(positions);
    }
}