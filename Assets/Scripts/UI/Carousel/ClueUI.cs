using TMPro;
using UnityEngine;

public class ClueUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;

    public void SetClue(Clue clue)
    {
        title.text = clue.hours;
        description.text = clue.text;
    }
}
