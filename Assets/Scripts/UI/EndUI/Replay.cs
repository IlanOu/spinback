using Cinematics;
using UnityEngine;

namespace UI.EndUI
{
    public class Replay: MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "Start";
        public void ReplayGame()
        {
            SceneTransitionBlinker.Instance.TransitionToSceneWithVideo(mainMenuSceneName);
        }
    }
}