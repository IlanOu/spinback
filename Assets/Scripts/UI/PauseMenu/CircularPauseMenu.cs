using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class CircularMenu : MonoBehaviour
{
    [Header("Menu Configuration")]
    [SerializeField] private Transform menuCenter;
    [SerializeField] private float menuRadius = 200f;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    
    [Header("Menu Items")]
    [SerializeField] private List<MenuItem> menuItems = new List<MenuItem>();
    
    [Header("Visual Elements")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color backgroundActiveColor = new Color(0, 0, 0, 0.7f);
    [SerializeField] private Transform arrowIndicator; // Flèche au centre
    [SerializeField] private float selectedItemScale = 1.2f; // Échelle de l'élément sélectionné
    
    [System.Serializable]
    public class MenuItem
    {
        public string name;
        public RectTransform itemTransform;
        public UnityEvent onSelect;
    }
    
    private bool _isOpen = false;
    private int _selectedIndex = -1;
    private Vector2[] _itemTargetPositions;
    private bool _isAnimating = false;
    private float _lastToggleTime = 0f;
    private Vector3[] _itemOriginalScales; // Échelles originales des éléments
    
    private void Start()
    {
        // Initialiser les positions cibles et les échelles
        _itemTargetPositions = new Vector2[menuItems.Count];
        _itemOriginalScales = new Vector3[menuItems.Count];
        
        CalculateItemPositions();
        StoreOriginalScales();
        
        // Cacher le menu au démarrage
        SetMenuVisibility(false);
        
        // Désactiver l'indicateur
        if (arrowIndicator != null)
        {
            arrowIndicator.gameObject.SetActive(false);
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
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
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
        
        // Animer l'ouverture
        StartCoroutine(AnimateOpen());
        
        // Mettre le jeu en pause
        Time.timeScale = 0f;
        
        Debug.Log("Menu opened");
    }
    
    public void CloseMenu()
    {
        if (!_isOpen || _isAnimating) return;
        
        _isOpen = false;
        
        // Réinitialiser les échelles des éléments
        ResetItemScales();
        
        // Cacher l'indicateur
        if (arrowIndicator != null)
        {
            arrowIndicator.gameObject.SetActive(false);
        }
        
        // Animer la fermeture
        StartCoroutine(AnimateClose());
        
        // Reprendre le jeu
        Time.timeScale = 1f;
        
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
    
    private IEnumerator AnimateOpen()
    {
        _isAnimating = true;
        
        // Animer le fond
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(backgroundActiveColor.r, backgroundActiveColor.g, backgroundActiveColor.b, 0f);
            StartCoroutine(FadeBackground(0f, backgroundActiveColor.a, animationDuration));
        }
        
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
                    menuItems[i].itemTransform.localPosition = Vector2.Lerp(Vector2.zero, _itemTargetPositions[i], smoothT);
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
        
        // Activer l'indicateur
        if (arrowIndicator != null)
        {
            arrowIndicator.gameObject.SetActive(true);
        }
        
        _isAnimating = false;
    }
    
    private IEnumerator AnimateClose()
    {
        _isAnimating = true;
        
        // Animer le fond
        if (backgroundImage != null)
        {
            StartCoroutine(FadeBackground(backgroundImage.color.a, 0f, animationDuration));
        }
        
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
                    menuItems[i].itemTransform.localPosition = Vector2.Lerp(_itemTargetPositions[i], Vector2.zero, smoothT);
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
        
        // Désactiver les éléments
        SetMenuVisibility(false);
        
        _isAnimating = false;
    }
    
    private IEnumerator FadeBackground(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        Color startColor = new Color(backgroundActiveColor.r, backgroundActiveColor.g, backgroundActiveColor.b, startAlpha);
        Color endColor = new Color(backgroundActiveColor.r, backgroundActiveColor.g, backgroundActiveColor.b, endAlpha);
        
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            
            backgroundImage.color = Color.Lerp(startColor, endColor, t);
            
            yield return null;
        }
        
        backgroundImage.color = endColor;
    }
    
    private void SetMenuVisibility(bool visible)
    {
        // Activer/désactiver le fond
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(visible);
        }
        
        // Activer/désactiver les éléments du menu
        foreach (var item in menuItems)
        {
            if (item.itemTransform != null)
            {
                item.itemTransform.gameObject.SetActive(visible);
            }
        }
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
            int closestIndex = -1;
            float closestDist = float.MaxValue;
            
            for (int i = 0; i < menuItems.Count; i++)
            {
                float itemAngle = Mathf.Atan2(_itemTargetPositions[i].x, _itemTargetPositions[i].y) * Mathf.Rad2Deg;
                if (itemAngle < 0) itemAngle += 360f;
                
                float angleDist = Mathf.Abs(Mathf.DeltaAngle(inputAngle, itemAngle));
                
                if (angleDist < closestDist)
                {
                    closestDist = angleDist;
                    closestIndex = i;
                }
            }
            
            // Si on a trouvé un élément et qu'il est différent de la sélection actuelle
            if (closestIndex >= 0 && closestIndex != _selectedIndex)
            {
                // Réinitialiser l'échelle de l'élément précédemment sélectionné
                if (_selectedIndex >= 0 && _selectedIndex < menuItems.Count && menuItems[_selectedIndex].itemTransform != null)
                {
                    menuItems[_selectedIndex].itemTransform.localScale = _itemOriginalScales[_selectedIndex];
                }
                
                _selectedIndex = closestIndex;
                
                // Agrandir l'élément sélectionné
                if (menuItems[_selectedIndex].itemTransform != null)
                {
                    menuItems[_selectedIndex].itemTransform.localScale = _itemOriginalScales[_selectedIndex] * selectedItemScale;
                }
            }
        }
        
        // Vérifier si l'utilisateur confirme la sélection
        if (_selectedIndex >= 0 && (Input.GetButtonDown("Submit") || Input.GetMouseButtonDown(0)))
        {
            // Exécuter l'action associée à l'élément
            menuItems[_selectedIndex].onSelect?.Invoke();
            
            // Fermer le menu
            CloseMenu();
        }
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
        // Remettre le timeScale à 1 au cas où
        Time.timeScale = 1f;
    }
    
    // Méthode publique pour tester le menu
    public void ForceOpenMenu()
    {
        if (!_isOpen && !_isAnimating)
        {
            OpenMenu();
        }
    }
}
