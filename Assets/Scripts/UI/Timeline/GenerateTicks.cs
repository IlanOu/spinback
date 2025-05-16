using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class GenerateTicks : MonoBehaviour
{
    [SerializeField] private GameObject longTickPrefab;
    [SerializeField] private GameObject mediumTickPrefab;
    [SerializeField] private GameObject smallTickPrefab;
    [SerializeField] private float timeLength = 120f;
    [SerializeField] private int ticksNumber = 12;
    private RectTransform rectTransform;

    public float width
    {
        get
        {
            Canvas.ForceUpdateCanvases();
        
            // Utiliser directement la largeur calculée
            Rect rect = rectTransform.rect;
            return rect.size.x;
        }
    }



    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (ticksNumber % 2 != 0) {
            Debug.LogError("Le nombre de ticks doit etre pair.");
            enabled = false;
        }
    }

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        float x = 0;

        // Generation des long tick sur les bords
        GameObject firstLongTick = Instantiate(longTickPrefab, transform);
        RectTransform firstLongTickTransform = firstLongTick.GetComponent<RectTransform>();
        firstLongTickTransform.anchoredPosition = new Vector3(0, 0, 0);

        GameObject lastLongTick = Instantiate(longTickPrefab, transform);
        RectTransform lastLongTickTransform = lastLongTick.GetComponent<RectTransform>();
        lastLongTickTransform.anchoredPosition = new Vector3(width, 0, 0);

        // Génération des autres ticks
        for (int i = 0 ; i < ticksNumber; i++)
        {
            // Invertir l'ordre des ticks en commencant par les petits
            GameObject tickPrefab = i % 2 == 0 ? smallTickPrefab : mediumTickPrefab;

            x += width / ticksNumber;
            GameObject tick = Instantiate(tickPrefab, transform);
            RectTransform tickTransform = tick.GetComponent<RectTransform>();
            tickTransform.anchoredPosition = new Vector3(x, 0, 0);
        }
    }
}
