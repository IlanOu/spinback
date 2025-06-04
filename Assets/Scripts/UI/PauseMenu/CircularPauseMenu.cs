using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CircularMenu : MonoBehaviour
{
    [Header("Menu Configuration")] [SerializeField]
    private Transform menuCenter;

    [SerializeField] private float menuRadius = 400f;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private List<MidiBind> midiBindings = new List<MidiBind>();
    
    [SerializeField]
    private bool closeMenuOnSelection = false; // Option pour fermer automatiquement le menu à la sélection

    [Header("Circle Position")] [SerializeField]
    private float startAngle = 0f; // Angle de départ (0-360°)

    [SerializeField] private bool clockwiseDistribution = true; // Distribution dans le sens horaire ou anti-horaire

    [Header("Menu Items")] [SerializeField]
    private List<MenuItem> menuItems = new List<MenuItem>();

    [Header("Visual Elements")]

    [SerializeField] private Transform arrowIndicator; // Flèche au centre
    [SerializeField] private float selectedItemScale = 1.2f; // Échelle de l'élément sélectionné
    
    [SerializeField] private CanvasGroup menuCanvasGroup;
    
    [Header("Arrow Animation")] [SerializeField]
    private float arrowAppearDuration = 0.3f;

    [SerializeField] private Ease arrowAppearEase = Ease.OutBack;
    [SerializeField] private Ease arrowDisappearEase = Ease.InBack;
    [SerializeField] private float arrowPulseMagnitude = 0.1f; // Amplitude de la pulsation
    [SerializeField] private float arrowPulseSpeed = 1f; // Vitesse de la pulsation

    [Header("External UI Elements")] [SerializeField]
    private UnityEvent onMenuOpen; // Événements à déclencher quand le menu s'ouvre

    [SerializeField] private UnityEvent onMenuClose; // Événements à déclencher quand le menu se ferme

    [Serializable]
    public class MenuItem
    {
        public string name;
        public RectTransform itemTransform;
        public UnityEvent onSelect;
        public bool isActive = true; // Indique si l'élément est actif ou grisé
        [Range(0, 1)] public float inactiveAlpha = 0.5f; // Transparence pour les éléments inactifs
        public Color activeColor = Color.white; // Couleur pour les éléments actifs
        public Color inactiveColor = Color.gray; // Couleur pour les éléments inactifs
    }

    private bool _isOpen = false;
    private int _selectedIndex = -1;
    private Vector2[] _itemTargetPositions;
    private bool _isAnimating = false;
    private float _lastToggleTime = 0f;
    private Vector3[] _itemOriginalScales; // Échelles originales des éléments
    private Vector3 _arrowOriginalScale;
    private Sequence _arrowPulseSequence;
    private Image[] _itemImages; // Références aux composants Image des éléments
    private Color[] _itemOriginalColors; // Couleurs originales des éléments

    private CursorCommandToken cursorToken;
    private PauseCommandToken pauseToken;

    private void Start()
    {
        // Initialiser les positions cibles et les échelles
        _itemTargetPositions = new Vector2[menuItems.Count];
        _itemOriginalScales = new Vector3[menuItems.Count];
        _itemImages = new Image[menuItems.Count];
        _itemOriginalColors = new Color[menuItems.Count];

        // Stocker l'échelle originale de la flèche
        if (arrowIndicator != null)
        {
            _arrowOriginalScale = arrowIndicator.localScale;
        }

        foreach (var binding in midiBindings)
        {
            MidiBinding.Instance.Subscribe(binding, OnMidiTrigger);
        }

        // Assurez-vous d'avoir un CanvasGroup
        if (menuCanvasGroup == null)
        {
            menuCanvasGroup = GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        
        // Stocker les références aux images et leurs couleurs originales
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (menuItems[i].itemTransform != null)
            {
                _itemImages[i] = menuItems[i].itemTransform.GetComponent<Image>();
                if (_itemImages[i] != null)
                {
                    _itemOriginalColors[i] = _itemImages[i].color;
                }
            }
        }

        CalculateItemPositions();
        StoreOriginalScales();

        // Cacher le menu au démarrage
        SetMenuVisibility(false);

        // Désactiver l'indicateur
        if (arrowIndicator != null)
        {
            arrowIndicator.gameObject.SetActive(false);
            arrowIndicator.localScale = Vector3.zero; // Commencer avec une échelle de zéro pour l'animation
        }
    }

    private void OnMidiTrigger(float value)
    {
        if (value == 0)
        {
            ToggleMenu();
        }
    }
    
    private void StoreOriginalScales()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (menuItems[i].itemTransform != null)
            {
                _itemOriginalScales[i] = menuItems[i].itemTransform.localScale;
            }
            else
            {
                _itemOriginalScales[i] = Vector3.one;
            }
        }
    }

    private void Update()
    {
        // Toggle du menu avec la touche configurée
        if (Input.GetKeyDown(toggleKey) && Time.unscaledTime - _lastToggleTime > 0.2f)
        {
            _lastToggleTime = Time.unscaledTime;
            ToggleMenu();
        }

        // Gérer la sélection si le menu est ouvert
        if (_isOpen && !_isAnimating)
        {
            HandleSelection();
        }
    }

    private void CalculateItemPositions()
    {
        int count = menuItems.Count;
        if (count <= 0) return;

        float angleStep = 360f / count;
        float directionMultiplier = clockwiseDistribution ? -1f : 1f;

        for (int i = 0; i < count; i++)
        {
            // Calculer l'angle avec le décalage et la direction
            float angle = (startAngle + i * angleStep * directionMultiplier) % 360f;
            float angleRad = angle * Mathf.Deg2Rad;

            // Calculer la position sur le cercle
            float x = Mathf.Sin(angleRad) * menuRadius;
            float y = Mathf.Cos(angleRad) * menuRadius;

            // Stocker la position
            _itemTargetPositions[i] = new Vector2(x, y);

            // Positionner l'élément au centre (position fermée)
            if (menuItems[i].itemTransform != null)
            {
                menuItems[i].itemTransform.localPosition = Vector3.zero;

                // S'assurer que le pivot est au centre (0.5, 0.5) pour un positionnement correct
                try
                {
                    menuItems[i].itemTransform.pivot = new Vector2(0.5f, 0.5f);
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Impossible de modifier le pivot de l'élément {menuItems[i].name}");
                }
            }
        }
    }

    public void ToggleMenu()
    {
        if (_isAnimating) return;

        if (_isOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (_isOpen || _isAnimating) return;

        _isOpen = true;
        _selectedIndex = -1; // Réinitialiser la sélection

        // Activer les éléments
        SetMenuVisibility(true);

        // Réinitialiser les échelles des éléments
        ResetItemScales();

        // Appliquer les états actifs/inactifs
        ApplyItemStates();

        // Déclencher les événements d'ouverture
        onMenuOpen?.Invoke();

        // Animer l'ouverture
        StartCoroutine(AnimateOpen());

        // Mettre le jeu en pause via le PauseManager
        pauseToken = PauseManager.Instance.AddPauseCommand("CircularMenu", 50);

        // Demander que le curseur soit visible
        cursorToken = CursorManager.Instance.AddCommand("CircularMenu", true, 50);

        Debug.Log("Menu opened");
    }

    public void CloseMenu()
    {
        if (!_isOpen || _isAnimating) return;

        _isOpen = false;

        // Réinitialiser les échelles des éléments
        ResetItemScales();

        // Déclencher les événements de fermeture
        onMenuClose?.Invoke();

        // Animer la fermeture
        StartCoroutine(AnimateClose());

        Debug.Log("Menu closed 1");

        // Reprendre le jeu via le PauseManager
        pauseToken?.Dispose();
        pauseToken = null;

        Debug.Log("Menu closed 2");

        // Libérer la commande de curseur
        cursorToken?.Dispose();
        cursorToken = null;

        Debug.Log("Menu closed");
    }

    private void ResetItemScales()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (menuItems[i].itemTransform != null)
            {
                menuItems[i].itemTransform.localScale = _itemOriginalScales[i];
            }
        }
    }

    private void ApplyItemStates()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (_itemImages[i] != null)
            {
                if (menuItems[i].isActive)
                {
                    // Élément actif
                    _itemImages[i].color = menuItems[i].activeColor;
                }
                else
                {
                    // Élément inactif (grisé)
                    Color inactiveColor = menuItems[i].inactiveColor;
                    inactiveColor.a = menuItems[i].inactiveAlpha;
                    _itemImages[i].color = inactiveColor;
                }
            }
        }
    }

    /// <summary>
    /// Définit l'état actif/inactif d'un élément du menu
    /// </summary>
    public void SetItemActive(int itemIndex, bool active)
    {
        if (itemIndex < 0 || itemIndex >= menuItems.Count)
            return;

        menuItems[itemIndex].isActive = active;

        // Mettre à jour l'apparence si le menu est ouvert
        if (_isOpen && _itemImages[itemIndex] != null)
        {
            if (active)
            {
                _itemImages[itemIndex].color = menuItems[itemIndex].activeColor;
            }
            else
            {
                Color inactiveColor = menuItems[itemIndex].inactiveColor;
                inactiveColor.a = menuItems[itemIndex].inactiveAlpha;
                _itemImages[itemIndex].color = inactiveColor;

                // Si cet élément était sélectionné, réinitialiser sa taille
                if (_selectedIndex == itemIndex)
                {
                    menuItems[itemIndex].itemTransform.localScale = _itemOriginalScales[itemIndex];
                    _selectedIndex = -1;
                }
            }
        }
    }

    private IEnumerator AnimateOpen()
    {
        _isAnimating = true;

        // Animer les éléments
        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / animationDuration);
            float smoothT = EaseOutBack(t);

            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].itemTransform != null)
                {
                    menuItems[i].itemTransform.localPosition =
                        Vector2.Lerp(Vector2.zero, _itemTargetPositions[i], smoothT);
                }
            }

            yield return null;
        }

        // Assurer les positions finales
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (menuItems[i].itemTransform != null)
            {
                menuItems[i].itemTransform.localPosition = _itemTargetPositions[i];
            }
        }

        // Animer l'apparition de la flèche avec DOTween
        AnimateArrowAppear();

        _isAnimating = false;
    }


    private IEnumerator AnimateClose()
    {
        _isAnimating = true;

        // Animer la disparition de la flèche avec DOTween
        AnimateArrowDisappear();

        // Attendre un peu pour que l'animation de la flèche commence
        yield return new WaitForSecondsRealtime(arrowAppearDuration * 0.5f);

        // Animer les éléments
        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / animationDuration);
            float smoothT = EaseInBack(t);

            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].itemTransform != null)
                {
                    menuItems[i].itemTransform.localPosition =
                        Vector2.Lerp(_itemTargetPositions[i], Vector2.zero, smoothT);
                }
            }

            yield return null;
        }

        // Assurer les positions finales
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (menuItems[i].itemTransform != null)
            {
                menuItems[i].itemTransform.localPosition = Vector2.zero;
            }
        }

        // Désactiver les éléments du menu, mais pas le background (il est géré par l'animation)
        foreach (var item in menuItems)
        {
            if (item.itemTransform != null)
            {
                item.itemTransform.gameObject.SetActive(false);
            }
        }

        // Restaurer les couleurs originales des éléments
        RestoreItemColors();

        _isAnimating = false;
    }


    private void RestoreItemColors()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (_itemImages[i] != null && i < _itemOriginalColors.Length)
            {
                _itemImages[i].color = _itemOriginalColors[i];
            }
        }
    }

    private void AnimateArrowAppear()
    {
        if (arrowIndicator == null) return;

        // Arrêter toute animation en cours
        arrowIndicator.DOKill();
        if (_arrowPulseSequence != null && _arrowPulseSequence.IsActive())
        {
            _arrowPulseSequence.Kill();
        }

        // Activer la flèche
        arrowIndicator.gameObject.SetActive(true);
        arrowIndicator.localScale = Vector3.zero;

        // Animer l'apparition
        arrowIndicator.DOScale(_arrowOriginalScale, arrowAppearDuration)
            .SetEase(arrowAppearEase)
            .SetUpdate(true) // Pour que l'animation fonctionne même si le temps est en pause
            .OnComplete(() =>
            {
                // Démarrer l'animation de pulsation
                StartArrowPulse();
            });
    }

    private void AnimateArrowDisappear()
    {
        if (arrowIndicator == null) return;

        // Arrêter toute animation en cours
        arrowIndicator.DOKill();
        if (_arrowPulseSequence != null && _arrowPulseSequence.IsActive())
        {
            _arrowPulseSequence.Kill();
        }

        // Animer la disparition
        arrowIndicator.DOScale(Vector3.zero, arrowAppearDuration)
            .SetEase(arrowDisappearEase)
            .SetUpdate(true) // Pour que l'animation fonctionne même si le temps est en pause
            .OnComplete(() => { arrowIndicator.gameObject.SetActive(false); });
    }

    private void StartArrowPulse()
    {
        if (arrowIndicator == null) return;

        // Créer une séquence de pulsation
        _arrowPulseSequence = DOTween.Sequence();

        // Ajouter les animations de pulsation
        _arrowPulseSequence.Append(arrowIndicator
            .DOScale(_arrowOriginalScale * (1 + arrowPulseMagnitude), arrowPulseSpeed * 0.5f)
            .SetEase(Ease.InOutSine));
        _arrowPulseSequence.Append(arrowIndicator.DOScale(_arrowOriginalScale, arrowPulseSpeed * 0.5f)
            .SetEase(Ease.InOutSine));

        // Configurer la séquence
        _arrowPulseSequence.SetLoops(-1) // Boucle infinie
            .SetUpdate(true); // Pour que l'animation fonctionne même si le temps est en pause
    }

    private void SetMenuVisibility(bool visible)
    {
        // Activer/désactiver les éléments du menu
        foreach (var item in menuItems)
        {
            if (item.itemTransform != null)
            {
                item.itemTransform.gameObject.SetActive(visible);
            }
        }
    
        // Contrôler l'interactivité via le CanvasGroup
        menuCanvasGroup.interactable = visible;
        menuCanvasGroup.blocksRaycasts = visible;
    }


    private void HandleSelection()
    {
        // Obtenir la direction du joystick ou de la souris
        Vector2 direction = GetInputDirection();

        if (direction.magnitude > 0.3f)
        {
            // Orienter la flèche vers la direction
            if (arrowIndicator != null)
            {
                float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                arrowIndicator.rotation = Quaternion.Euler(0, 0, -angle);
            }

            // Calculer l'angle de l'entrée
            float inputAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            if (inputAngle < 0) inputAngle += 360f;

            // Trouver l'élément le plus proche de cet angle
            int closestIndex = FindClosestActiveItemToAngle(inputAngle);

            // Si on a trouvé un élément actif et qu'il est différent de la sélection actuelle
            if (closestIndex >= 0 && closestIndex != _selectedIndex)
            {
                // Réinitialiser l'échelle de l'élément précédemment sélectionné
                if (_selectedIndex >= 0 && _selectedIndex < menuItems.Count &&
                    menuItems[_selectedIndex].itemTransform != null)
                {
                    menuItems[_selectedIndex].itemTransform.DOScale(_itemOriginalScales[_selectedIndex], 0.2f)
                        .SetEase(Ease.OutQuad)
                        .SetUpdate(true);
                }

                _selectedIndex = closestIndex;

                // Agrandir l'élément sélectionné avec DOTween seulement s'il est actif
                if (menuItems[_selectedIndex].isActive && menuItems[_selectedIndex].itemTransform != null)
                {
                    menuItems[_selectedIndex].itemTransform
                        .DOScale(_itemOriginalScales[_selectedIndex] * selectedItemScale, 0.2f)
                        .SetEase(Ease.OutQuad)
                        .SetUpdate(true);
                }
            }
        }

        // Vérifier si l'utilisateur confirme la sélection
        if (_selectedIndex >= 0 && (Input.GetButtonDown("Submit") || Input.GetMouseButtonDown(0)))
        {
            // Exécuter l'action associée à l'élément seulement s'il est actif
            if (menuItems[_selectedIndex].isActive)
            {
                menuItems[_selectedIndex].onSelect?.Invoke();

                // Fermer le menu si l'option est activée
                if (closeMenuOnSelection)
                {
                    CloseMenu();
                }
            }
        }
    }

    /// <summary>
    /// Trouve l'élément actif le plus proche d'un angle donné
    /// </summary>
    private int FindClosestActiveItemToAngle(float inputAngle)
    {
        int closestIndex = -1;
        float closestDist = float.MaxValue;

        for (int i = 0; i < menuItems.Count; i++)
        {
            // Ne considérer que les éléments actifs
            if (!menuItems[i].isActive)
                continue;

            float itemAngle = Mathf.Atan2(_itemTargetPositions[i].x, _itemTargetPositions[i].y) * Mathf.Rad2Deg;
            if (itemAngle < 0) itemAngle += 360f;

            float angleDist = Mathf.Abs(Mathf.DeltaAngle(inputAngle, itemAngle));

            if (angleDist < closestDist)
            {
                closestDist = angleDist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private Vector2 GetInputDirection()
    {
        // Obtenir la direction depuis le joystick/clavier
        Vector2 keyboardDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Obtenir la direction depuis la souris
        Vector2 mousePos = Input.mousePosition;
        Vector2 centerPos = RectTransformUtility.WorldToScreenPoint(null, menuCenter.position);
        Vector2 mouseDir = (mousePos - centerPos).normalized;

        // Utiliser la direction avec la plus grande magnitude
        return keyboardDir.magnitude > 0.3f ? keyboardDir : mouseDir;
    }

    // Fonctions d'interpolation pour des animations plus fluides
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    private float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return c3 * t * t * t - c1 * t * t;
    }

    private void OnDestroy()
    {
        // Arrêter toutes les animations DOTween
        if (arrowIndicator != null)
        {
            arrowIndicator.DOKill();
        }

        if (_arrowPulseSequence != null && _arrowPulseSequence.IsActive())
        {
            _arrowPulseSequence.Kill();
        }

        foreach (var item in menuItems)
        {
            if (item.itemTransform != null)
            {
                item.itemTransform.DOKill();
            }
        }

        cursorToken?.Dispose();
        pauseToken?.Dispose();

        // Remettre le timeScale à 1 au cas où
        Time.timeScale = 1f;
    }
}