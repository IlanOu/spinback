using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Cinematics
{
    public class SceneTransitionBlinker : MonoBehaviour
    {
        public static SceneTransitionBlinker Instance;

        [Header("Panels qui forment les paupières")]
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private RectTransform bottomPanel;

        [Header("Durée du clin d'œil")]
        [SerializeField] private float blinkDuration = 0.4f;

        [Header("Courbe d'animation")]
        [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector2 topClosedPos;
        private Vector2 bottomClosedPos;
        private Vector2 topOpenPos;
        private Vector2 bottomOpenPos;

        private void Awake()
        {
            // Singleton simple
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitPanelPositions();
        }

        private void InitPanelPositions()
        {
            float height = topPanel.rect.height;
            topOpenPos = Vector2.zero;
            bottomOpenPos = Vector2.zero;
            topClosedPos = new Vector2(0, -height);
            bottomClosedPos = new Vector2(0, height);
            
            // On démarre avec les "yeux ouverts"
            topPanel.anchoredPosition = topOpenPos;
            bottomPanel.anchoredPosition = bottomOpenPos;
        }

        /// <summary>
        /// Amène les panels au sommet de la hiérarchie pour qu’ils soient visibles devant tout.
        /// </summary>
        private void BringToFront()
        {
            topPanel.transform.SetAsLastSibling();
            bottomPanel.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Transition complète : blink (fermeture) → chargement → blink (ouverture)
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            StartCoroutine(BlinkThenLoad(sceneName));
        }

        private IEnumerator BlinkThenLoad(string sceneName)
        {
            // Fermeture
            yield return StartCoroutine(Blink(close: true));

            // Chargement de la nouvelle scène
            yield return SceneManager.LoadSceneAsync(sceneName);
            // Attend la fin de la frame pour s'assurer que la nouvelle scène est active
            yield return new WaitForEndOfFrame();

            // Force les panels à être sur le dessus
            BringToFront();
            
            // Ouverture
            yield return StartCoroutine(Blink(close: false));
        }

        /// <summary>
        /// Effectue un blink complet en deux étapes, avec une action intermédiaire.
        /// </summary>
        public IEnumerator BlinkAndDo(System.Func<IEnumerator> actionToRunMidBlink)
        {
            // Fermeture
            yield return StartCoroutine(Blink(true));
            Debug.Log("Fermeture des paupières");
            
            // Exécute l'action centrale pendant l'écran noir
            if (actionToRunMidBlink != null)
                yield return StartCoroutine(actionToRunMidBlink());

            // Attend une frame pour laisser le temps de mettre à jour la scène
            yield return new WaitForEndOfFrame();
            BringToFront();

            // Ouverture
            Debug.Log("Ouverture des paupières");
            yield return StartCoroutine(Blink(false));
        }

        /// <summary>
        /// Exécute le blink : si close = true, ferme les paupières, sinon les ouvre.
        /// </summary>
        public IEnumerator Blink(bool close)
        {
            float timer = 0f;
            while (timer < blinkDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / blinkDuration);
                float eval = blinkCurve.Evaluate(t);
                if (close)
                {
                    topPanel.anchoredPosition = Vector2.Lerp(topOpenPos, topClosedPos, eval);
                    bottomPanel.anchoredPosition = Vector2.Lerp(bottomOpenPos, bottomClosedPos, eval);
                }
                else
                {
                    topPanel.anchoredPosition = Vector2.Lerp(topClosedPos, topOpenPos, eval);
                    bottomPanel.anchoredPosition = Vector2.Lerp(bottomClosedPos, bottomOpenPos, eval);
                }
                yield return null;
            }
            // Assure la position finale exacte
            topPanel.anchoredPosition = close ? topClosedPos : topOpenPos;
            bottomPanel.anchoredPosition = close ? bottomClosedPos : bottomOpenPos;
        }
    }
}
