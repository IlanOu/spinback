using Cinematics;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TimeSystem
{
    /// <summary>
    /// Gère les transitions entre scènes avec confirmation et animations.
    /// Supporte les déclencheurs clavier et MIDI.
    /// </summary>
    public class CheckpointChanger : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Configuration de scène")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private bool useVideoTransition = true;
        [SerializeField] private KeyCode changeSceneKey = KeyCode.Space;
        
        [Header("Configuration de confirmation")]
        [SerializeField] private float confirmationTimeWindow = 2f;
        [SerializeField] private GameObject confirmationUI;
        
        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform uiRectTransform;
        [SerializeField] private Image timerFillImage;
        
        [Header("MIDI Configuration")]
        [SerializeField] private bool useMidiTriggers = true;
        [SerializeField] private List<MidiBind> midiBindings = new List<MidiBind>();
        #endregion

        #region Private Fields
        private bool _waitingForConfirmation = false;
        private float _confirmationTimer = 0f;
        private Vector3 _originalScale;
        private Sequence _currentAnimation;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            InitializeReferences();
        }
        
        private void Start()
        {
            SubscribeToMidi();
            HideUIImmediately();
        }
        
        private void Update()
        {
            HandleConfirmationTimer();
            CheckKeyboardInput();
        }
        
        private void OnDisable()
        {
            CleanupAnimations();
            HideUIImmediately();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromMidi();
            CleanupAnimations();
        }
        #endregion

        #region Initialization
        private void InitializeReferences()
        {
            // Initialiser les références UI si non assignées
            if (confirmationUI != null)
            {
                if (canvasGroup == null)
                    canvasGroup = confirmationUI.GetComponent<CanvasGroup>();
                
                if (uiRectTransform == null)
                    uiRectTransform = confirmationUI.GetComponent<RectTransform>();
            }
            
            // Stocker l'échelle originale
            if (uiRectTransform != null)
                _originalScale = uiRectTransform.localScale;
        }
        
        private void SubscribeToMidi()
        {
            if (!useMidiTriggers || MidiBinding.Instance == null)
                return;
                
            foreach (var binding in midiBindings)
            {
                MidiBinding.Instance.Subscribe(binding, OnMidiTrigger);
            }
        }
        
        private void UnsubscribeFromMidi()
        {
            if (!useMidiTriggers || MidiBinding.Instance == null)
                return;
                
            foreach (var binding in midiBindings)
            {
                MidiBinding.Instance.Unsubscribe(binding, OnMidiTrigger);
            }
        }
        
        private void HideUIImmediately()
        {
            if (confirmationUI != null)
            {
                confirmationUI.SetActive(false);
                
                if (canvasGroup != null)
                    canvasGroup.alpha = 0;
                
                if (uiRectTransform != null)
                    uiRectTransform.localScale = Vector3.zero;
            }
        }
        #endregion

        #region Input Handling
        private void OnMidiTrigger(float value)
        {
            if (value == 0)
            {
                GoToCheckpoint();
            }
        }
        
        private void CheckKeyboardInput()
        {
            if (Input.GetKeyDown(changeSceneKey))
            {
                GoToCheckpoint();
            }
        }
        
        private void HandleConfirmationTimer()
        {
            if (!_waitingForConfirmation)
                return;
                
            _confirmationTimer -= Time.deltaTime;
            UpdateTimerFill();
            
            if (_confirmationTimer <= 0)
            {
                _waitingForConfirmation = false;
                HideConfirmationUI();
            }
        }
        #endregion

        #region UI Management
        private void UpdateTimerFill()
        {
            if (timerFillImage != null)
            {
                timerFillImage.fillAmount = _confirmationTimer / confirmationTimeWindow;
            }
        }
        
        private void ShowConfirmationUI()
        {
            if (confirmationUI == null)
                return;
                
            StopCurrentAnimation();
            confirmationUI.SetActive(true);
            
            _currentAnimation = DOTween.Sequence();
            
            // Animer le fade
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                _currentAnimation.Join(canvasGroup.DOFade(1, animationDuration));
            }
            
            // Animer le scale
            if (uiRectTransform != null)
            {
                uiRectTransform.localScale = Vector3.zero;
                _currentAnimation.Join(uiRectTransform.DOScale(_originalScale, animationDuration).SetEase(showEase));
            }
            
            // Réinitialiser le timer fill
            if (timerFillImage != null)
            {
                timerFillImage.fillAmount = 1;
            }
            
            Debug.Log("Appuyez à nouveau pour confirmer le changement de scène");
        }
        
        private void HideConfirmationUI()
        {
            if (confirmationUI == null)
                return;
                
            StopCurrentAnimation();
            
            _currentAnimation = DOTween.Sequence();
            
            // Animer le fade
            if (canvasGroup != null)
            {
                _currentAnimation.Join(canvasGroup.DOFade(0, animationDuration));
            }
            
            // Animer le scale
            if (uiRectTransform != null)
            {
                _currentAnimation.Join(uiRectTransform.DOScale(Vector3.zero, animationDuration).SetEase(hideEase));
            }
            
            // Désactiver l'UI à la fin
            _currentAnimation.OnComplete(() => {
                if (confirmationUI != null)
                    confirmationUI.SetActive(false);
            });
        }
        
        private void StopCurrentAnimation()
        {
            if (_currentAnimation != null && _currentAnimation.IsActive())
            {
                _currentAnimation.Kill();
                _currentAnimation = null;
            }
        }
        
        private void CleanupAnimations()
        {
            StopCurrentAnimation();
        }
        #endregion

        #region Scene Transition
        /// <summary>
        /// Méthode principale pour déclencher le processus de changement de scène
        /// </summary>
        public void GoToCheckpoint()
        {
            if (_waitingForConfirmation)
            {
                // Confirmer le changement
                _waitingForConfirmation = false;
                HideConfirmationUI();
                PerformSceneChange();
            }
            else
            {
                // Demander confirmation
                _waitingForConfirmation = true;
                _confirmationTimer = confirmationTimeWindow;
                ShowConfirmationUI();
            }
        }
        
        /// <summary>
        /// Effectue le changement de scène sans demander de confirmation
        /// </summary>
        private void PerformSceneChange()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("Nom de scène cible non défini!");
                return;
            }
            
            if (useVideoTransition)
                SceneTransitionBlinker.Instance.TransitionToSceneWithVideo(targetSceneName);
            else
                SceneTransitionBlinker.Instance.TransitionToScene(targetSceneName);
        }
        #endregion
    }
}
