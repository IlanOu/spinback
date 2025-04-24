using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentTimeRewindable : MonoBehaviour, ITimeRewindable
{
    private class TimeState
    {
        public float time;
        public Vector3 velocity;
        public Vector3 destination;
        public float stoppingDistance;
    }

    private List<TimeState> timeStates = new List<TimeState>();
    private float recordInterval = 0.1f;
    private int maxStates = 3000;

    private NavMeshAgent _mainAgent;

    void Start()
    {
        _mainAgent = GetComponent<NavMeshAgent>();
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

    public void InitializeStateRecording(float interval, bool adaptiveRecording)
    {
        recordInterval = interval;
        maxStates = Mathf.CeilToInt(300f / recordInterval);
        timeStates.Clear();

        // Enregistrer un état initial
        TimeState initialState = new TimeState
        {
            time = 0f,
            velocity = _mainAgent.velocity,
            destination = _mainAgent.hasPath ? _mainAgent.destination : transform.position,
            stoppingDistance = _mainAgent.stoppingDistance
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
        if (_mainAgent == null || !_mainAgent.isOnNavMesh) return;

        TimeState state = new TimeState
        {
            time = time,
            velocity = _mainAgent.velocity,
            destination = _mainAgent.hasPath ? _mainAgent.destination : transform.position,
            stoppingDistance = _mainAgent.stoppingDistance
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

        if (_mainAgent != null && _mainAgent.isOnNavMesh)
        {
            _mainAgent.ResetPath();
            _mainAgent.velocity = rewindVelocity;
            if (rewindDestination != transform.position)
            {
                _mainAgent.SetDestination(rewindDestination);
            }
            _mainAgent.stoppingDistance = rewindStoppingDistance;
        }
    }

    public void ClearStates()
    {
        timeStates.Clear();
    }
}