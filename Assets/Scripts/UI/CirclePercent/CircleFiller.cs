using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    
    [Header("Contrôle dynamique")]
    [Range(0f, 100f)]
    [SerializeField] private float targetPercentage = 0f;
    [SerializeField] private float currentPercentage = 0f; // Pour affichage dans l'inspecteur
    
    private List<Image> activeImages = new List<Image>();
    private float lastTargetPercentage = -1f; // Pour détecter les changements
    private float lastRadius = -1f; // Pour détecter les changements de rayon
    
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
        // Nettoyer les segments actifs
        ClearActiveSegments();
        
        // Trier les segments par pourcentage décroissant pour optimiser
        List<CircleSegment> sortedSegments = new List<CircleSegment>(availableSegments);
        sortedSegments.Sort((a, b) => b.percentage.CompareTo(a.percentage));
        
        // Utiliser un algorithme glouton pour trouver la meilleure combinaison
        List<CircleSegment> bestCombination = FindBestCombination(sortedSegments, percentage);
        
        // Placer les segments sélectionnés autour du cercle
        PlaceSegmentsInCircle(bestCombination);
        
        // Mettre à jour le pourcentage actuel
        currentPercentage = bestCombination.Sum(s => s.percentage);
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
        // Trouver le pourcentage maximum pour normaliser les tailles
        float maxPercentage = availableSegments.Max(s => s.percentage);
        float currentAngle = 0f;
        
        foreach (var segment in segments)
        {
            // Calculer l'angle que ce segment occupe dans le cercle
            float segmentAngle = (segment.percentage / 100f) * 360f;
            
            // Calculer la position sur le cercle
            float middleAngle = currentAngle + segmentAngle / 2f;
            float radians = middleAngle * Mathf.Deg2Rad;
            Vector2 position = new Vector2(
                circleCenter.anchoredPosition.x + circleRadius * Mathf.Cos(radians),
                circleCenter.anchoredPosition.y + circleRadius * Mathf.Sin(radians)
            );
            
            // Créer un GameObject avec une Image UI
            GameObject imageObj = new GameObject($"Segment_{segment.percentage}%");
            imageObj.transform.SetParent(parent.transform, false);
            
            // Ajouter et configurer le RectTransform
            RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            
            // Ajouter et configurer l'Image
            Image image = imageObj.AddComponent<Image>();
            image.sprite = segment.sprite;
            image.preserveAspect = true; // Préserver le ratio d'aspect du sprite
            
            // Calculer la taille de l'image proportionnellement à son pourcentage
            // Toutes les images auront une taille relative à leur pourcentage
            float sizeRatio = segment.percentage / maxPercentage;
            float imageSize = baseImageSize * sizeRatio;
            rectTransform.sizeDelta = new Vector2(imageSize, imageSize);
            
            // Orienter l'image correctement (face vers l'extérieur du cercle)
            Vector2 directionFromCenter = (position - circleCenter.anchoredPosition).normalized;
            float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 pour orienter correctement
            
            // Ajouter aux segments actifs
            activeImages.Add(image);
            
            // Mettre à jour l'angle pour le prochain segment
            currentAngle += segmentAngle;
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
}
