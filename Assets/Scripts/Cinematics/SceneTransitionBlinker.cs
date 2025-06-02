using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ThreadPriority = System.Threading.ThreadPriority;

namespace Cinematics 
{
    public class SceneTransitionBlinker : MonoBehaviour 
    {
        public static SceneTransitionBlinker Instance;

        [Header("Panels qui forment les paupières")]
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private RectTransform bottomPanel;

        [Header("Vidéo de transition")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RectTransform videoPanel; // Panel contenant le VideoPlayer
        [SerializeField] private VideoClip transitionVideo; // Clip vidéo à jouer

        [Header("Durée du clin d'œil")]
        [SerializeField] private float blinkDuration = 0.4f;

        [Header("Courbe d'animation")]
        [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Audio")]
        [SerializeField] private float audioFadeDuration = 0.5f;
        [SerializeField] private string masterMixerName = "Master"; // Nom du groupe de mixage principal
        [SerializeField] private string volumeParameterName = "MasterVolume"; // Nom du paramètre de volume

        [Header("Optimisation")]
        [SerializeField] private bool loadSceneDuringVideo = true;
        [SerializeField] private float preloadDelay = 0.5f; // Délai avant de commencer le chargement
        [SerializeField] private ThreadPriority loadingPriority = ThreadPriority.BelowNormal; // Priorité du thread de chargement
        [SerializeField] private float maxLoadingTimePerFrame = 0.01f; // Temps max de chargement par frame (en secondes)
        [SerializeField] private bool waitForVideoToFinish = true; // Attendre la fin de la vidéo avant d'activer la scène

        private Vector2 topClosedPos;
        private Vector2 bottomClosedPos;
        private Vector2 topOpenPos;
        private Vector2 bottomOpenPos;
        
        // Dictionnaire pour stocker les sources audio et leurs volumes originaux
        private Dictionary<AudioSource, float> _activeAudioSources = new Dictionary<AudioSource, float>();
        
        // Référence au mixer trouvé dynamiquement
        private AudioMixer _masterMixer;
        private float _originalMixerVolume = 0f;
        
        // Pour le chargement asynchrone
        private AsyncOperation _sceneLoadOperation;
        private bool _isLoadingScene = false;

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
            InitVideoPlayer();
            
            // S'abonner à l'événement de chargement de scène
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // Se désabonner de l'événement
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Réinitialiser les sources audio pour la nouvelle scène
            _activeAudioSources.Clear();
            _isLoadingScene = false;
            
            // Essayer de trouver le mixer audio dans la nouvelle scène
            FindAudioMixer();
        }
        
        private void FindAudioMixer()
        {
            // Chercher tous les AudioListener dans la scène
            AudioListener[] listeners = FindObjectsOfType<AudioListener>();
            
            if (listeners.Length > 0)
            {
                // Essayer de trouver un AudioMixerGroup sur le même GameObject ou ses parents
                foreach (AudioListener listener in listeners)
                {
                    AudioSource source = listener.GetComponent<AudioSource>();
                    if (source != null && source.outputAudioMixerGroup != null)
                    {
                        _masterMixer = source.outputAudioMixerGroup.audioMixer;
                        if (_masterMixer != null)
                        {
                            // Stocker la valeur de volume originale
                            _masterMixer.GetFloat(volumeParameterName, out _originalMixerVolume);
                            return;
                        }
                    }
                }
            }
            
            // Si on n'a pas trouvé de mixer via l'AudioListener, chercher dans toutes les sources audio
            AudioSource[] sources = FindObjectsOfType<AudioSource>();
            foreach (AudioSource source in sources)
            {
                if (source.outputAudioMixerGroup != null)
                {
                    _masterMixer = source.outputAudioMixerGroup.audioMixer;
                    if (_masterMixer != null)
                    {
                        // Stocker la valeur de volume originale
                        _masterMixer.GetFloat(volumeParameterName, out _originalMixerVolume);
                        return;
                    }
                }
            }
            
            // Si on n'a toujours pas trouvé, chercher tous les mixers dans les Resources
            AudioMixer[] mixers = Resources.FindObjectsOfTypeAll<AudioMixer>();
            foreach (AudioMixer mixer in mixers)
            {
                if (mixer.name.Contains(masterMixerName))
                {
                    _masterMixer = mixer;
                    _masterMixer.GetFloat(volumeParameterName, out _originalMixerVolume);
                    return;
                }
            }
            
            Debug.LogWarning("Aucun AudioMixer trouvé dans la scène. Le fade audio utilisera uniquement les AudioSources.");
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

        private void InitVideoPlayer()
        {
            if (videoPlayer != null)
            {
                // Configure le VideoPlayer
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.skipOnDrop = false; // Ne pas sauter de frames pour une lecture plus fluide
                
                // Cache le panel vidéo au départ
                if (videoPanel != null)
                    videoPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Amène les panels au sommet de la hiérarchie pour qu'ils soient visibles devant tout.
        /// </summary>
        private void BringToFront()
        {
            topPanel.transform.SetAsLastSibling();
            bottomPanel.transform.SetAsLastSibling();
            if (videoPanel != null)
                videoPanel.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Transition complète avec vidéo : blink (fermeture) → vidéo + chargement → blink (ouverture)
        /// </summary>
        public void TransitionToSceneWithVideo(string sceneName, VideoClip videoClip = null)
        {
            if (_isLoadingScene) return; // Éviter les transitions multiples
            
            _isLoadingScene = true;
            StartCoroutine(BlinkWithVideoThenLoad(sceneName, videoClip));
        }

        /// <summary>
        /// Transition sans vidéo (méthode originale)
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            if (_isLoadingScene) return; // Éviter les transitions multiples
            
            _isLoadingScene = true;
            StartCoroutine(BlinkThenLoad(sceneName));
        }

        private IEnumerator BlinkWithVideoThenLoad(string sceneName, VideoClip videoClip = null)
        {
            // Fade out audio pendant la fermeture
            StartCoroutine(FadeOutAudio());
            
            // Fermeture
            yield return StartCoroutine(Blink(close: true));

            // Préparer la vidéo
            VideoClip clipToPlay = videoClip ?? transitionVideo;
            bool hasVideo = videoPlayer != null && clipToPlay != null;
            
            // Démarrer la lecture de la vidéo
            if (hasVideo)
            {
                StartCoroutine(PlayTransitionVideo(clipToPlay));
                
                // Attendre un peu avant de commencer le chargement pour laisser la vidéo démarrer correctement
                if (preloadDelay > 0)
                    yield return new WaitForSecondsRealtime(preloadDelay);
            }
            
            // Démarrer le chargement de la scène en arrière-plan si on a une vidéo
            if (hasVideo && loadSceneDuringVideo)
            {
                // Chargement optimisé
                yield return StartCoroutine(LoadSceneAsyncOptimized(sceneName, clipToPlay));
            }
            else
            {
                // Pas de vidéo ou chargement pendant vidéo désactivé
                if (hasVideo)
                {
                    // Attendre la fin de la vidéo
                    yield return StartCoroutine(WaitForVideoToFinish(clipToPlay));
                }
                
                // Charger la scène normalement
                yield return SceneManager.LoadSceneAsync(sceneName);
            }
            
            yield return new WaitForEndOfFrame();

            // Force les panels à être sur le dessus
            BringToFront();
            
            // Ouverture
            yield return StartCoroutine(Blink(close: false));
            
            // Fade in audio après l'ouverture
            StartCoroutine(FadeInAudio());
            
            _isLoadingScene = false;
        }
        
        private IEnumerator LoadSceneAsyncOptimized(string sceneName, VideoClip clip)
        {
            // Démarrer le chargement asynchrone
            _sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);
            _sceneLoadOperation.allowSceneActivation = false;
            
            // Définir la priorité du thread de chargement (si possible)
            try
            {
                Thread.CurrentThread.Priority = loadingPriority;
            }
            catch (System.Exception)
            {
                // Ignorer si on ne peut pas changer la priorité
            }
            
            float lastTime = Time.realtimeSinceStartup;
            float videoLength = (float)clip.length;
            float timer = 0f;
            
            // Attendre que le chargement soit presque terminé
            while (_sceneLoadOperation.progress < 0.9f)
            {
                // Limiter le temps de chargement par frame pour éviter les saccades
                float currentTime = Time.realtimeSinceStartup;
                float deltaTime = currentTime - lastTime;
                
                if (deltaTime < maxLoadingTimePerFrame)
                {
                    // Attendre pour donner plus de temps à la vidéo
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    timer += deltaTime;
                }
                
                lastTime = Time.realtimeSinceStartup;
                
                // Si on a dépassé la durée de la vidéo, ne pas attendre plus
                if (timer > videoLength * 0.9f && !waitForVideoToFinish)
                {
                    break;
                }
                
                yield return null;
            }
            
            // Attendre que la vidéo soit terminée si demandé
            if (waitForVideoToFinish)
            {
                while (videoPlayer.isPlaying)
                {
                    yield return null;
                }
            }
            else
            {
                // Sinon, attendre au moins 75% de la durée de la vidéo
                float minWaitTime = videoLength * 0.75f;
                if (timer < minWaitTime)
                {
                    yield return new WaitForSecondsRealtime(minWaitTime - timer);
                }
            }
            
            // Cacher le panel vidéo
            if (videoPanel != null)
            {
                videoPanel.gameObject.SetActive(false);
            }
            
            // Activer la scène
            _sceneLoadOperation.allowSceneActivation = true;
            
            // Attendre que la scène soit activée
            while (!_sceneLoadOperation.isDone)
            {
                yield return null;
            }
            
            // Restaurer la priorité du thread
            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
            }
            catch (System.Exception)
            {
                // Ignorer si on ne peut pas changer la priorité
            }
        }
        
        private IEnumerator PlayTransitionVideo(VideoClip clip)
        {
            // Active et configure la vidéo
            if (videoPanel != null)
                videoPanel.gameObject.SetActive(true);
                
            videoPlayer.clip = clip;
            
            // Configurer la priorité de la vidéo
            videoPlayer.playbackSpeed = 1.0f;
            videoPlayer.skipOnDrop = false; // Ne pas sauter de frames
            
            // Prépare la vidéo
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            // Joue la vidéo
            videoPlayer.Play();
            
            // Attendre quelques frames pour s'assurer que la vidéo démarre correctement
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        
        private IEnumerator WaitForVideoToFinish(VideoClip clip)
        {
            // Attendre que la vidéo soit terminée
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }
            
            // Cacher le panel vidéo
            if (videoPanel != null)
            {
                videoPanel.gameObject.SetActive(false);
            }
        }

        private IEnumerator BlinkThenLoad(string sceneName)
        {
            // Fade out audio pendant la fermeture
            StartCoroutine(FadeOutAudio());
            
            // Fermeture
            yield return StartCoroutine(Blink(close: true));

            // Chargement de la nouvelle scène
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return new WaitForEndOfFrame();

            // Force les panels à être sur le dessus
            BringToFront();
            
            // Ouverture
            yield return StartCoroutine(Blink(close: false));
            
            // Fade in audio après l'ouverture
            StartCoroutine(FadeInAudio());
            
            _isLoadingScene = false;
        }

                /// <summary>
        /// Effectue un blink complet avec vidéo optionnelle au milieu
        /// </summary>
        public IEnumerator BlinkAndDoWithVideo(System.Func<IEnumerator> actionToRunMidBlink, VideoClip videoClip = null)
        {
            // Fade out audio pendant la fermeture
            StartCoroutine(FadeOutAudio());
            
            // Fermeture
            yield return StartCoroutine(Blink(true));
            Debug.Log("Fermeture des paupières");
            
            // Préparer la vidéo
            VideoClip clipToPlay = videoClip ?? transitionVideo;
            bool hasVideo = videoPlayer != null && clipToPlay != null;
            
            // Démarrer la lecture de la vidéo
            if (hasVideo)
            {
                StartCoroutine(PlayTransitionVideo(clipToPlay));
                
                // Attendre un peu avant d'exécuter l'action pour laisser la vidéo démarrer correctement
                if (preloadDelay > 0)
                    yield return new WaitForSecondsRealtime(preloadDelay);
            }
            
            // Exécuter l'action en parallèle avec la vidéo
            if (actionToRunMidBlink != null)
            {
                if (hasVideo)
                {
                    // Exécuter l'action avec une limitation de temps par frame pour éviter les saccades
                    StartCoroutine(ExecuteActionWithTimeLimit(actionToRunMidBlink));
                }
                else
                {
                    // Pas de vidéo, exécuter l'action normalement
                    yield return StartCoroutine(actionToRunMidBlink());
                }
            }
            
            // Attendre la fin de la vidéo si nécessaire
            if (hasVideo)
            {
                yield return StartCoroutine(WaitForVideoToFinish(clipToPlay));
            }

            yield return new WaitForEndOfFrame();
            BringToFront();

            // Ouverture
            Debug.Log("Ouverture des paupières");
            yield return StartCoroutine(Blink(false));
            
            // Fade in audio après l'ouverture
            StartCoroutine(FadeInAudio());
        }
        
        private IEnumerator ExecuteActionWithTimeLimit(System.Func<IEnumerator> action)
        {
            IEnumerator actionEnumerator = action();
            float lastTime = Time.realtimeSinceStartup;
            
            while (actionEnumerator.MoveNext())
            {
                // Limiter le temps d'exécution par frame pour éviter les saccades
                float currentTime = Time.realtimeSinceStartup;
                float deltaTime = currentTime - lastTime;
                
                if (deltaTime < maxLoadingTimePerFrame)
                {
                    // Continuer l'action
                    yield return actionEnumerator.Current;
                }
                else
                {
                    // Attendre la frame suivante pour donner plus de temps à la vidéo
                    yield return new WaitForEndOfFrame();
                }
                
                lastTime = Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Effectue un blink complet en deux étapes, avec une action intermédiaire (méthode originale).
        /// </summary>
        public IEnumerator BlinkAndDo(System.Func<IEnumerator> actionToRunMidBlink)
        {
            // Fade out audio pendant la fermeture
            StartCoroutine(FadeOutAudio());
            
            // Fermeture
            yield return StartCoroutine(Blink(true));
            Debug.Log("Fermeture des paupières");
            
            // Exécute l'action centrale pendant l'écran noir
            if (actionToRunMidBlink != null)
                yield return StartCoroutine(actionToRunMidBlink());

            yield return new WaitForEndOfFrame();
            BringToFront();

            // Ouverture
            Debug.Log("Ouverture des paupières");
            yield return StartCoroutine(Blink(false));
            
            // Fade in audio après l'ouverture
            StartCoroutine(FadeInAudio());
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
        
        /// <summary>
        /// Collecte toutes les sources audio actives dans la scène
        /// </summary>
        private void CollectActiveAudioSources()
        {
            _activeAudioSources.Clear();
            AudioSource[] sources = FindObjectsOfType<AudioSource>();
            
            foreach (AudioSource source in sources)
            {
                if (source.isPlaying && source.volume > 0)
                {
                    _activeAudioSources[source] = source.volume;
                }
            }
            
            // Chercher le mixer si on ne l'a pas encore
            if (_masterMixer == null)
            {
                FindAudioMixer();
            }
        }
        
        /// <summary>
        /// Fade out progressif de l'audio
        /// </summary>
        private IEnumerator FadeOutAudio()
        {
            // Collecter les sources audio actives
            CollectActiveAudioSources();
            
            float timer = 0f;
            
            while (timer < audioFadeDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / audioFadeDuration);
                
                // Fade du mixer si disponible
                if (_masterMixer != null)
                {
                    float dbValue = Mathf.Lerp(_originalMixerVolume, -80f, t);
                    _masterMixer.SetFloat(volumeParameterName, dbValue);
                }
                
                // Fade de chaque source audio individuelle
                foreach (var kvp in _activeAudioSources)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.volume = Mathf.Lerp(kvp.Value, 0f, t);
                    }
                }
                
                yield return null;
            }
            
            // Assurer que le volume est à zéro
            if (_masterMixer != null)
            {
                _masterMixer.SetFloat(volumeParameterName, -80f);
            }
            
            foreach (var kvp in _activeAudioSources)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.volume = 0f;
                }
            }
        }
        
