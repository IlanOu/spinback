using Nenn.InspectorEnhancements.Runtime.Attributes;
using Object.InvestigationReport;
using UnityEngine;

namespace Object
{
    public class InvestigationObject:MonoBehaviour
    {
        [SerializeField] private InteractableObject interactable;
        
        [Header("UI")]
        [SerializeField] private InvestigationReportUI investigationReportUI;
        [Required] public InvestigationData description;
        
        void Start()
        {
                MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_1, (input) => OnControllerButtonPressed());
            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_2, (input) => OnControllerButtonPressed());
            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_1, (input) => OnControllerButtonPressed());
            MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_2, (input) => OnControllerButtonPressed());
        }

        private void Update()
        {
            // Si la touche E est pressée, simuler l'appui sur le bouton du controller
            if (Input.GetKeyDown(KeyCode.I))
            {
                OnControllerButtonPressed();
            }
        }

        private void OnControllerButtonPressed()
        {
            if (interactable.IsFocused)
            {
                investigationReportUI.AddInfoToReport(description);
            }
        }
    }
}