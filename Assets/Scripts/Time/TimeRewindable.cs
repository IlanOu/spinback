using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that must be added to any object that should be affected by time rewinding.
/// Stores and restores position and rotation states.
/// </summary>
public class TimeRewindable : MonoBehaviour, ITimeRewindable
{
    private class TimeState
    {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public bool isKeyframe;
    }
    
    [Tooltip("Minimum distance an object must move to record a new state (in adaptive mode)")]
    [SerializeField] private float significantMovementThreshold = 0.05f;
    
    [Tooltip("Minimum rotation angle (degrees) to record a new state (in adaptive mode)")]
    [SerializeField] private float significantRotationThreshold = 5f;
    
    private List<TimeState> timeStates = new List<TimeState>();
    private float recordInterval = 0.1f;
    private int maxStates = 3000;
    private bool useAdaptiveRecording = true;
    
    private Vector3 lastRecordedPosition;
    private Quaternion lastRecordedRotation;
    
    private void OnEnable()
    {
        if (TimeRewindManager.Instance != null)
            TimeRewindManager.Instance.RegisterRewindableObject(this);
    }
    
    private void OnDisable()
    {
        if (TimeRewindManager.Instance != null)
            TimeRewindManager.Instance.UnregisterRewindableObject(this);
    }
    
    /// <summary>
    /// Initialize the state recording system with specified parameters
    /// </summary>
    /// <param name="interval">Time interval between state recordings</param>
    /// <param name="adaptiveRecording">Whether to use adaptive recording to save memory</param>
    public void InitializeStateRecording(float interval, bool adaptiveRecording)
    {
        recordInterval = interval;
        useAdaptiveRecording = adaptiveRecording;
        maxStates = Mathf.CeilToInt(300f / recordInterval);
        timeStates.Clear();
        
        // Record initial state as keyframe
        TimeState initialState = new TimeState
        {
            time = 0f,
            position = transform.position,
            rotation = transform.rotation,
            isKeyframe = true
            
        };
        
        timeStates.Add(initialState);
        lastRecordedPosition = transform.position;
        lastRecordedRotation = transform.rotation;
    }
    
    /// <summary>
    /// Remove all states after the specified time point
    /// </summary>
    /// <param name="timePoint">Time point to truncate history after</param>
    public void TruncateHistoryAfter(float timePoint)
    {
        if (timeStates.Count == 0)
            return;
            
        // Find the index of the last state before or equal to the time point
        int lastValidIndex = -1;
        for (int i = 0; i < timeStates.Count; i++)
        {
            if (timeStates[i].time <= timePoint)
                lastValidIndex = i;
            else
                break;
        }
        
        // If we found a valid state, remove all states after it
        if (lastValidIndex >= 0 && lastValidIndex < timeStates.Count - 1)
        {
            timeStates.RemoveRange(lastValidIndex + 1, timeStates.Count - lastValidIndex - 1);
            
            // Ensure the last state has exactly the target time
            // This avoids temporal jumps when resuming recording
            timeStates[lastValidIndex].time = timePoint;
            
            // Update last recorded positions
            lastRecordedPosition = timeStates[lastValidIndex].position;
            lastRecordedRotation = timeStates[lastValidIndex].rotation;
            
            // Mark this state as a keyframe to prevent its elimination
            timeStates[lastValidIndex].isKeyframe = true;
        }
    }
    
