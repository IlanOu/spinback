using UI.Popup;
using UnityEngine;

namespace Game
{
    public class QuitApplication : MonoBehaviour
    {
        // Référence à votre popup
        [SerializeField] private ConfirmationPopup confirmationPopup;

        // Exemple d'utilisation pour quitter l'application
        public void ExitGame()
        {
            confirmationPopup.Show(
                "Quitter l'application",
                ConfirmationPopup.PopupType.YesNo,
                ConfirmationPopup.PopupMode.Blocking,
                () =>
                {
                    // Action à exécuter si l'utilisateur confirme
                    Debug.Log("Quitter confirmé");

                    // Utiliser le script QuitApplication
                    InstantExitGame();
                },
                () =>
                {
                    // Action à exécuter si l'utilisateur annule
                    Debug.Log("Quitter annulé");
                }
            );
        }


        public void InstantExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

            Debug.Log("Application quittée");
        }
    }
}