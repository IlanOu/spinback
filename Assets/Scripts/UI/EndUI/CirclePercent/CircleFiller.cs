using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;

public class CircleFillerUI : MonoBehaviour
{
    [System.Serializable]
    public class CircleSegment
    {
        public Sprite sprite;
        public float percentage; // Pourcentage du cercle que cette image représente
    }

    [Header("Configuration")]
    public List<CircleSegment> availableSegments = new List<CircleSegment>();
    public RectTransform circleCenter; // Référence au centre du cercle (RectTransform)
    public float circleRadius = 200f; // Rayon en pixels pour l'UI
    public float baseImageSize = 50f; // Taille de base pour les images (en pixels)
    public RectTransform parent; // Parent où les images seront créées
    public TextMeshProUGUI percentageText; // Texte pour afficher le pourcentage
    
    [Header("Animation")]
    public float animationDuration = 0.5f; // Durée d'animation pour chaque image
    public float delayBetweenImages = 0.1f; // Délai entre l'animation de chaque image
    public Ease animationEase = Ease.OutBack;
    public bool animatePercentageText = true;
    public bool clockwiseDirection = true; // Direction du cercle (true = sens horaire, false = sens anti-horaire)
    public bool clockwiseAnimation = true; // Direction de l'animation (true = sens horaire, false = sens anti-horaire)
    
    [Header("Contrôle dynamique")]
    [Range(0f, 100f)]
    public float targetPercentage = 0f;
    [SerializeField] private float currentPercentage = 0f; // Pour affichage dans l'inspecteur
    
    private List<Image> activeImages = new List<Image>();
    private float lastTargetPercentage = -1f; // Pour détecter les changements
    private float lastRadius = -1f; // Pour détecter les changements de rayon
    private Sequence currentAnimation;
    
    private void Start()
    {
        // S'assurer qu'on a un parent
        if (parent == null)
        {
            parent = GetComponent<RectTransform>();
            if (parent == null)
            {
                Debug.LogError("CircleFillerUI nécessite un RectTransform parent.");
                return;
            }
        }
        
        // Initialiser le texte du pourcentage
        UpdatePercentageText(0, false);
        
        // Appliquer le pourcentage initial
        FillCircle(targetPercentage);
        lastTargetPercentage = targetPercentage;
        lastRadius = circleRadius;
    }
    
    private void Update()
    {
        // Vérifier si le pourcentage cible ou le rayon a changé
        if (targetPercentage != lastTargetPercentage || circleRadius != lastRadius)
        {
            FillCircle(targetPercentage);
            lastTargetPercentage = targetPercentage;
            lastRadius = circleRadius;
        }
    }
    
    // Fonction principale pour remplir le cercle à un pourcentage donné
    public void FillCircle(float percentage)
    {
        // Arrêter l'animation en cours si elle existe
        if (currentAnimation != null && currentAnimation.IsActive())
        {
            currentAnimation.Kill();
        }
        
        // Nettoyer les segments actifs
        ClearActiveSegments();
        
        // Trier les segments par pourcentage décroissant pour optimiser
        List<CircleSegment> sortedSegments = new List<CircleSegment>(availableSegments);
        sortedSegments.Sort((a, b) => b.percentage.CompareTo(a.percentage));
        
        // Utiliser un algorithme glouton pour trouver la meilleure combinaison
        List<CircleSegment> bestCombination = FindBestCombination(sortedSegments, percentage);
        
        // Placer les segments sélectionnés autour du cercle avec animation
        PlaceSegmentsInCircle(bestCombination);
        
        // Mettre à jour le pourcentage actuel
        float newPercentage = bestCombination.Sum(s => s.percentage);
        UpdatePercentageText(newPercentage, true);
        currentPercentage = newPercentage;
        
        Debug.Log($"Pourcentage cible: {percentage}%, Pourcentage atteint: {currentPercentage}%");
    }
    
    private List<CircleSegment> FindBestCombination(List<CircleSegment> segments, float targetPercentage)
    {
        // Utiliser un algorithme de programmation dynamique pour résoudre ce problème
        // Similaire au problème du sac à dos (knapsack problem)
        
        // Pour simplifier, commençons par une approche gloutonne
        List<CircleSegment> result = new List<CircleSegment>();
        float currentSum = 0f;
        bool[] used = new bool[segments.Count];
        
        while (currentSum < targetPercentage)
        {
            float bestDiff = float.MaxValue;
            int bestIndex = -1;
            
            // Trouver le meilleur segment à ajouter
            for (int i = 0; i < segments.Count; i++)
            {
                if (!used[i])
                {
                    float newSum = currentSum + segments[i].percentage;
                    float diff = Mathf.Abs(targetPercentage - newSum);
                    
                    // Si ajouter ce segment nous rapproche du pourcentage cible
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestIndex = i;
                    }
                }
            }
            
            // Si on ne peut plus améliorer, on s'arrête
            if (bestIndex == -1)
                break;
                
            // Ajouter le meilleur segment
            result.Add(segments[bestIndex]);
            currentSum += segments[bestIndex].percentage;
            used[bestIndex] = true;
            
            // Si on dépasse le pourcentage cible, vérifier si on était plus proche avant
            if (currentSum > targetPercentage)
            {
                float diffWithSegment = currentSum - targetPercentage;
                float diffWithoutSegment = targetPercentage - (currentSum - segments[bestIndex].percentage);
                
                // Si on était plus proche sans ce dernier segment, on le retire
                if (diffWithoutSegment < diffWithSegment)
                {
                    result.RemoveAt(result.Count - 1);
                    currentSum -= segments[bestIndex].percentage;
                }
                
                break;
            }
        }
        
