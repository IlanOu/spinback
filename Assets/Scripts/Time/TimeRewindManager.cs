using System.Collections.Generic;
using DefaultNamespace;
using Minis;
using UnityEngine;

/// <summary>
/// Central manager for the time rewind system.
/// Handles recording and rewinding of all rewindable objects.
/// </summary>
public class TimeRewindManager : MonoBehaviour
{
    // Singleton instance
    public static TimeRewindManager Instance { get; private set; }
    
    [Tooltip("Maximum recording time in seconds (5 minutes by default)")]
    [SerializeField] private float maxRecordTime = 300f;
    
    [Tooltip("Time interval between state recordings (in seconds)")]
    [SerializeField] private float recordInterval = 0.1f;
    
    [Tooltip("Mouse scroll sensitivity for time navigation")]
    [SerializeField] private float scrollSensitivity = 0.5f;
    
    [Header("Jog Wheel Settings")]
    [SerializeField] private float jogSensitivity = 0.5f;
    [SerializeField] private float jogInertiaFactor = 0.9f; // Facteur de décélération (0-1)
    [SerializeField] private float jogMinimumVelocity = 0.01f;
    
    
    private float jogVelocity = 0f;
    private bool isJogging = false;
    
    [Tooltip("Use adaptive recording to reduce memory usage")]
    [SerializeField] private bool useAdaptiveRecording = true;
    
    [Tooltip("Maximum objects to process per frame (for performance)")]
    [SerializeField] private int maxObjectsPerFrame = 5;
    
    // Internal state
    private List<TimeRewindable> rewindableObjects = new List<TimeRewindable>();
    private float recordingTime = 0f;
    private float currentPlaybackTime = 0f;
    private bool isRewinding = false;
    private float lastRecordTime = 0f;
    private int currentObjectIndex = 0;
    
    // Properties for external access
    public float RecordingTime => recordingTime;
    public float CurrentPlaybackTime => currentPlaybackTime;
    public bool IsRewinding => isRewinding;
    
    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Find and initialize all rewindable objects in the scene
        rewindableObjects.AddRange(FindObjectsOfType<TimeRewindable>());
        
        foreach (TimeRewindable obj in rewindableObjects)
        {
            if (obj != null)
                obj.InitializeStateRecording(recordInterval, useAdaptiveRecording);
        }
        