    /// <summary>
    /// Record the current state of the object at the specified time
    /// </summary>
    /// <param name="time">Current time to record the state at</param>
    public void RecordState(float time)
    {
        bool shouldRecord = true;
        
        // If using adaptive recording, check if movement is significant
        if (useAdaptiveRecording && timeStates.Count > 0)
        {
            bool significantMovement = Vector3.Distance(transform.position, lastRecordedPosition) > significantMovementThreshold;
            bool significantRotation = Quaternion.Angle(transform.rotation, lastRecordedRotation) > significantRotationThreshold;
            
            shouldRecord = significantMovement || significantRotation;
        }
        
        if (shouldRecord)
        {
            // Check if a state with this exact time already exists (after truncation)
            bool timeExists = false;
            for (int i = 0; i < timeStates.Count; i++)
            {
                if (Mathf.Approximately(timeStates[i].time, time))
                {
                    // Update existing state instead of creating a new one
                    timeStates[i].position = transform.position;
                    timeStates[i].rotation = transform.rotation;
                    timeExists = true;
                    break;
                }
            }
            
            // If time doesn't exist, create a new state
            if (!timeExists)
            {
                TimeState state = new TimeState
                {
                    time = time,
                    position = transform.position,
                    rotation = transform.rotation,
                    isKeyframe = !useAdaptiveRecording || // All states are keyframes if not using adaptive
                                timeStates.Count == 0 || // First state is always a keyframe
                                time - timeStates[timeStates.Count-1].time > recordInterval * 10 // Keyframe every ~1 sec
                };
                
                timeStates.Add(state);
            }
            
            lastRecordedPosition = transform.position;
            lastRecordedRotation = transform.rotation;
            
            // Optimization: remove redundant states
            if (useAdaptiveRecording && timeStates.Count > 3)
            {
                for (int i = timeStates.Count - 3; i > 0; i--)
                {
                    // Don't remove keyframes
                    if (timeStates[i].isKeyframe)
                        continue;
                        
                    // Check if intermediate state is on a straight line
                    Vector3 dirBefore = timeStates[i].position - timeStates[i-1].position;
                    Vector3 dirAfter = timeStates[i+1].position - timeStates[i].position;
                    
                    if (dirBefore.magnitude < 0.001f || dirAfter.magnitude < 0.001f)
                        continue;
                        
                    float dot = Vector3.Dot(dirBefore.normalized, dirAfter.normalized);
                    
                    // If directions are similar and rotation is also similar
                    if (dot > 0.99f && Quaternion.Angle(timeStates[i-1].rotation, timeStates[i].rotation) < 2f &&
                        Quaternion.Angle(timeStates[i].rotation, timeStates[i+1].rotation) < 2f)
                    {
                        timeStates.RemoveAt(i);
                        i--; // Adjust index after removal
                    }
                }
            }
            
            // Limit history size
            while (timeStates.Count > maxStates)
                timeStates.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Rewind the object to its state at the specified time
    /// </summary>
    /// <param name="targetTime">Target time to rewind to</param>
    public void RewindToTime(float targetTime)
    {
        if (timeStates.Count < 2)
            return;
            
        // Binary search to quickly find the two states bracketing the target time
        int low = 0;
        int high = timeStates.Count - 1;
        int indexBefore = 0;
        
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (timeStates[mid].time <= targetTime)
            {
                indexBefore = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        
        int indexAfter = indexBefore + 1;
        if (indexAfter >= timeStates.Count)
            indexAfter = timeStates.Count - 1;
            
        // If indices are identical, no interpolation needed
        if (indexBefore == indexAfter)
        {
            transform.position = timeStates[indexBefore].position;
            transform.rotation = timeStates[indexBefore].rotation;
            return;
        }
        
        // Calculate interpolation factor
        float beforeTime = timeStates[indexBefore].time;
        float afterTime = timeStates[indexAfter].time;
        float t = 0;
        
        if (afterTime > beforeTime) // Avoid division by zero
            t = Mathf.Clamp01((targetTime - beforeTime) / (afterTime - beforeTime));
        
        // Interpolate position and rotation
        transform.position = Vector3.Lerp(timeStates[indexBefore].position, timeStates[indexAfter].position, t);
        transform.rotation = Quaternion.Slerp(timeStates[indexBefore].rotation, timeStates[indexAfter].rotation, t);
    }
    
    /// <summary>
    /// Clear all recorded states
    /// </summary>
    public void ClearStates()
    {
        timeStates.Clear();
    }
}