using Minis;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class InvestigationReportUI : MonoBehaviour
{
    [SerializeField] private DetectableGameObject detectableGameObject;
    [SerializeField] private GameObject investigationReportUI;

    private bool isShowing = false;

    void Start()
    {
        MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_1, OnNote);
        MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_1, OnNote);
        MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_CUE_2, OnNote);
        MidiBinding.Instance.Subscribe(MidiBind.BUTTON_1_ROLL_2, OnNote);
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

    void OnNote(float value)
    {
        // Si le bouton est cliqué
        if (value > 0)
        {
            OnControllerButtonPressed();
        }
    }
}