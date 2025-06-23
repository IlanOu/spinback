using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class ConfirmationPopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private GameObject mainVerticalLayout;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private GameObject buttonsVerticalLayout;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI cancelButtonText;
        [SerializeField] private Button closeButton;

        private Action onConfirm;
        private Action onCancel;
        private bool isBlocking = true;

        private CursorCommandToken cursorToken;
        private PauseCommandToken pauseToken;
        
        public enum PopupType
        {
            OkOnly, // Un seul bouton "OK"
            YesNo, // Deux boutons "Oui" et "Non"
            ConfirmCancel, // Deux boutons "Confirmer" et "Annuler"
            Declaration, // Deux boutons "Signer ma déclaration" et "Revenir en jeu"
            Custom // Textes personnalisés pour les boutons
        }

        public enum PopupMode
        {
            Blocking, // L'utilisateur doit interagir avec la popup
            NonBlocking // L'utilisateur peut ignorer la popup
        }

        private void Awake()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCancelClicked);
        }

        public void Show(string message, PopupType type, PopupMode mode,
            Action onConfirmAction = null, Action onCancelAction = null,
            string customConfirmText = "", string customCancelText = "")
        {
            // Configurer le message
            if (messageText != null) messageText.text = message;

            // Configurer les callbacks
            onConfirm = onConfirmAction;
            onCancel = onCancelAction;

            // Configurer le mode
            isBlocking = (mode == PopupMode.Blocking);
            if (closeButton != null)
                closeButton.gameObject.SetActive(!isBlocking);

            // Configurer les boutons selon le type
            SetupButtons(type, customConfirmText, customCancelText);

            // Activer les layouts
            if (mainVerticalLayout != null)
                mainVerticalLayout.SetActive(true);

            // Afficher la popup
            if (popupPanel != null)
                popupPanel.SetActive(true);
            
            pauseToken = PauseManager.Instance.AddPauseCommand("ConfirmationPopup", 100);
            cursorToken = CursorManager.Instance.AddCommand("ConfirmationPopup", true, 100);
        }

        private void SetupButtons(PopupType type, string customConfirmText, string customCancelText)
        {
            switch (type)
            {
                case PopupType.OkOnly:
                    if (!confirmButtonText != null) confirmButtonText.text = "OK";
                    if (!cancelButton != null) cancelButton.gameObject.SetActive(false);
                    break;

                case PopupType.YesNo:
                    if (confirmButtonText != null) confirmButtonText.text = "Oui";
                    if (cancelButtonText != null) cancelButtonText.text = "Non";
                    if (cancelButton != null) cancelButton.gameObject.SetActive(true);
                    break;

                case PopupType.ConfirmCancel:
                    if (confirmButtonText != null) confirmButtonText.text = "Confirmer";
                    if (cancelButtonText != null) cancelButtonText.text = "Annuler";
                    if (cancelButton != null) cancelButton.gameObject.SetActive(true);
                    break;
                
                case PopupType.Declaration:
                    if (confirmButtonText != null) confirmButtonText.text = "Valider mon témoignage";
                    if (cancelButtonText != null) cancelButtonText.text = "Revenir en jeu";
                    if (cancelButton != null) cancelButton.gameObject.SetActive(true);
                    break;

                case PopupType.Custom:
                    if (confirmButtonText != null)
                        confirmButtonText.text =
                            !string.IsNullOrEmpty(customConfirmText) ? customConfirmText : "Confirmer";
                    if (cancelButtonText != null)
                        cancelButtonText.text = !string.IsNullOrEmpty(customCancelText) ? customCancelText : "Annuler";
                    if (cancelButton != null) cancelButton.gameObject.SetActive(true);
                    break;
            }

            // S'assurer que le layout des boutons est actif
            if (buttonsVerticalLayout != null)
                buttonsVerticalLayout.SetActive(true);
        }

        private void OnConfirmClicked()
        {
            Hide();
            onConfirm?.Invoke();
        }

        private void OnCancelClicked()
        {
            Hide();
            onCancel?.Invoke();
        }

        private void Hide()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);
            
            pauseToken?.Dispose();
            pauseToken = null;
            
            cursorToken?.Dispose();
            cursorToken = null;
        }
        
        private void OnDestroy()
        {
            pauseToken?.Dispose();
            cursorToken?.Dispose();
        }
    }
}