using System.Collections.Generic;
using UnityEngine;

public class EndUIPresets : MonoBehaviour
{
    [System.Serializable]
    private enum PresetType
    {
        Win,
        Lose
    }
    
    [System.Serializable]
    private class UIPreset
    {
        public PresetType type;
        public List<bool> gameObjectStates = new List<bool>();
    }
    
    [Header("Configuration")]
    public CircleFillerUI circleFillerUI;
    [Range(0, 100)]
    public float winThreshold = 50f; // Seuil pour déterminer Win/Lose
    
    [Header("GameObjects à contrôler")]
    public List<GameObject> controlledObjects = new List<GameObject>();
    
    [Header("Presets")]
    [SerializeField] private List<UIPreset> presets = new List<UIPreset>();
    
    [Header("Test en direct")]
    [Range(0, 100)]
    public float currentPercentage = 0f;
    
    private float lastPercentage = -1f;
    
    private void Start()
    {
        // Vérifier les références
        if (circleFillerUI == null)
        {
            circleFillerUI = GetComponentInChildren<CircleFillerUI>();
            if (circleFillerUI == null)
            {
                Debug.LogError("EndUIPresets nécessite une référence à CircleFillerUI.");
                return;
            }
        }
        
        // Vérifier si les presets existent déjà
        if (presets.Count == 0)
        {
            // Seulement initialiser les presets s'ils n'existent pas
            InitializeDefaultPresets();
        }
        else
        {
            // Valider les presets existants
            ValidatePresets();
        }
        
        // Appliquer le preset initial
        ApplyPresetByPercentage(currentPercentage);
        lastPercentage = currentPercentage;
    }
    
    // Valider que les presets existants ont le bon nombre d'états
    private void ValidatePresets()
    {
        foreach (UIPreset preset in presets)
        {
            // Ajuster la taille de la liste d'états si nécessaire
            while (preset.gameObjectStates.Count < controlledObjects.Count)
            {
                preset.gameObjectStates.Add(false);
            }
        }
    }
    
    // Initialiser les presets par défaut (seulement si aucun preset n'existe)
    private void InitializeDefaultPresets()
    {
        // Vérifier si les presets Win et Lose existent déjà
        bool hasWinPreset = presets.Exists(p => p.type == PresetType.Win);
        bool hasLosePreset = presets.Exists(p => p.type == PresetType.Lose);
        
        // Créer le preset Lose s'il n'existe pas
        if (!hasLosePreset)
        {
            UIPreset losePreset = new UIPreset { type = PresetType.Lose };
            
            // Ajouter des états par défaut pour chaque GameObject
            for (int i = 0; i < controlledObjects.Count; i++)
            {
                // Pour le preset Lose, activer les objets 0, 1, 2 par défaut
                losePreset.gameObjectStates.Add(i < 3);
            }
            
            presets.Add(losePreset);
        }
        
        // Créer le preset Win s'il n'existe pas
        if (!hasWinPreset)
        {
            UIPreset winPreset = new UIPreset { type = PresetType.Win };
            
            // Ajouter des états par défaut pour chaque GameObject
            for (int i = 0; i < controlledObjects.Count; i++)
            {
                // Pour le preset Win, activer les objets 0, 1, 3, 4 par défaut
                winPreset.gameObjectStates.Add(i == 0 || i == 1 || i == 3 || i == 4);
            }
            
            presets.Add(winPreset);
        }
    }
    
    // Méthode principale: appliquer le preset en fonction du pourcentage
    public void ApplyPresetByPercentage(float percentage)
    {
        // Mettre à jour le pourcentage
        currentPercentage = Mathf.Clamp(percentage, 0f, 100f);
        
        // Déterminer le type de preset
        PresetType type = currentPercentage >= winThreshold ? PresetType.Win : PresetType.Lose;
        
        // Trouver le preset correspondant
        UIPreset preset = presets.Find(p => p.type == type);
        
        if (preset != null)
        {
            // Mettre à jour le cercle
            circleFillerUI.SetTargetPercentage(currentPercentage);
            
            // Activer/désactiver les GameObjects selon le preset
            for (int i = 0; i < controlledObjects.Count; i++)
            {
                if (i < preset.gameObjectStates.Count && controlledObjects[i] != null)
                {
                    controlledObjects[i].SetActive(preset.gameObjectStates[i]);
                }
            }
        }
    }
    
    // Mise à jour pour tester en temps réel
    private void Update()
    {
        // Appliquer les changements si le pourcentage a changé
        if (lastPercentage != currentPercentage)
        {
            ApplyPresetByPercentage(currentPercentage);
            lastPercentage = currentPercentage;
        }
    }
    
    // Validation en mode éditeur
    private void OnValidate()
    {
        // S'assurer que les presets existent
        if (presets.Count == 0)
        {
            InitializeDefaultPresets();
        }
        else
        {
            ValidatePresets();
        }
    }
}
