using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Cursor
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class MouseCursorManager : MonoBehaviour
    {
        [Header("Textures de curseur")] public Texture2D defaultCursor;
        public Vector2 defaultHotspot = Vector2.zero;
        public Texture2D pointerCursor;
        public Vector2 pointerHotspot = Vector2.zero;

        private GraphicRaycaster raycaster;
        private PointerEventData pointerData;
        private EventSystem eventSystem;
        private bool isHoveringButton = false;

        void Awake()
        {
            raycaster = GetComponent<GraphicRaycaster>();
            eventSystem = EventSystem.current;
            pointerData = new PointerEventData(eventSystem);
        }

        void Start()
        {
            UnityEngine.Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
        }

        void Update()
        {
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            bool overButton = false;
            foreach (var res in results)
                if (res.gameObject.GetComponent<Button>() != null)
                {
                    overButton = true;
                    break;
                }

            if (overButton && !isHoveringButton)
            {
                UnityEngine.Cursor.SetCursor(pointerCursor, pointerHotspot, CursorMode.Auto);
                isHoveringButton = true;
            }
            else if (!overButton && isHoveringButton)
            {
                UnityEngine.Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
                isHoveringButton = false;
            }
        }
    }
}