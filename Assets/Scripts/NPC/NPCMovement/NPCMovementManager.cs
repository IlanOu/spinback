using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovementManager : MonoBehaviour, ITimeRewindable
{
    [SerializeField] private List<NPCMovement> npcMovements;
    [HideInInspector] public NavMeshAgent MainAgent;
    private int _currentMovementIndex = 0;
    private NPCMovement CurrentMovement => npcMovements.Count == 0 || _currentMovementIndex >= npcMovements.Count ? null : npcMovements[_currentMovementIndex];

    private class TimeState
    {
        public float time;
        public Vector3 velocity;
        public Vector3 destination;
        public float stoppingDistance;
        public int currentMovementIndex;
        public float[] movementStartTimes;
        public bool[] movementWasLaunched;
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
        foreach (NPCMovement npcMovement in npcMovements)
        {
            npcMovement.Init(this);
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
        HandleMovements();  
    }

    void HandleMovements()
    {
        if (CurrentMovement != null) CurrentMovement.Handle();
    }

    public void NextMovement()
    {
        _currentMovementIndex++;
        Debug.Log("Next movement: " + _currentMovementIndex);
    }

    public void InitializeStateRecording(float interval, bool adaptiveRecording)
    {
        recordInterval = interval;
        maxStates = Mathf.CeilToInt(300f / recordInterval);
        timeStates.Clear();

        TimeState initialState = new TimeState
        {
            time = 0f,
            velocity = MainAgent.velocity,
            destination = MainAgent.destination != null ? MainAgent.destination : transform.position,
            stoppingDistance = MainAgent.stoppingDistance,
            currentMovementIndex = _currentMovementIndex,
            movementStartTimes = npcMovements.Select(m => m.TimeToStart).ToArray(),
            movementWasLaunched = npcMovements.Select(m => m.WasLaunched).ToArray()
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
            currentMovementIndex = _currentMovementIndex,
            movementStartTimes = npcMovements.Select(m => m.TimeToStart).ToArray(),
            movementWasLaunched = npcMovements.Select(m => m.WasLaunched).ToArray()
        };

        timeStates.Add(state);

        while (timeStates.Count > maxStates)
            timeStates.RemoveAt(0);
    }

    public void RewindToTime(float targetTime)
    {
        if (timeStates.Count < 2) return;

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
        Vector3 rewindDestination = before.destination;
        float rewindStoppingDistance = before.stoppingDistance;
        int rewindMovementIndex = before.currentMovementIndex;

        _currentMovementIndex = rewindMovementIndex;
        for (int i = 0; i < npcMovements.Count; i++)
        {
            if (i < before.movementStartTimes.Length)
                npcMovements[i].TimeToStart = before.movementStartTimes[i];

            if (i < before.movementWasLaunched.Length)
                npcMovements[i].WasLaunched = before.movementWasLaunched[i];
        }

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