using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

/// <summary>
/// Gestionnaire de pause centralisé pour les timelines et autres éléments temporels
/// </summary>
public class PauseManager : MonoBehaviour
{
    // Singleton
    private static PauseManager _instance;
    public static PauseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PauseManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PauseManager");
                    _instance = go.AddComponent<PauseManager>();
                }
            }
            return _instance;
        }
    }

    // Événement pour notifier des changements d'état de pause
    public UnityEvent<bool> OnPauseStateChanged;

    // Liste des commandes de pause actives
    private Dictionary<string, PauseCommand> activePauseCommands = new Dictionary<string, PauseCommand>();

    // État de pause actuel
    private bool isPaused = false;

    // Liste des PlayableDirectors à contrôler
    [SerializeField] private List<PlayableDirector> managedDirectors = new List<PlayableDirector>();
    
    // Liste des objets implémentant IPausable (interface personnalisée)
    private List<IPausable> pausableObjects = new List<IPausable>();

    private void Awake()
    {
        // S'assurer qu'il n'y a qu'une seule instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Trouver tous les PlayableDirectors dans la scène si la liste est vide
        if (managedDirectors.Count == 0)
        {
            managedDirectors.AddRange(FindObjectsOfType<PlayableDirector>());
        }
        
        // Trouver tous les objets implémentant IPausable
        FindPausableObjects();
    }

    /// <summary>
    /// Trouve tous les objets qui implémentent l'interface IPausable
    /// </summary>
    public void FindPausableObjects()
    {
        pausableObjects.Clear();
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        
        foreach (MonoBehaviour mb in allMonoBehaviours)
        {
            if (mb is IPausable pausable)
            {
                pausableObjects.Add(pausable);
            }
        }
        
        Debug.Log($"Found {pausableObjects.Count} pausable objects");
    }

    /// <summary>
    /// Ajoute un PlayableDirector à la liste des directeurs gérés
    /// </summary>
    public void AddManagedDirector(PlayableDirector director)
    {
        if (!managedDirectors.Contains(director))
        {
            managedDirectors.Add(director);
        }
    }

    /// <summary>
    /// Ajoute un objet implémentant IPausable à la liste
    /// </summary>
    public void AddPausableObject(IPausable pausable)
    {
        if (!pausableObjects.Contains(pausable))
        {
            pausableObjects.Add(pausable);
        }
    }

    /// <summary>
    /// Met à jour l'état de pause en fonction des commandes actives
    /// </summary>
    private void UpdatePauseState()
    {
        bool shouldPause = activePauseCommands.Count > 0;
        
        if (shouldPause != isPaused)
        {
            isPaused = shouldPause;
            
            // Mettre en pause ou reprendre tous les directeurs
            foreach (var director in managedDirectors)
            {
                if (director != null)
                {
                    if (isPaused)
                    {
                        director.Pause();
                    }
                    else
                    {
                        director.Resume();
                    }
                }
            }
            
            // Mettre en pause ou reprendre tous les objets pausables
            foreach (var pausable in pausableObjects)
            {
                if (isPaused)
                {
                    pausable.Pause();
                }
                else
                {
                    pausable.Resume();
                }
            }
            
            Debug.Log($"Game pause state changed to: {isPaused}");
            
            // Notifier les observateurs
            OnPauseStateChanged?.Invoke(isPaused);
        }
    }

    /// <summary>
    /// Ajoute une commande pour mettre le jeu en pause
    /// </summary>
    /// <param name="commandId">Identifiant unique de la commande</param>
    /// <param name="priority">Priorité de la commande (plus élevé = plus prioritaire)</param>
    /// <returns>Un token pour annuler la commande</returns>
    public PauseCommandToken AddPauseCommand(string commandId, int priority = 0)
    {
        // Créer une nouvelle commande
        PauseCommand command = new PauseCommand
        {
            Id = commandId,
            Priority = priority
        };
        
        // Ajouter ou remplacer la commande
        activePauseCommands[commandId] = command;
        
        // Mettre à jour l'état de pause
        UpdatePauseState();
        
        Debug.Log($"Pause command added: {commandId} (Priority: {priority}). Active commands: {activePauseCommands.Count}");
        
        // Retourner un token pour annuler la commande
        return new PauseCommandToken(this, commandId);
    }

    /// <summary>
    /// Supprime une commande de pause par son ID
    /// </summary>
    public void RemovePauseCommand(string commandId)
    {
        if (activePauseCommands.ContainsKey(commandId))
        {
            activePauseCommands.Remove(commandId);
            Debug.Log($"Pause command removed: {commandId}. Remaining commands: {activePauseCommands.Count}");
            UpdatePauseState();
        }
        else
        {
            Debug.LogWarning($"Attempted to remove non-existent pause command: {commandId}");
        }
    }

    /// <summary>
    /// Vérifie si le jeu est actuellement en pause
    /// </summary>
    public bool IsGamePaused()
    {
        return isPaused;
    }

    /// <summary>
    /// Méthode de secours pour s'assurer que le jeu n'est pas bloqué en pause
    /// </summary>
    public void ForceResumeGame()
    {
        activePauseCommands.Clear();
        
        // Reprendre tous les directeurs
        foreach (var director in managedDirectors)
        {
            if (director != null)
            {
                director.Resume();
            }
        }
        
        // Reprendre tous les objets pausables
        foreach (var pausable in pausableObjects)
        {
            pausable.Resume();
        }
        
        isPaused = false;
        OnPauseStateChanged?.Invoke(isPaused);
        
        Debug.Log("Game forcibly resumed");
    }
    
    /// <summary>
    /// Affiche les commandes de pause actives (pour le débogage)
    /// </summary>
    public void LogActiveCommands()
    {
        Debug.Log($"=== Active Pause Commands ({activePauseCommands.Count}) ===");
        foreach (var command in activePauseCommands.Values)
        {
            Debug.Log($"ID: {command.Id}, Priority: {command.Priority}");
        }
        Debug.Log("===============================");
    }
}

/// <summary>
/// Structure représentant une commande de pause
/// </summary>
public struct PauseCommand
{
    public string Id;
    public int Priority;
}

/// <summary>
/// Token permettant d'annuler une commande de pause
/// </summary>
public class PauseCommandToken : IDisposable
{
    private PauseManager manager;
    private string commandId;
    private bool isDisposed = false;

    public PauseCommandToken(PauseManager manager, string commandId)
    {
        this.manager = manager;
        this.commandId = commandId;
    }

    public void Dispose()
    {
        if (!isDisposed && manager != null)
        {
            Debug.Log($"Disposing pause command token: {commandId}");
            manager.RemovePauseCommand(commandId);
            isDisposed = true;
        }
    }
    
    ~PauseCommandToken()
    {
        if (!isDisposed)
        {
            Debug.LogWarning($"PauseCommandToken for {commandId} was not properly disposed!");
            Dispose();
        }
    }
}

/// <summary>
/// Interface pour les objets qui peuvent être mis en pause
/// </summary>
public interface IPausable
{
    void Pause();
    void Resume();
}
