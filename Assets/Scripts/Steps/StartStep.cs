using System.Collections;
using Cinematics;
using UnityEngine;

namespace Steps
{
    public class StartStep: MonoBehaviour
    {
        [SerializeField] private string nextSceneName;
        
        void Start()
        {
            StartCoroutine(DelayedTransition());
        }

        private IEnumerator DelayedTransition()
        {
            yield return new WaitUntil(() => SceneTransitionBlinker.Instance != null);

            SceneTransitionBlinker.Instance.TransitionToScene(nextSceneName);
        }

    }
}