        // Enregistrer les bindings MIDI
        MidiBindingRegistry.Instance.Bind(ActionEnum.ScrubTimeline, OnMidiJog);
        MidiBindingRegistry.Instance.Bind(ActionEnum.ToggleRewind, OnMidiRewindToggle);
    }
    
    private void OnMidiRewindToggle(MidiInput input)
    {
        // Si c'est une note ON (Velocity > 0), activer le rewind
        // Si c'est une note OFF (Velocity = 0), désactiver le rewind
        if (input.Velocity > 0)
        {
            // Note ON - Activer le mode rewind s'il n'est pas déjà actif
            if (!isRewinding)
            {
                Debug.Log("MIDI: Activation du mode rewind");
                ActivateRewindMode();
            }
        }
        else
        {
            // Note OFF - Désactiver le mode rewind s'il est actif
            if (isRewinding)
            {
                Debug.Log("MIDI: Désactivation du mode rewind");
                DeactivateRewindMode();
            }
        }
    }
    
    private void OnMidiJog(MidiInput input)
    {
        // Si c'est un message avec une valeur (déplacement du jog)
        if (isRewinding) // Seulement en mode rewind
        {
            // Calcul de la vitesse de déplacement
            float raw = -(input.Value - 64);
            jogVelocity = raw * jogSensitivity;
            isJogging = Mathf.Abs(jogVelocity) > jogMinimumVelocity;
            
            // Application immédiate du déplacement
            if (isJogging)
            {
                float deltaTime = jogVelocity * Time.deltaTime;
                SetPlaybackTime(CurrentPlaybackTime + deltaTime);
            }
        }
    }
    
    private void Update()
    {
        // Toggle rewind mode with spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleRewindMode();
        }

        if (isRewinding)
        {
            // Gestion de l'inertie du jog wheel
            if (Mathf.Abs(jogVelocity) > jogMinimumVelocity)
            {
                // Appliquer l'inertie actuelle
                float deltaTime = jogVelocity * Time.deltaTime;
                SetPlaybackTime(CurrentPlaybackTime + deltaTime);
                
                // Réduire progressivement la vitesse (décélération)
                jogVelocity *= jogInertiaFactor;
            }
            else
            {
                // Arrêter complètement si on est sous le seuil minimum
                jogVelocity = 0f;
                isJogging = false;
            }
            
            // Time navigation using mouse scroll wheel
            float scrollInput = Input.mouseScrollDelta.y * scrollSensitivity;
            if (scrollInput != 0)
            {
                currentPlaybackTime = Mathf.Clamp(currentPlaybackTime - scrollInput, 0f, recordingTime);
            }
            
            // Apply time to objects (distributed across frames)
            ProcessObjectsInRewindMode();
        }
        else // Mode enregistrement
        {
            // En mode normal, réinitialiser la vitesse du jog
            jogVelocity = 0f;
            isJogging = false;
            
            // Record states at regular intervals
            recordingTime += Time.deltaTime;
            if (recordingTime > maxRecordTime)
                recordingTime = maxRecordTime;
                
            if (recordingTime >= lastRecordTime + recordInterval)
            {
                lastRecordTime = recordingTime;
                
                // Record object states (distributed across frames)
                ProcessObjectsInRecordMode();
            }
        }
    }
    
    // Méthode pour activer le mode rewind
    private void ActivateRewindMode()
    {
        isRewinding = true;
        jogVelocity = 0f; // Réinitialiser la vitesse du jog
        isJogging = false;
        currentPlaybackTime = recordingTime;
        Debug.Log("Rewind mode activated. Current time: " + currentPlaybackTime);
    }
    
    // Méthode pour désactiver le mode rewind
    private void DeactivateRewindMode()
    {
        isRewinding = false;
        jogVelocity = 0f; // Réinitialiser la vitesse du jog
        isJogging = false;
        
        // IMPORTANT: Truncate history at resume point
        float oldRecordingTime = recordingTime;
        recordingTime = currentPlaybackTime;
        
        // Inform all objects to truncate their history
        foreach (TimeRewindable obj in rewindableObjects)
        {
            if (obj != null)
                obj.TruncateHistoryAfter(currentPlaybackTime);
        }
        
        // Reset recording time to avoid jumps
        lastRecordTime = recordingTime;
        
        Debug.Log("Rewind mode deactivated. Resuming recording from T=" + recordingTime);
    }
    
    /// <summary>
    /// Process a subset of objects each frame in rewind mode
    /// </summary>
    private void ProcessObjectsInRewindMode()
    {
        int objectsProcessed = 0;
        int startIndex = currentObjectIndex;
        
        while (objectsProcessed < maxObjectsPerFrame && objectsProcessed < rewindableObjects.Count)
        {
            int index = (startIndex + objectsProcessed) % rewindableObjects.Count;
            if (rewindableObjects[index] != null)
                rewindableObjects[index].RewindToTime(currentPlaybackTime);
            
            objectsProcessed++;
        }
        
        currentObjectIndex = (startIndex + objectsProcessed) % rewindableObjects.Count;
    }
    
    /// <summary>
    /// Process a subset of objects each frame in record mode
    /// </summary>
    private void ProcessObjectsInRecordMode()
    {
        int objectsProcessed = 0;
        int startIndex = currentObjectIndex;
        
        while (objectsProcessed < maxObjectsPerFrame && objectsProcessed < rewindableObjects.Count)
        {
            int index = (startIndex + objectsProcessed) % rewindableObjects.Count;
            if (rewindableObjects[index] != null)
                rewindableObjects[index].RecordState(recordingTime);
            
            objectsProcessed++;
        }
        
        currentObjectIndex = (startIndex + objectsProcessed) % rewindableObjects.Count;
    }
    
    /// <summary>
    /// Toggle between rewind and record modes
    /// </summary>
    public void ToggleRewindMode()
    {
        if (!isRewinding)
        {
            ActivateRewindMode();
        }
        else
        {
            DeactivateRewindMode();
        }
    }
    
    /// <summary>
    /// Register a new rewindable object with the manager
    /// </summary>
    /// <param name="obj">Object to register</param>
    public void RegisterRewindableObject(TimeRewindable obj)
    {
        if (!rewindableObjects.Contains(obj))
        {
            rewindableObjects.Add(obj);
            obj.InitializeStateRecording(recordInterval, useAdaptiveRecording);
        }
    }
    
    /// <summary>
    /// Unregister a rewindable object from the manager
    /// </summary>
    /// <param name="obj">Object to unregister</param>
    public void UnregisterRewindableObject(TimeRewindable obj)
    {
        if (rewindableObjects.Contains(obj))
            rewindableObjects.Remove(obj);
    }
    
    /// <summary>
    /// Set the current playback time in rewind mode
    /// </summary>
    /// <param name="time">Target playback time</param>
    public void SetPlaybackTime(float time)
    {
        if (isRewinding)
        {
            currentPlaybackTime = Mathf.Clamp(time, 0f, recordingTime);
        }
    }
    
    /// <summary>
    /// Jump backward in time by the specified amount
    /// </summary>
    /// <param name="seconds">Seconds to jump backward</param>
    public void JumpBackward(float seconds)
    {
        if (isRewinding)
        {
            SetPlaybackTime(currentPlaybackTime - seconds);
        }
    }
    
    /// <summary>
    /// Jump forward in time by the specified amount
    /// </summary>
    /// <param name="seconds">Seconds to jump forward</param>
    public void JumpForward(float seconds)
    {
        if (isRewinding)
        {
            SetPlaybackTime(currentPlaybackTime + seconds);
        }
    }
    
    /// <summary>
    /// Clear all recorded history for all objects
    /// </summary>
    public void ClearAllHistory()
    {
        foreach (TimeRewindable obj in rewindableObjects)
        {
            if (obj != null)
                obj.ClearStates();
        }
        
        recordingTime = 0f;
        currentPlaybackTime = 0f;
        lastRecordTime = 0f;
    }
}