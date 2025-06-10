using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Cursor
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class MouseCursorManager : MonoBehaviour
    {
        [Header("Textures de curseur")]
        public Texture2D defaultCursor;
        public Vector2 defaultHotspot = Vector2.zero;
        public Texture2D pointerCursor;
        public Vector2 pointerHotspot = Vector2.zero;
        
        [Header("Taille fixe du curseur")]
        public Vector2 fixedCursorSize = new Vector2(32, 32);
        
        private GraphicRaycaster raycaster;
        private PointerEventData pointerData;
        private EventSystem eventSystem;
        private bool isHoveringButton = false;
        private Texture2D processedDefaultCursor;
        private Texture2D processedPointerCursor;

        void Awake()
        {
            raycaster = GetComponent<GraphicRaycaster>();
            eventSystem = EventSystem.current;
            pointerData = new PointerEventData(eventSystem);
            
            // Préparer les curseurs avec une taille fixe
            processedDefaultCursor = ResizeTexture(defaultCursor, fixedCursorSize);
            processedPointerCursor = ResizeTexture(pointerCursor, fixedCursorSize);
        }
        
        Texture2D ResizeTexture(Texture2D source, Vector2 size)
        {
            if (source == null) return null;
            
            int width = (int)size.x;
            int height = (int)size.y;
            
            Texture2D result = new Texture2D(width, height, source.format, false);
            
            // Préserver les paramètres importants
            result.filterMode = FilterMode.Bilinear;
            result.wrapMode = TextureWrapMode.Clamp;
            
            // Redimensionner
            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            
            return result;
        }

        void Start()
        {
            UnityEngine.Cursor.SetCursor(processedDefaultCursor, defaultHotspot, CursorMode.Auto);
        }

        void Update()
        {
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            bool overButton = false;
            foreach (var res in results)
                if (res.gameObject.GetComponentInParent<Selectable>() != null &&
                    res.gameObject.GetComponentInParent<Selectable>().IsInteractable())
                {
                    overButton = true;
                    break;
                }

            if (overButton && !isHoveringButton)
            {
                UnityEngine.Cursor.SetCursor(processedPointerCursor, pointerHotspot, CursorMode.Auto);
                isHoveringButton = true;
            }
            else if (!overButton && isHoveringButton)
            {
                UnityEngine.Cursor.SetCursor(processedDefaultCursor, defaultHotspot, CursorMode.Auto);
                isHoveringButton = false;
            }
        }
    }
}
