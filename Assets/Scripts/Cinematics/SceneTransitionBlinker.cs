using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

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

        private Vector2 topClosedPos;
        private Vector2 bottomClosedPos;
        private Vector2 topOpenPos;
        private Vector2 bottomOpenPos;
        
        // Dictionnaire pour stocker les sources audio et leurs volumes originaux
        private Dictionary<AudioSource, float> _activeAudioSources = new Dictionary<AudioSource, float>();
        
        // Référence au mixer trouvé dynamiquement
        private AudioMixer _masterMixer;
        private float _originalMixerVolume = 0f;

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
        /// Transition complète avec vidéo : blink (fermeture) → vidéo → chargement → blink (ouverture)
        /// </summary>
        public void TransitionToSceneWithVideo(string sceneName, VideoClip videoClip = null)
        {
            StartCoroutine(BlinkWithVideoThenLoad(sceneName, videoClip));
        }

        /// <summary>
        /// Transition sans vidéo (méthode originale)
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            StartCoroutine(BlinkThenLoad(sceneName));
        }

        private IEnumerator BlinkWithVideoThenLoad(string sceneName, VideoClip videoClip = null)
        {
            // Fade out audio pendant la fermeture
            StartCoroutine(FadeOutAudio());
            
            // Fermeture
            yield return StartCoroutine(Blink(close: true));

            // Joue la vidéo si disponible
            if (videoPlayer != null && (videoClip != null || transitionVideo != null))
            {
                yield return StartCoroutine(PlayTransitionVideo(videoClip ?? transitionVideo));
            }

            // Chargement de la nouvelle scène
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return new WaitForEndOfFrame();

            // Force les panels à être sur le dessus
            BringToFront();
            
            // Ouverture
            yield return StartCoroutine(Blink(close: false));
            
            // Fade in audio après l'ouverture
            StartCoroutine(FadeInAudio());
        }

        private IEnumerator PlayTransitionVideo(VideoClip clip)
        {
            // Active et configure la vidéo
            videoPanel.gameObject.SetActive(true);
            videoPlayer.clip = clip;
            
            // Prépare la vidéo
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            // Joue la vidéo
            videoPlayer.Play();
            
            // Attend la fin de la vidéo
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            // Cache le panel vidéo
            videoPanel.gameObject.SetActive(false);
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
            
            // Joue la vidéo si disponible
            if (videoPlayer != null && (videoClip != null || transitionVideo != null))
            {
                yield return StartCoroutine(PlayTransitionVideo(videoClip ?? transitionVideo));
            }
            
            // Exécute l'action centrale
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
    }
}
