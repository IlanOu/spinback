using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Cinematics
{
    public class CinematicSequencer : MonoBehaviour
    {
        [Tooltip("List of scene names to play in sequence")]
        public List<string> cinematicScenes = new List<string>();
    
        private int _currentSceneIndex = 0;
        private PlayableDirector _currentDirector;
    
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    
        void Start()
        {
            if (cinematicScenes.Count > 0)
            {
                LoadNextCinematic();
            }
            else
            {
                Debug.LogWarning("No cinematic scenes defined in the sequence!");
            }
        }
    
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Find the PlayableDirector in the new scene
            _currentDirector = FindAnyObjectByType<PlayableDirector>();
        
            if (_currentDirector != null)
            {
                // Register for the stopped event
                _currentDirector.stopped += OnCinematicCompleted;
            
                // Start the cinematic if it's not set to play automatically
                if (_currentDirector.state != PlayState.Playing)
                {
                    _currentDirector.Play();
                }
            }
            else
            {
                Debug.LogWarning("No PlayableDirector found in the scene: " + scene.name);
                // If there's no director, move to the next scene
                LoadNextCinematic();
            }
        }
    
        void OnCinematicCompleted(PlayableDirector director)
        {
            // Unregister from the event to prevent memory leaks
            if (_currentDirector != null)
            {
                _currentDirector.stopped -= OnCinematicCompleted;
            }
        
            // Load the next cinematic
            LoadNextCinematic();
        }
    
        public void LoadNextCinematic()
        {
            if (_currentSceneIndex < cinematicScenes.Count)
            {
                string nextScene = cinematicScenes[_currentSceneIndex];
                _currentSceneIndex++;
            
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                Debug.Log("All cinematics completed!");
                // Load final scene or menu
                // SceneManager.LoadScene("MainMenu");
            
                Destroy(gameObject);
            }
        }
    }
}