using Object.InteractableState;
using UnityEngine;

namespace Object
{
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] private DetectableGameObject detectableGameObject;
        [SerializeField] public float outlineSize = 15f;
        [SerializeField] public GameObject label3D;
        [SerializeField, Range(0f, 1f)] private float zoomValue = 0.3f;
        [HideInInspector] public IInteractableState currentState;
        [HideInInspector] public Material material;
        [HideInInspector] public Outline outline;
        private CameraZoom cameraZoom;
        public bool IsLookingAt => detectableGameObject.isLookingAt;
        public bool alwaysShowLabel = false;

        public bool IsFocused => IsLookingAt && IsZooming();

        void Awake()
        {
            material = GetComponent<Renderer>().material;
            outline = GetComponent<Outline>();
            currentState = new OutlineInteractableState(this);
        }
    
        void Start()
        {
            cameraZoom = Camera.main.GetComponent<CameraZoom>();

            CameraZoomSettings settings = GlobalCameraSettings.Instance.GetSettings<CameraZoomSettings>(ObjectType.Object);
            zoomValue = settings.zoomValue;
        }

        void Update()
        {
            currentState.Handle();
        }

        public bool IsZooming()
        {
            if (cameraZoom == null) return false;
            return cameraZoom.IsZooming(zoomValue);
        }

        public void UpdateState(IInteractableState newState)
        {
            currentState = newState;
        }
    }
}
