using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Gestionnaire de curseur utilisant un pattern Singleton et Command
/// </summary>
public class CursorManager : MonoBehaviour
{
    // Singleton
    private static CursorManager _instance;
    public static CursorManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CursorManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CursorManager");
                    _instance = go.AddComponent<CursorManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Paramètres par défaut")]
    [SerializeField] private bool hideCursorOnStart = true;
    [SerializeField] private KeyCode toggleCursorKey = KeyCode.Tab;
    [SerializeField] private KeyCode showCursorKey = KeyCode.Escape;
    [SerializeField] private bool hideOnClick = true;

    // Événement pour notifier des changements d'état
    public event Action<bool> OnCursorVisibilityChanged;

    // État initial du curseur
    private bool cursorWasVisible;
    private CursorLockMode previousLockMode;
    
    // État par défaut (quand aucune commande n'est active)
    private bool defaultCursorHidden;
    
    // Désactiver temporairement le hideOnClick
    private bool temporarilyDisableHideOnClick = false;

    // Liste des commandes actives, triées par priorité
    private SortedList<int, CursorCommand> activeCommands = new SortedList<int, CursorCommand>();

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
        
        // Sauvegarder l'état initial
        cursorWasVisible = Cursor.visible;
        previousLockMode = Cursor.lockState;
    }

    private void Start()
    {
        defaultCursorHidden = hideCursorOnStart;
        UpdateCursorState();
    }

    private void OnEnable()
    {
        UpdateCursorState();
    }

    private void OnDisable()
    {
        // Restaurer l'état initial
        Cursor.visible = cursorWasVisible;
        Cursor.lockState = previousLockMode;
    }

    private void Update()
    {
        // Gestion des touches
        if (Input.GetKeyDown(toggleCursorKey))
        {
            defaultCursorHidden = !defaultCursorHidden;
            UpdateCursorState();
        }
        
        if (defaultCursorHidden && Input.GetKeyDown(showCursorKey))
        {
            defaultCursorHidden = false;
            UpdateCursorState();
        }
        
        // Cacher sur clic si activé et aucune commande prioritaire
        if (!temporarilyDisableHideOnClick && !defaultCursorHidden && hideOnClick && 
            (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
        {
            if (activeCommands.Count == 0)
            {
                defaultCursorHidden = true;
                UpdateCursorState();
            }
        }
    }

    /// <summary>
    /// Met à jour l'état du curseur en fonction des commandes actives
    /// </summary>
    private void UpdateCursorState()
    {
        bool shouldBeVisible;
        
        // Si des commandes sont actives, utiliser la commande de plus haute priorité
        if (activeCommands.Count > 0)
        {
            // Prendre la dernière commande (priorité la plus élevée)
            CursorCommand highestPriorityCommand = activeCommands.Values[activeCommands.Count - 1];
            shouldBeVisible = highestPriorityCommand.ShowCursor;
        }
        else
        {
            // Sinon, utiliser l'état par défaut
            shouldBeVisible = !defaultCursorHidden;
        }
        
        // Appliquer l'état
        Cursor.visible = shouldBeVisible;
        Cursor.lockState = shouldBeVisible ? CursorLockMode.None : CursorLockMode.Locked;
        
        // Notifier les observateurs
        OnCursorVisibilityChanged?.Invoke(shouldBeVisible);
    }

    /// <summary>
    /// Ajoute une commande pour contrôler la visibilité du curseur
    /// </summary>
    /// <param name="commandId">Identifiant unique de la commande</param>
    /// <param name="showCursor">True pour afficher le curseur, False pour le cacher</param>
    /// <param name="priority">Priorité de la commande (plus élevé = plus prioritaire)</param>
    /// <returns>Un token pour annuler la commande</returns>
    public CursorCommandToken AddCommand(string commandId, bool showCursor, int priority = 0)
    {
        // Créer une nouvelle commande
        CursorCommand command = new CursorCommand
        {
            Id = commandId,
            ShowCursor = showCursor,
            Priority = priority
        };
        
        // Supprimer toute commande existante avec le même ID
        RemoveCommand(commandId);
        
        // Ajouter la nouvelle commande
        activeCommands.Add(priority, command);
        
        // Mettre à jour l'état du curseur
        UpdateCursorState();
        
        // Retourner un token pour annuler la commande
        return new CursorCommandToken(this, commandId);
    }

    /// <summary>
    /// Supprime une commande par son ID
    /// </summary>
    public void RemoveCommand(string commandId)
    {
        // Chercher et supprimer la commande
        for (int i = 0; i < activeCommands.Count; i++)
        {
            if (activeCommands.Values[i].Id == commandId)
            {
                activeCommands.RemoveAt(i);
                UpdateCursorState();
                return;
            }
        }
    }

    /// <summary>
    /// Définit l'état par défaut du curseur
    /// </summary>
    public void SetDefaultCursorHidden(bool hidden)
    {
        defaultCursorHidden = hidden;
        UpdateCursorState();
    }
    
    /// <summary>
    /// Active/désactive temporairement la fonctionnalité hideOnClick
    /// </summary>
    public void SetHideOnClickEnabled(bool enabled)
    {
        temporarilyDisableHideOnClick = !enabled;
    }
    
    /// <summary>
    /// Vérifie si le curseur est actuellement caché
    /// </summary>
    public bool IsCursorHidden()
    {
        return !Cursor.visible;
    }
}

/// <summary>
/// Structure représentant une commande de curseur
/// </summary>
public struct CursorCommand
{
    public string Id;
    public bool ShowCursor;
    public int Priority;
}

/// <summary>
/// Token permettant d'annuler une commande de curseur
/// </summary>
public class CursorCommandToken : IDisposable
{
    private CursorManager manager;
    private string commandId;
    private bool isDisposed = false;

    public CursorCommandToken(CursorManager manager, string commandId)
    {
        this.manager = manager;
        this.commandId = commandId;
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            manager.RemoveCommand(commandId);
            isDisposed = true;
        }
    }
}