        return result;
    }
    
    private void PlaceSegmentsInCircle(List<CircleSegment> segments)
    {
        // Créer une nouvelle séquence d'animation
        currentAnimation = DOTween.Sequence();
        
        // Trouver le pourcentage maximum pour normaliser les tailles
        float maxPercentage = availableSegments.Max(s => s.percentage);
        float currentAngle = 0f;
        
        // Préparer les informations pour chaque segment
        List<SegmentInfo> segmentInfos = new List<SegmentInfo>();
        
        foreach (var segment in segments)
        {
            // Calculer l'angle que ce segment occupe dans le cercle
            float segmentAngle = (segment.percentage / 100f) * 360f;
            
            // Ajuster l'angle de départ en fonction du sens (horaire ou anti-horaire)
            float middleAngle;
            if (clockwiseDirection)
            {
                // Sens horaire: commence à 90° (haut) et tourne vers la droite
                middleAngle = 90f - (currentAngle + segmentAngle / 2f);
            }
            else
            {
                // Sens anti-horaire: commence à 90° (haut) et tourne vers la gauche
                middleAngle = 90f + (currentAngle + segmentAngle / 2f);
            }
            
            float radians = middleAngle * Mathf.Deg2Rad;
            Vector2 position = new Vector2(
                circleCenter.anchoredPosition.x + circleRadius * Mathf.Cos(radians),
                circleCenter.anchoredPosition.y + circleRadius * Mathf.Sin(radians)
            );
            
            // Calculer la taille de l'image proportionnellement à son pourcentage
            float sizeRatio = segment.percentage / maxPercentage;
            float imageSize = baseImageSize * sizeRatio;
            
            // Calculer l'angle pour l'orientation
            Vector2 directionFromCenter = (position - circleCenter.anchoredPosition).normalized;
            float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;
            
            // Stocker les informations du segment
            segmentInfos.Add(new SegmentInfo {
                Segment = segment,
                Position = position,
                Angle = angle,
                ImageSize = imageSize,
                StartAngle = currentAngle
            });
            
            // Mettre à jour l'angle pour le prochain segment
            currentAngle += segmentAngle;
        }
        
        // Trier les segments pour l'animation selon la direction choisie
        if (clockwiseAnimation)
        {
            segmentInfos.Sort((a, b) => a.StartAngle.CompareTo(b.StartAngle));
        }
        else
        {
            segmentInfos.Sort((a, b) => b.StartAngle.CompareTo(a.StartAngle));
        }
        
        // Créer et animer chaque segment un par un
        float delay = 0f;
        
        foreach (var info in segmentInfos)
        {
            // Créer un GameObject avec une Image UI
            GameObject imageObj = new GameObject($"Segment_{info.Segment.percentage}%");
            imageObj.transform.SetParent(parent.transform, false);
            
            // Ajouter et configurer le RectTransform
            RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = info.Position;
            
            // Ajouter et configurer l'Image
            Image image = imageObj.AddComponent<Image>();
            image.sprite = info.Segment.sprite;
            image.preserveAspect = true; // Préserver le ratio d'aspect du sprite
            
            // Configurer l'image avec une taille initiale de zéro pour l'animation
            rectTransform.sizeDelta = Vector2.zero;
            
            // Orienter l'image correctement (face vers l'extérieur du cercle)
            rectTransform.rotation = Quaternion.Euler(0, 0, info.Angle - 90); // -90 pour orienter correctement
            
            // Ajouter l'animation à la séquence avec un délai croissant
            currentAnimation.Insert(delay, rectTransform.DOSizeDelta(new Vector2(info.ImageSize, info.ImageSize), animationDuration)
                .SetEase(animationEase));
            
            // Ajouter aux segments actifs
            activeImages.Add(image);
            
            // Augmenter le délai pour le prochain segment
            delay += delayBetweenImages;
        }
        
        // Jouer la séquence d'animation
        currentAnimation.Play();
    }
    
    private class SegmentInfo
    {
        public CircleSegment Segment;
        public Vector2 Position;
        public float Angle;
        public float ImageSize;
        public float StartAngle;
    }
    
    private void UpdatePercentageText(float percentage, bool animate)
    {
        if (percentageText != null)
        {
            if (animate && animatePercentageText)
            {
                // Animer le texte du pourcentage
                float startValue = currentPercentage;
                DOTween.To(() => startValue, x => {
                    percentageText.text = $"{Mathf.Round(x)}%";
                }, percentage, animationDuration)
                .SetEase(animationEase);
            }
            else
            {
                // Mettre à jour le texte sans animation
                percentageText.text = $"{Mathf.Round(percentage)}%";
            }
        }
    }
    
    private void ClearActiveSegments()
    {
        foreach (var image in activeImages)
        {
            if (image != null)
            {
                Destroy(image.gameObject);
            }
        }
        
        activeImages.Clear();
    }
    
    // Pour permettre de mettre à jour depuis d'autres scripts
    public void SetTargetPercentage(float percentage)
    {
        targetPercentage = Mathf.Clamp(percentage, 0f, 100f);
    }
    
    // Pour obtenir le pourcentage actuel réellement affiché
    public float GetCurrentPercentage()
    {
        return currentPercentage;
    }
    
    // Pour changer la direction du cercle
    public void SetClockwiseDirection(bool clockwise)
    {
        clockwiseDirection = clockwise;
        // Mettre à jour le cercle si nécessaire
        if (targetPercentage > 0)
        {
            FillCircle(targetPercentage);
        }
    }
    
    // Pour changer la direction de l'animation
    public void SetClockwiseAnimation(bool clockwise)
    {
        clockwiseAnimation = clockwise;
    }
}
