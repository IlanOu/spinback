using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class PauseBackground : MonoBehaviour, IPausable
{
    [Header("Background Configuration")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color backgroundActiveColor = new Color(0, 0, 0, 0.7f);
    [SerializeField] private float animationDuration = 0.3f;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onBackgroundShown;
    [SerializeField] private UnityEvent onBackgroundHidden;

    private Sequence _backgroundSequence;
    private bool _isVisible = false;

    private void Awake()
    {
        // S'assurer que le background est désactivé au démarrage
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
            backgroundImage.color = new Color(backgroundActiveColor.r, backgroundActiveColor.g, backgroundActiveColor.b, 0f);
        }
    }

    /// <summary>
    /// Affiche ou cache le fond en fonction du paramètre
    /// </summary>
    /// <param name="show">True pour afficher, False pour cacher</param>
    public void DisplayBackground(bool show)
    {
        if (show)
        {
            ShowBackground();
        }
        else
        {
            HideBackground();
        }
    }

    /// <summary>
    /// Affiche le fond avec animation
    /// </summary>
    public void ShowBackground()
    {
        if (backgroundImage == null || _isVisible) return;

        // Arrêter toute animation en cours
        if (_backgroundSequence != null && _backgroundSequence.IsActive())
        {
            _backgroundSequence.Kill();
        }

        // Activer le background
        backgroundImage.gameObject.SetActive(true);
        backgroundImage.color = new Color(backgroundActiveColor.r, backgroundActiveColor.g, backgroundActiveColor.b, 0f);

        // Créer une nouvelle séquence d'animation
        _backgroundSequence = DOTween.Sequence();
        _backgroundSequence.Append(backgroundImage.DOFade(backgroundActiveColor.a, animationDuration))
            .SetUpdate(true) // Pour que l'animation fonctionne en pause
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                _isVisible = true;
                onBackgroundShown?.Invoke();
            });
    }

    /// <summary>
    /// Cache le fond avec animation
    /// </summary>
    public void HideBackground()
    {
        if (backgroundImage == null || !_isVisible) return;

        // Arrêter toute animation en cours
        if (_backgroundSequence != null && _backgroundSequence.IsActive())
        {
            _backgroundSequence.Kill();
        }

        // Créer une nouvelle séquence d'animation
        _backgroundSequence = DOTween.Sequence();
        _backgroundSequence.Append(backgroundImage.DOFade(0f, animationDuration))
            .SetUpdate(true) // Pour que l'animation fonctionne en pause
            .SetEase(Ease.InQuad)
            .OnComplete(() => 
            {
                // Désactiver le background à la fin de l'animation
                backgroundImage.gameObject.SetActive(false);
                _isVisible = false;
                onBackgroundHidden?.Invoke();
            });
    }

    /// <summary>
    /// Implémentation de IPausable.Pause()
    /// </summary>
    public void Pause()
    {
        // Cette méthode est appelée par le PauseManager quand le jeu est mis en pause
        DisplayBackground(true);
    }

    /// <summary>
    /// Implémentation de IPausable.Resume()
    /// </summary>
    public void Resume()
    {
        // Cette méthode est appelée par le PauseManager quand le jeu reprend
        DisplayBackground(false);
    }

    private void OnDestroy()
    {
        // Arrêter toutes les animations DOTween
        if (_backgroundSequence != null && _backgroundSequence.IsActive())
        {
            _backgroundSequence.Kill();
        }
    }
}
