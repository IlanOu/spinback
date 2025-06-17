using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class PointCursorManager : MonoBehaviour
{
    [Header("Textures")]
    [SerializeField] Texture normalTex;
    [SerializeField] Texture focusTex;

    [Header("Tailles (pixels)")]
    [SerializeField] Vector2 normalSize = new(32, 32);
    [SerializeField] Vector2 focusSize  = new(48, 48);

    RawImage      img;
    RectTransform rt;

    void Awake()
    {
        img = GetComponent<RawImage>();
        rt  = img.rectTransform;

        ToggleFocus(false);                 // état par défaut
    }

    public void Show(bool show)      => img.enabled = show;

    public void ToggleFocus(bool on) // change texture + taille
    {
        img.texture     = on ? focusTex  : normalTex;
        rt.sizeDelta    = on ? focusSize : normalSize;
    }
}