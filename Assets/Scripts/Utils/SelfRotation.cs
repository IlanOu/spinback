using UnityEngine;
namespace Utils
{

    public class SelfRotation : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private float speed = 30f;
        
        [Header("Rotation Axis")]
        [SerializeField] private bool rotateX = false;
        [SerializeField] private bool rotateY = true;
        [SerializeField] private bool rotateZ = false;
        
        [Header("Custom Axis (Optional)")]
        [SerializeField] private bool useCustomAxis = false;
        [SerializeField] private Vector3 customAxis = Vector3.up;
        
        private Vector3 rotationAxis;

        private void Start()
        {
            UpdateRotationAxis();
        }

        public void Update()
        {
            if (useCustomAxis)
            {
                // Utiliser l'axe personnalisé
                transform.Rotate(customAxis.normalized, speed * Time.deltaTime);
            }
            else
            {
                // Utiliser l'axe calculé à partir des booléens
                transform.Rotate(rotationAxis, speed * Time.deltaTime);
            }
        }
        
        // Mettre à jour l'axe de rotation en fonction des paramètres
        private void UpdateRotationAxis()
        {
            rotationAxis = new Vector3(
                rotateX ? 1 : 0,
                rotateY ? 1 : 0,
                rotateZ ? 1 : 0
            ).normalized;
            
            // Si aucun axe n'est sélectionné, utiliser l'axe Y par défaut
            if (rotationAxis == Vector3.zero)
            {
                rotationAxis = Vector3.up;
            }
        }
        
        // Méthodes publiques pour changer les paramètres en cours d'exécution
        
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
        
        public void SetRotationAxis(bool x, bool y, bool z)
        {
            rotateX = x;
            rotateY = y;
            rotateZ = z;
            UpdateRotationAxis();
        }
        
        public void SetCustomAxis(Vector3 axis)
        {
            customAxis = axis;
        }
        
        public void UseCustomAxis(bool use)
        {
            useCustomAxis = use;
        }
        
        // Pour l'inspecteur Unity - permet de mettre à jour l'axe quand les valeurs sont modifiées
        private void OnValidate()
        {
            UpdateRotationAxis();
        }
    }

}