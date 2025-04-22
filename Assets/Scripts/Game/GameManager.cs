using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Playables;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        // Singleton (facile à appeler depuis n'importe où)
        public static GameManager Instance { get; private set; }

        // L'état actuel du jeu
        public GameState CurrentState { get; private set; } = GameState.MenuMain;

        // Événements auxquels TU vas t'abonner (dans tes autres scripts)
        [Header("Événements du Game Flow")]
        public UnityEvent OnLoadingStarted;
        public UnityEvent OnCinematicStarted;
        public UnityEvent OnGameplayStarted;
        public UnityEvent OnGamePaused;
        public UnityEvent OnGameResumed;
        public UnityEvent OnGameOver;
        public UnityEvent OnVictory;
        public UnityEvent OnReturnToMainMenu;

        private void Awake()
        {
            // Initialisation du singleton
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject); // Persiste entre les scènes
            }
        }

        private void Start()
        {
            // Exemple : on démarre sur le menu principal
            ChangeState(GameState.Cinematic);
        }

        // La méthode magique : CHANGER D'ÉTAT
        public void ChangeState(GameState newState)
        {
            // Quitte l'état actuel
            if (CurrentState != GameState.MenuMain)
            {
                OnExitState(CurrentState);
            }

            // Sauvegarde le nouvel état
            CurrentState = newState;
            Debug.Log($"[GameManager] Changement d'état : {CurrentState}");

            // Entre dans le nouvel état
            OnEnterState(CurrentState);

            // (Facultatif) Appelle l'événement correspondant
            InvokeStateEvent(CurrentState);
        }

        // Que faire quand ON ENTRE dans un état ?
        private void OnEnterState(GameState state)
        {
            switch (state)
            {
                case GameState.Loading:
                    // Exemple : démarre un chargement de scène asynchrone
                    StartCoroutine(LoadSceneAsync("Level1"));
                    break;

                case GameState.Cinematic:
                    // Trouve la cinématique à jouer (ex: "IntroCinematic")
                    PlayableDirector cine = GameObject.Find("CinematicController").GetComponent<PlayableDirector>();
                    cine.Play(); // Joue la Timeline
                    break;

                case GameState.Gameplay:
                    // Active les contrôles joueur, les IA, la physique...
                    
                    Time.timeScale = 1f; // (si jamais on revient d'une pause)
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f; // Gèle le temps
                    ShowPauseMenu(); // Affiche le menu de pause
                    break;

                case GameState.MenuMain:
                    // Charge le menu principal (si ce n'est pas déjà fait)
                    if (SceneManager.GetActiveScene().name != "MenuScene")
                    {
                        SceneManager.LoadScene("MenuScene");
                    }
                    HideAllGameUI(); // Cache les éléments de jeu (barre de vie, etc.)
                    ShowMainMenuUI(); // Affiche les boutons "Jouer", "Options"...
                    break;

                case GameState.GameOver:
                    ShowGameOverScreen(); // Affiche l'écran "Game Over"
                    break;

                case GameState.Victory:
                    ShowVictoryScreen(); // Affiche "Félicitations, vous avez gagné !"
                    
                    break;
            }
        }

        // Que faire quand ON SORT d'un état ?
        private void OnExitState(GameState state)
        {
            switch (state)
            {
                case GameState.Cinematic:
                    // Assure-toi que la cinématique est stoppée (des fois utile)
                    GameObject.Find("CinematicController").GetComponent<PlayableDirector>().Stop();
                    break;

                case GameState.Gameplay:
                    // Désactive les contrôles (quand on met pause ou fin du jeu)
                    
                    break;

                case GameState.Paused:
                    Time.timeScale = 1f; // Dégèle le temps
                    HidePauseMenu(); // Cache le menu pause
                    break;

                case GameState.MenuMain:
                    // Prépare la scène de jeu (si on va jouer)
                    
                    break;
            }
        }

        // Appelle l'événement UNE FOIS qu'on est dans le nouvel état
        private void InvokeStateEvent(GameState state)
        {
            switch (state)
            {
                case GameState.Loading: OnLoadingStarted?.Invoke(); break;
                case GameState.Cinematic: OnCinematicStarted?.Invoke(); break;
                case GameState.Gameplay: OnGameplayStarted?.Invoke(); break;
                case GameState.Paused: OnGamePaused?.Invoke(); break;
                case GameState.GameOver: OnGameOver?.Invoke(); break;
                case GameState.Victory: OnVictory?.Invoke(); break;
                // Pour reprendre après pause : OnGameResumed dans OnEnterState(Gameplay)
            }
        }

        // ------------------- PETITS HELPERS -------------------
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                Debug.Log($"Chargement : {asyncLoad.progress * 100:F1}%");
                yield return null;
            }
            // Une fois chargé, on passe à l'état suivant (ex: Cinematic ou Gameplay)
            ChangeState(GameState.Cinematic); // ou Gameplay si pas de ciné
        }
        
        private void ShowPauseMenu() { /* UI Pause */ }
        private void HidePauseMenu() { /* Cache UI Pause */ }
        private void ShowMainMenuUI() { /* Boutons Jouer, Quitter... */ }
        private void HideAllGameUI() { /* Cache barre de vie, mini-map... */ }
        private void ShowGameOverScreen() { /* Ecran noir "Game Over" */ }
        private void ShowVictoryScreen() { /* Ecran de victoire */ }
    }
}