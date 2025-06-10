using UnityEngine;
using UnityEngine.UI;

namespace UI.Cursor
{
    public class PointCursorManager: MonoBehaviour
    {
        [SerializeField] private Texture2D cursorTexture;
        [SerializeField] private Image cursorImage;

        private void Start()
        {
            cursorImage.sprite = Sprite.Create(cursorTexture, new Rect(0, 0, cursorTexture.width, cursorTexture.height), Vector2.zero);
        }
        
        public void DisplayCursor(bool display)
        {
            cursorImage.enabled = display;
        }
    }
}