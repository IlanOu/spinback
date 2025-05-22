using UnityEngine;

public class ConversationUIManager : MonoBehaviour
{
    [SerializeField] private UISoundFrequency ui;

    public void Handle(float distance, bool enabled)
    {
        if (enabled)
        {
            ui.Show();
            ui.HandleUI(distance);
        }
        else
        {
            ui.Hide();
        }
    }
}
