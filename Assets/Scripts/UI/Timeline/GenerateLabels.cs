using TMPro;
using UnityEngine;

public class GenerateLabels : MonoBehaviour
{
    [SerializeField] private string startHourText;
    [SerializeField] private string endHourText;
    [SerializeField] private string startMinuteText;
    [SerializeField] private string endMinuteText;
    [SerializeField] private TMP_FontAsset font;

    void Start()
    {
        GenerateAll();
    }

    void GenerateAll()
    {
        Generate("StartHourLabel", startHourText, Anchors.TopLeft);
        Generate("EndHourLabel", endHourText, Anchors.TopRight);
        Generate("StartMinuteLabel", startMinuteText, Anchors.BottomLeft);
        Generate("EndMinuteLabel", endMinuteText, Anchors.BottomRight);
    }

    void Generate(string name, string text, Anchors.Anchor anchor)
    {
        // Crée l'objet texte
        GameObject textGO = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));

        // Parentage immédiat
        textGO.transform.SetParent(transform, false);

        // Récupérer les composants
        RectTransform rectTransform = textGO.GetComponent<RectTransform>();
        TextMeshProUGUI textComponent = textGO.GetComponent<TextMeshProUGUI>();

        // Paramétrage du RectTransform
        rectTransform.anchorMin = anchor.min;
        rectTransform.anchorMax = anchor.max;
        rectTransform.pivot = Anchors.defaultPivot;
        rectTransform.anchoredPosition = new Vector2(0, 0); // position selon besoin
        rectTransform.sizeDelta = new Vector2(160, 30);     // fixe une taille

        // Texte
        textComponent.text = text;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 20;
        textComponent.color = Color.white;
        textComponent.font = font;
    }
}
