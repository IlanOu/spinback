using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
namespace UI.PauseMenu
{

    /// <summary>
    /// Script simple pour animer l'apparition/disparition d'éléments UI avec DOTween.
    /// </summary>
    public class SimpleUIAnimator : MonoBehaviour
    {
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private bool animateScale = true;
        [SerializeField] private bool animateFade = true;
        
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Créer un CanvasGroup si nécessaire
            if (_canvasGroup == null && animateFade)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            Show();
        }

        /// <summary>
        /// Affiche l'UI avec animation
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            
            if (animateScale && _rectTransform != null)
            {
                _rectTransform.localScale = Vector3.zero;
                _rectTransform.DOScale(Vector3.one, animationDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true); // Fonctionne en pause
            }
            
            if (animateFade && _canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.DOFade(1, animationDuration)
                    .SetUpdate(true); // Fonctionne en pause
            }
        }
        
        /// <summary>
        /// Cache l'UI avec animation
        /// </summary>
        public void Hide()
        {
            if (animateScale && _rectTransform != null)
            {
                _rectTransform.DOScale(Vector3.zero, animationDuration)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true); // Fonctionne en pause
            }
            
            if (animateFade && _canvasGroup != null)
            {
                _canvasGroup.DOFade(0, animationDuration)
                    .SetUpdate(true) // Fonctionne en pause
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                // Si pas de fade, désactiver après l'animation d'échelle
                DOVirtual.DelayedCall(animationDuration, () => gameObject.SetActive(false))
                    .SetUpdate(true);
            }
        }
        
        /// <summary>
        /// Bascule entre affichage et masquage
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }
        
        private void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}