        /// <summary>
        /// Fade in progressif de l'audio
        /// </summary>
        private IEnumerator FadeInAudio()
        {
            // Attendre une frame pour que les nouveaux AudioSources soient initialisés
            yield return null;
            
            // Collecter les nouvelles sources audio dans la scène
            _activeAudioSources.Clear();
            AudioSource[] sources = FindObjectsOfType<AudioSource>();
            
            foreach (AudioSource source in sources)
            {
                if (source.isPlaying)
                {
                    // Stocker le volume cible
                    float targetVolume = source.volume;
                    _activeAudioSources[source] = targetVolume;
                    
                    // Commencer à volume zéro
                    source.volume = 0f;
                }
            }
            
            // Chercher le mixer dans la nouvelle scène si nécessaire
            if (_masterMixer == null)
            {
                FindAudioMixer();
            }
            
            float timer = 0f;
            
            while (timer < audioFadeDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / audioFadeDuration);
                
                // Fade du mixer si disponible
                if (_masterMixer != null)
                {
                    float dbValue = Mathf.Lerp(-80f, _originalMixerVolume, t);
                    _masterMixer.SetFloat(volumeParameterName, dbValue);
                }
                
                // Fade de chaque source audio individuelle
                foreach (var kvp in _activeAudioSources)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.volume = Mathf.Lerp(0f, kvp.Value, t);
                    }
                }
                
