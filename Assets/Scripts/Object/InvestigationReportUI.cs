using DefaultNamespace;
using Minis;
using UnityEngine;

public class InvestigationReportUI : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private GameObject investigationReportUI;

    private bool isShowing = false;

    void Start()
    {
        MidiBindingRegistry.Instance.Bind(ActionEnum.InvestigationReport, (input) => OnControllerButtonPressed());
    }

    private void Update()
    {
        // Si la touche I est pressée, simuler l'appui sur le bouton du controller
        if (Input.GetKeyDown(KeyCode.I))
        {
            OnControllerButtonPressed();
        }
        
        // Vérifier si l'UI est affichée mais que l'objet n'est plus regardé
        if (isShowing && !detectableGameObject.isLookingAt)
        {
            HideUI();
        }
    }

    void ShowUI()
    {
        investigationReportUI.SetActive(true);
        isShowing = true;
    }

    void HideUI()
    {
        investigationReportUI.SetActive(false);
        isShowing = false;
    }

    void OnControllerButtonPressed()
    {
        if (detectableGameObject.isLookingAt && !isShowing)
        {
            ShowUI();
        }
        else if (isShowing)
        {
            HideUI();
        }
    }

    // Pour maintenir la compatibilité avec le code existant
    void OnClicked()
    {
        OnControllerButtonPressed();
    }
}