﻿using System.Collections;
using System.Collections.Generic;
using Cinematics;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace TimeSystem
{
    /// <summary>
    /// Gère les transitions entre scènes avec une animation de flèche.
    /// Supporte les déclencheurs clavier et MIDI.
    /// </summary>
    public class CheckpointChanger : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private PlayableDirector director;
        [SerializeField] private ProceduralImage circleImage;
        [SerializeField] private ProceduralImage timerImage;
        [SerializeField] private ProceduralImage arrowImage;

        [Header("Configuration")] [SerializeField]
        private string targetSceneName;

        [SerializeField] private bool useVideoTransition = true;
        [SerializeField] private KeyCode changeSceneKey = KeyCode.Space;
        [SerializeField] private float confirmationTimeWindow = 2f;
        [SerializeField] private bool startAtZeroDegrees = true; // Toggle pour commencer à 0° ou 90°
        [SerializeField] private float delayBeforeSceneChange = 0.5f; // Délai après l'animation
        [SerializeField] private float opacityWithNoAnimation = 40f;

        [Header("Animation de base")] [SerializeField]
        private float animationDuration = 0.3f;

        [SerializeField] private RectTransform arrowTransform;

        [Header("Effets d'animation")] [SerializeField]
        private Ease startEase = Ease.OutQuint; // Effet pour la première partie de l'animation

        [SerializeField] private Ease endEase = Ease.InOutBack; // Effet pour la seconde partie de l'animation
        [SerializeField] private float overshootAmount = 5f; // Amplitude du dépassement en degrés
        [SerializeField] private float startDurationRatio = 0.7f; // Ratio de la durée pour la première partie (0-1)

        [Header("Animation de balancement")] [SerializeField]
        private float wobbleAmplitude = 5f; // Amplitude du balancement (±5° autour de la position)

        [SerializeField] private float wobbleFrequency = 2f; // Fréquence du balancement (oscillations par seconde)

        [Header("Timer visuel")] [SerializeField]
        private Image timerFillImage; // Image qui se remplit/vide

        [SerializeField] private Color timerStartColor = Color.white; // Couleur au début du timer
        [SerializeField] private Color timerEndColor = Color.white; // Couleur à la fin du timer

        [Header("Images de thème")] [SerializeField]
        private GameObject lightThemeIcon; // Icône du thème clair

        [SerializeField] private GameObject darkThemeIcon; // Icône du thème sombre
        [SerializeField] private float themeIconFadeDuration = 0.3f; // Durée de l'animation de fondu
        [SerializeField] private Ease themeIconFadeEase = Ease.OutQuad; // Effet pour l'animation de fondu

        [Header("MIDI Configuration")] [SerializeField]
        private bool useMidiTriggers = true;

        [SerializeField] private List<MidiBind> midiBindings = new List<MidiBind>();

        // États de la flèche
        private float _currentRotation;
        private float _targetRotation;
        private float _transitionRotation = 45f; // Position intermédiaire

        // Variables privées
        private bool _waitingForConfirmation = false;
        private float _confirmationTimer = 0f;
        private Sequence _currentAnimation;
        private Coroutine _sceneChangeCoroutine;
        private Tweener _wobbleTweener;
        private Sequence _timerSequence;
        private Sequence _themeIconSequence;

        private void Start()
        {
            // S'abonner aux événements MIDI
            SubscribeToMidi();

            // Initialiser la rotation de la flèche en fonction du toggle
            _currentRotation = startAtZeroDegrees ? 0f : 90f;
            UpdateArrowRotation(_currentRotation, 0);

            // Initialiser l'image du timer
            if (timerFillImage != null)
            {
                timerFillImage.fillAmount = 0;
                timerFillImage.gameObject.SetActive(false);
            }

            // Initialiser les icônes de thème
            UpdateThemeIcons(false);

            // Add opacity
            circleImage.color = new Color(circleImage.color.r, circleImage.color.g, circleImage.color.b, (opacityWithNoAnimation / 255f));
            timerImage.color = new Color(timerImage.color.r, timerImage.color.g, timerImage.color.b, (opacityWithNoAnimation / 255f));
            arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, (opacityWithNoAnimation / 255f));
        }

        private void Update()
        {
            // Gérer le timer de confirmation
            if (_waitingForConfirmation)
            {
                _confirmationTimer -= Time.deltaTime;

                if (_confirmationTimer <= 0)
                {
                    _waitingForConfirmation = false;
                    CancelSceneChange();
                }
            }

            // Vérifier l'entrée clavier
            if (Input.GetKeyDown(changeSceneKey))
            {
                ToggleScene();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromMidi();
            StopAllAnimations();

            if (_sceneChangeCoroutine != null)
            {
                StopCoroutine(_sceneChangeCoroutine);
                _sceneChangeCoroutine = null;
            }
        }

        #region MIDI Handling

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

        private void OnMidiTrigger(float value)
        {
            if (value == 0)
            {
                ToggleScene();
            }
        }

        #endregion

        /// <summary>
        /// Méthode principale pour déclencher le processus de changement de scène
        /// </summary>
        public void ToggleScene()
        {
            if (_waitingForConfirmation)
            {
                // Confirmer le changement
                _waitingForConfirmation = false;
                StopTimerAnimation();
                ConfirmSceneChange();
            }
            else
            {
                // Demander confirmation
                _waitingForConfirmation = true;
                _confirmationTimer = confirmationTimeWindow;
                PrepareSceneChange();
                StartTimerAnimation();
            }
        }

        private void PrepareSceneChange()
        {
            // Définir la rotation cible (inverse de la rotation actuelle)
            _targetRotation = (_currentRotation == 0f) ? 90f : 0f;

            // Mettre la flèche en position intermédiaire avec effet
            AnimateToTransitionPosition();

            // Démarrer l'animation de balancement après la transition
            _currentAnimation.OnComplete(() => { StartWobbleAnimation(); });

            // Add opacity
            circleImage.color = new Color(circleImage.color.r, circleImage.color.g, circleImage.color.b, 1f);
            timerImage.color = new Color(timerImage.color.r, timerImage.color.g, timerImage.color.b, 1f);
            arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, 1f);
        }

        private void ConfirmSceneChange()
        {
            // Arrêter l'animation de balancement
            StopWobbleAnimation();

            // Mettre à jour la rotation actuelle
            _currentRotation = _targetRotation;

            // Animer la flèche vers la position finale
            UpdateArrowRotation(_currentRotation, animationDuration);

            // Mettre à jour les icônes de thème avec animation
            UpdateThemeIcons(true);

            // Attendre la fin de l'animation avant de changer de scène
            if (_sceneChangeCoroutine != null)
            {
                StopCoroutine(_sceneChangeCoroutine);
            }

            _sceneChangeCoroutine = StartCoroutine(ChangeSceneAfterDelay());
        }

        private IEnumerator ChangeSceneAfterDelay()
        {
            // Attendre la fin de l'animation + le délai supplémentaire
            yield return new WaitForSeconds(animationDuration + delayBeforeSceneChange);

            // Utiliser le SceneTransitionBlinker pour changer de scène
            PerformSceneChange();
        }

        private void CancelSceneChange()
        {
            // Arrêter l'animation de balancement
            StopWobbleAnimation();

            // Arrêter l'animation du timer
            StopTimerAnimation();

            // Remettre la flèche à sa position initiale
            UpdateArrowRotation(_currentRotation, animationDuration);

            // Add opacity
            circleImage.color = new Color(circleImage.color.r, circleImage.color.g, circleImage.color.b, (opacityWithNoAnimation / 255f));
            timerImage.color = new Color(timerImage.color.r, timerImage.color.g, timerImage.color.b, (opacityWithNoAnimation / 255f));
            arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, (opacityWithNoAnimation / 255f));
        }

        /// <summary>
        /// Effectue le changement de scène en utilisant le SceneTransitionBlinker
        /// </summary>
        private void PerformSceneChange()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("Nom de scène cible non défini!");
                return;
            }

            director.Pause();

            if (useVideoTransition)
                SceneTransitionBlinker.Instance.TransitionToSceneWithVideo(targetSceneName);
            else
                SceneTransitionBlinker.Instance.TransitionToScene(targetSceneName);
        }

        /// <summary>
        /// Anime la flèche vers la position intermédiaire avec un effet de transition
        /// </summary>
        private void AnimateToTransitionPosition()
        {
            if (arrowTransform == null)
                return;

            StopCurrentAnimation();

            // Animation avec effet
            _currentAnimation = DOTween.Sequence();

            // Calculer un léger dépassement dans la direction du mouvement
            float overshoot = _currentRotation < _transitionRotation ? overshootAmount : -overshootAmount;

            // Première partie: aller un peu au-delà de la position cible
            _currentAnimation.Append(
                arrowTransform.DORotate(new Vector3(0, 0, _transitionRotation + overshoot),
                        animationDuration * startDurationRatio)
                    .SetEase(startEase)
            );

            // Deuxième partie: revenir à la position cible exacte
            _currentAnimation.Append(
                arrowTransform.DORotate(new Vector3(0, 0, _transitionRotation),
                        animationDuration * (1 - startDurationRatio))
                    .SetEase(endEase)
            );
        }

        private void UpdateArrowRotation(float rotation, float duration)
        {
            if (arrowTransform == null)
                return;

            StopCurrentAnimation();

            if (duration <= 0)
            {
                // Mise à jour immédiate
                arrowTransform.rotation = Quaternion.Euler(0, 0, rotation);
            }
            else
            {
                // Animation avec effet
                _currentAnimation = DOTween.Sequence();

                // Calculer un léger dépassement dans la direction du mouvement
                float currentRotationZ = arrowTransform.rotation.eulerAngles.z;
                // Normaliser l'angle entre 0 et 360
                if (currentRotationZ > 180) currentRotationZ -= 360;
                float overshoot = currentRotationZ < rotation ? overshootAmount : -overshootAmount;

                // Première partie: aller un peu au-delà de la position cible
                _currentAnimation.Append(
                    arrowTransform.DORotate(new Vector3(0, 0, rotation + overshoot), duration * startDurationRatio)
                        .SetEase(startEase)
                );

                // Deuxième partie: revenir à la position cible exacte
                _currentAnimation.Append(
                    arrowTransform.DORotate(new Vector3(0, 0, rotation), duration * (1 - startDurationRatio))
                        .SetEase(endEase)
                );
            }
        }

        /// <summary>
        /// Met à jour les icônes de thème en fonction de la rotation actuelle
        /// </summary>
        private void UpdateThemeIcons(bool animate)
        {
            if (lightThemeIcon == null || darkThemeIcon == null)
                return;

            StopThemeIconAnimation();

            // Déterminer quel thème est actif
            bool isLightTheme = _currentRotation == 0f;

            if (!animate)
            {
                // Mise à jour immédiate
                lightThemeIcon.SetActive(isLightTheme);
                darkThemeIcon.SetActive(!isLightTheme);
            }
            else
            {
                // Animation avec fondu
                _themeIconSequence = DOTween.Sequence();

                // Configurer les objets
                lightThemeIcon.SetActive(true);
                darkThemeIcon.SetActive(true);

                // Obtenir les CanvasGroup ou les ajouter si nécessaire
                CanvasGroup lightGroup = GetOrAddCanvasGroup(lightThemeIcon);
                CanvasGroup darkGroup = GetOrAddCanvasGroup(darkThemeIcon);

                // Configurer l'alpha initial
                lightGroup.alpha = isLightTheme ? 0 : 1;
                darkGroup.alpha = isLightTheme ? 1 : 0;

                // Animer le fondu
                _themeIconSequence.Append(
                    lightGroup.DOFade(isLightTheme ? 1 : 0, themeIconFadeDuration)
                        .SetEase(themeIconFadeEase)
                );

                _themeIconSequence.Join(
                    darkGroup.DOFade(isLightTheme ? 0 : 1, themeIconFadeDuration)
                        .SetEase(themeIconFadeEase)
                );

                // Désactiver l'icône invisible à la fin
                _themeIconSequence.OnComplete(() =>
                {
                    lightThemeIcon.SetActive(isLightTheme);
                    darkThemeIcon.SetActive(!isLightTheme);
                });
            }
        }

        /// <summary>
        /// Obtient ou ajoute un CanvasGroup à un GameObject
        /// </summary>
        private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
        {
            CanvasGroup group = obj.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = obj.AddComponent<CanvasGroup>();
            }

            return group;
        }

        private void StartWobbleAnimation()
        {
            if (arrowTransform == null)
                return;

            StopWobbleAnimation();

            // Créer une animation de balancement sinusoïdal
            float baseRotation = _transitionRotation;

            // Utiliser DOTween pour animer une valeur de 0 à 1 en boucle
            _wobbleTweener = DOTween.To(
                    () => 0f,
                    (x) =>
                    {
                        // Calculer la rotation en fonction d'une courbe sinusoïdale
                        float wobbleOffset = Mathf.Sin(x * Mathf.PI * 2) * wobbleAmplitude;
                        arrowTransform.rotation = Quaternion.Euler(0, 0, baseRotation + wobbleOffset);
                    },
                    1f,
                    1f / wobbleFrequency
                )
                .SetEase(Ease.Linear)
                .SetLoops(-1); // Boucle infinie
        }

        private void StopWobbleAnimation()
        {
            if (_wobbleTweener != null && _wobbleTweener.IsActive())
            {
                _wobbleTweener.Kill();
                _wobbleTweener = null;
            }
        }

        private void StartTimerAnimation()
        {
            if (timerFillImage == null)
                return;

            StopTimerAnimation();

            // Activer l'image du timer
            timerFillImage.gameObject.SetActive(true);
            timerFillImage.fillAmount = 1f;
            timerFillImage.color = timerStartColor;

            // Créer une séquence pour l'animation du timer
            _timerSequence = DOTween.Sequence();

            // Animer le fill amount
            _timerSequence.Append(
                timerFillImage.DOFillAmount(0f, confirmationTimeWindow)
                    .SetEase(Ease.Linear)
            );

            // Animer la couleur
            _timerSequence.Join(
                timerFillImage.DOColor(timerEndColor, confirmationTimeWindow)
                    .SetEase(Ease.Linear)
            );

            // Cacher l'image à la fin
            _timerSequence.OnComplete(() => { timerFillImage.gameObject.SetActive(false); });
        }

        private void StopTimerAnimation()
        {
            if (_timerSequence != null && _timerSequence.IsActive())
            {
                _timerSequence.Kill();
                _timerSequence = null;
            }

            // Cacher l'image du timer
            if (timerFillImage != null)
            {
                timerFillImage.gameObject.SetActive(false);
            }
        }

        private void StopThemeIconAnimation()
        {
            if (_themeIconSequence != null && _themeIconSequence.IsActive())
            {
                _themeIconSequence.Kill();
                _themeIconSequence = null;
            }
        }

        private void StopCurrentAnimation()
        {
            if (_currentAnimation != null && _currentAnimation.IsActive())
            {
                _currentAnimation.Kill();
                _currentAnimation = null;
            }
        }

        private void StopAllAnimations()
        {
            StopCurrentAnimation();
            StopWobbleAnimation();
            StopTimerAnimation();
            StopThemeIconAnimation();
        }
    }
}