                yield return null;
            }
            
            // Assurer que le volume est restauré
            if (_masterMixer != null)
            {
                _masterMixer.SetFloat(volumeParameterName, _originalMixerVolume);
            }
            
            foreach (var kvp in _activeAudioSources)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.volume = kvp.Value;
                }
            }
        }
        
        /// <summary>
        /// Obtient la durée estimée de la transition complète
        /// </summary>
        public float GetEstimatedTransitionDuration(bool withVideo, VideoClip customClip = null)
        {
            float duration = blinkDuration * 2; // Fermeture + ouverture
            
            if (withVideo)
            {
                VideoClip clip = customClip ?? transitionVideo;
                if (clip != null)
                {
                    duration += (float)clip.length;
                }
            }
            
            // Ajouter un petit délai pour le chargement de la scène
            duration += 0.2f;
            
            return duration;
        }
        
        /// <summary>
        /// Méthode utilitaire pour tester la transition
        /// </summary>
        public void TestTransition(string sceneName, bool withVideo)
        {
            if (_isLoadingScene) return;
            
            if (withVideo)
            {
                TransitionToSceneWithVideo(sceneName);
            }
            else
            {
                TransitionToScene(sceneName);
            }
        }
        
        /// <summary>
        /// Vérifie si une transition est en cours
        /// </summary>
        public bool IsTransitioning()
        {
            return _isLoadingScene;
        }
        
        /// <summary>
        /// Configure les paramètres d'optimisation pour la vidéo
        /// </summary>
        public void ConfigureOptimization(bool loadDuringVideo, float delay, ThreadPriority priority, float maxTimePerFrame, bool waitForVideo)
        {
            loadSceneDuringVideo = loadDuringVideo;
            preloadDelay = delay;
            loadingPriority = priority;
            maxLoadingTimePerFrame = maxTimePerFrame;
            waitForVideoToFinish = waitForVideo;
        }
    }
}

