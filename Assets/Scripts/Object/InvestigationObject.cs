using System;
using DefaultNamespace;
using Minis;
using Nenn.InspectorEnhancements.Runtime.Attributes;
using Object.InvestigationReport;
using TMPro;
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
            MidiBindingRegistry.Instance.Bind(ActionEnum.InvestigationReport, (input) => OnControllerButtonPressed());
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