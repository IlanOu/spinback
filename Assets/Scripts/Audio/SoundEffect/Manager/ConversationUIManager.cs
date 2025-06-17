using UI.Cursor;
using UnityEngine;

public class ConversationUIManager : MonoBehaviour
{
    [SerializeField] private UISoundFrequency ui;
    [SerializeField] private PointCursorManager pointCursorManager;
    
    public void Handle(float distance, bool conversationEffectEnabled)
    {
        if (conversationEffectEnabled)
        {
            ui.Show();
            ui.HandleUI(distance);
        }
        else
        {
            ui.Hide();
        }
        pointCursorManager.ToggleFocus(conversationEffectEnabled);
    }
}
