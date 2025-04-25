using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEventManager : MonoBehaviour, ITimeRewindable
{
    [SerializeField] private List<NPCEvent> npcEvents;
    [HideInInspector] public NavMeshAgent MainAgent;
    public int _currentEventIndex = 0;
    private bool isInRewindMode = false;

    // Use full for rewindable
    private class TimeState
    {
        public float time;
        public Vector3 velocity;
        public Vector3 destination;
        public float stoppingDistance;
        public int currentEventIndex;
    }
    private List<TimeState> timeStates = new List<TimeState>();
    private float recordInterval = 0.1f;
    private int maxStates = 3000;

    void Awake()
    {
        if (MainAgent == null)
            MainAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        foreach (NPCEvent npcEvent in npcEvents)
        {
            npcEvent.InitStrategy(this);
        }
    }

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

    void Update()
    {
        ChangeRewindMode();
        HandleEvents();  
    }

    void ChangeRewindMode()
    {
        bool isRewinding = TimeRewindManager.Instance.IsRewinding;
        float currentTime = TimeRewindManager.Instance.RecordingTime;
        NPCEvent currentEvent = npcEvents.Count > 0 ? npcEvents[_currentEventIndex] : null;
        // Passed to normal mode
        if (!isRewinding && isInRewindMode)
        {
            isInRewindMode = false;
            // Debug.Log("Actual index: " + _currentEventIndex);
            // if (currentEvent != null)
            // {
            //     Debug.Log("TimeToStart: " + currentEvent.TimeToStart + " CanStart: " + currentEvent.CanStart(currentTime));
            //     if (currentEvent.CanStart(currentTime))
            //     {
            //         currentEvent.StartEvent(currentTime);
            //     }
            // }
        }

        // Passed to rewind mode
        if (isRewinding && !isInRewindMode)
        {
            isInRewindMode = true;
        }
    }

    void HandleEvents()
    {
        if (npcEvents.Count > 0)
        {
            NPCEvent currentEvent = npcEvents[_currentEventIndex];
            currentEvent.Handle();
        }
    }

    public void NextEvent()
    {
        if (npcEvents.Count > _currentEventIndex + 1)
        {
            _currentEventIndex++;
            Debug.Log("Next event: " + _currentEventIndex);
        }
    }

    private IEnumerator WaitForNextEvent()
    {
        yield return new WaitForSeconds(recordInterval);
    }

    public void InitializeStateRecording(float interval, bool adaptiveRecording)
    {
        recordInterval = interval;
        maxStates = Mathf.CeilToInt(300f / recordInterval);
        timeStates.Clear();

        // Enregistrer un état initial
        TimeState initialState = new TimeState
        {
            time = 0f,
            velocity = MainAgent.velocity,
            destination = MainAgent.destination != null ? MainAgent.destination : transform.position,
            stoppingDistance = MainAgent.stoppingDistance,
            currentEventIndex = _currentEventIndex
        };

        timeStates.Add(initialState);
    }

    public void TruncateHistoryAfter(float timePoint)
    {
        if (timeStates.Count == 0) return;

        int lastValidIndex = -1;
        for (int i = 0; i < timeStates.Count; i++)
        {
            if (timeStates[i].time <= timePoint)
                lastValidIndex = i;
            else
                break;
        }

        if (lastValidIndex >= 0 && lastValidIndex < timeStates.Count - 1)
        {
            timeStates.RemoveRange(lastValidIndex + 1, timeStates.Count - lastValidIndex - 1);
            timeStates[lastValidIndex].time = timePoint;
        }
    }

    public void RecordState(float time)
    {
        if (MainAgent == null || !MainAgent.isOnNavMesh) return;

        TimeState state = new TimeState
        {
            time = time,
            velocity = MainAgent.velocity,
            destination = MainAgent.hasPath ? MainAgent.destination : transform.position,
            stoppingDistance = MainAgent.stoppingDistance,
            currentEventIndex = _currentEventIndex
        };

        timeStates.Add(state);

        // Limite du nombre d'états
        while (timeStates.Count > maxStates)
            timeStates.RemoveAt(0);
    }

    public void RewindToTime(float targetTime)
    {
        if (timeStates.Count < 2) return;

        // Recherche binaire
        int low = 0, high = timeStates.Count - 1, indexBefore = 0;
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

        int indexAfter = Mathf.Min(indexBefore + 1, timeStates.Count - 1);
        TimeState before = timeStates[indexBefore];
        TimeState after = timeStates[indexAfter];

        float t = (after.time > before.time) ? Mathf.Clamp01((targetTime - before.time) / (after.time - before.time)) : 0f;

        Vector3 rewindVelocity = Vector3.Lerp(before.velocity, after.velocity, t);
        Vector3 rewindDestination = Vector3.Lerp(before.destination, after.destination, t);

        float rewindStoppingDistance = Mathf.Lerp(before.stoppingDistance, after.stoppingDistance, t);
        int rewindEventIndex = before.currentEventIndex;

        _currentEventIndex = rewindEventIndex;
        Debug.Log("Set index: " + rewindEventIndex);
        if (MainAgent != null && MainAgent.isOnNavMesh)
        {
            MainAgent.ResetPath();
            MainAgent.velocity = rewindVelocity;
            if (rewindDestination != transform.position)
            {
                MainAgent.SetDestination(rewindDestination);
            }
            MainAgent.stoppingDistance = rewindStoppingDistance;
        }
    }

    public void ClearStates()
    {
        timeStates.Clear();
    }
}