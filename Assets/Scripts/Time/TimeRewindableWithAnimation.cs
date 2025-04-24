using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension of TimeRewindable that also records and restores Animator states.
/// </summary>
[RequireComponent(typeof(Animator))]
public class TimeRewindableWithAnimation : MonoBehaviour, ITimeRewindable
{
    private class AnimatorStateSnapshot
    {
        public int stateHash;
        public float normalizedTime;
        public int layerIndex;
    }

    private class TimeState
    {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public bool isKeyframe;

        public Dictionary<string, float> floatParams = new();
        public Dictionary<string, int> intParams = new();
        public Dictionary<string, bool> boolParams = new();
        public List<AnimatorStateSnapshot> animatorStates = new();
    }

    [SerializeField] private float significantMovementThreshold = 0.05f;
    [SerializeField] private float significantRotationThreshold = 5f;

    private List<TimeState> timeStates = new();
    private float recordInterval = 0.1f;
    private int maxStates = 3000;
    private bool useAdaptiveRecording = true;

    private Vector3 lastRecordedPosition;
    private Quaternion lastRecordedRotation;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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
        useAdaptiveRecording = adaptiveRecording;
        maxStates = Mathf.CeilToInt(300f / recordInterval);
        timeStates.Clear();

        var initialState = CreateTimeState(0f);
        initialState.isKeyframe = true;

        timeStates.Add(initialState);
        lastRecordedPosition = transform.position;
        lastRecordedRotation = transform.rotation;
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
            lastRecordedPosition = timeStates[lastValidIndex].position;
            lastRecordedRotation = timeStates[lastValidIndex].rotation;
            timeStates[lastValidIndex].isKeyframe = true;
        }
    }

    public void RecordState(float time)
    {
        bool shouldRecord = true;

        if (useAdaptiveRecording && timeStates.Count > 0)
        {
            bool moved = Vector3.Distance(transform.position, lastRecordedPosition) > significantMovementThreshold;
            bool rotated = Quaternion.Angle(transform.rotation, lastRecordedRotation) > significantRotationThreshold;
            shouldRecord = moved || rotated;
        }

        if (shouldRecord)
        {
            bool timeExists = false;
            for (int i = 0; i < timeStates.Count; i++)
            {
                if (Mathf.Approximately(timeStates[i].time, time))
                {
                    UpdateTimeState(timeStates[i]);
                    timeExists = true;
                    break;
                }
            }

            if (!timeExists)
            {
                var state = CreateTimeState(time);
                timeStates.Add(state);
            }

            lastRecordedPosition = transform.position;
            lastRecordedRotation = transform.rotation;

            if (useAdaptiveRecording && timeStates.Count > 3)
            {
                for (int i = timeStates.Count - 3; i > 0; i--)
                {
                    if (timeStates[i].isKeyframe) continue;

                    Vector3 dirBefore = timeStates[i].position - timeStates[i - 1].position;
                    Vector3 dirAfter = timeStates[i + 1].position - timeStates[i].position;

                    if (dirBefore.magnitude < 0.001f || dirAfter.magnitude < 0.001f) continue;

                    float dot = Vector3.Dot(dirBefore.normalized, dirAfter.normalized);

                    if (dot > 0.99f &&
                        Quaternion.Angle(timeStates[i - 1].rotation, timeStates[i].rotation) < 2f &&
                        Quaternion.Angle(timeStates[i].rotation, timeStates[i + 1].rotation) < 2f)
                    {
                        timeStates.RemoveAt(i);
                        i--;
                    }
                }
            }

            while (timeStates.Count > maxStates)
                timeStates.RemoveAt(0);
        }
    }

    public void RewindToTime(float targetTime)
    {
        if (timeStates.Count < 2) return;

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

        int indexAfter = Mathf.Min(indexBefore + 1, timeStates.Count - 1);

        if (indexBefore == indexAfter)
        {
            ApplyTimeState(timeStates[indexBefore]);
            return;
        }

        TimeState before = timeStates[indexBefore];
        TimeState after = timeStates[indexAfter];
        float t = (after.time > before.time) ? Mathf.Clamp01((targetTime - before.time) / (after.time - before.time)) : 0f;

        transform.position = Vector3.Lerp(before.position, after.position, t);
        transform.rotation = Quaternion.Slerp(before.rotation, after.rotation, t);
        ApplyAnimatorState(before); // Or Lerp between before/after if needed
    }
    
    private void ApplyTimeState(TimeState state)
    {
        transform.position = state.position;
        transform.rotation = state.rotation;
        ApplyAnimatorState(state);
    }

    public void ClearStates()
    {
        timeStates.Clear();
    }

    private TimeState CreateTimeState(float time)
    {
        var state = new TimeState
        {
            time = time,
            position = transform.position,
            rotation = transform.rotation,
            isKeyframe = !useAdaptiveRecording ||
                         timeStates.Count == 0 ||
                         time - timeStates[timeStates.Count - 1].time > recordInterval * 10
        };

        RecordAnimatorState(state);
        return state;
    }

    private void UpdateTimeState(TimeState state)
    {
        state.position = transform.position;
        state.rotation = transform.rotation;
        RecordAnimatorState(state);
    }

    private void RecordAnimatorState(TimeState state)
    {
        state.floatParams.Clear();
        state.intParams.Clear();
        state.boolParams.Clear();
        state.animatorStates.Clear();

        foreach (var param in animator.parameters)
        {
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    state.floatParams[param.name] = animator.GetFloat(param.name);
                    break;
                case AnimatorControllerParameterType.Int:
                    state.intParams[param.name] = animator.GetInteger(param.name);
                    break;
                case AnimatorControllerParameterType.Bool:
                    state.boolParams[param.name] = animator.GetBool(param.name);
                    break;
            }
        }

        for (int i = 0; i < animator.layerCount; i++)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(i);
            state.animatorStates.Add(new AnimatorStateSnapshot
            {
                stateHash = info.shortNameHash,
                normalizedTime = info.normalizedTime,
                layerIndex = i
            });
        }
    }

    private void ApplyAnimatorState(TimeState state)
    {
        foreach (var kv in state.floatParams)
            animator.SetFloat(kv.Key, kv.Value);

        foreach (var kv in state.intParams)
            animator.SetInteger(kv.Key, kv.Value);

        foreach (var kv in state.boolParams)
            animator.SetBool(kv.Key, kv.Value);

        foreach (var snapshot in state.animatorStates)
        {
            animator.Play(snapshot.stateHash, snapshot.layerIndex, snapshot.normalizedTime);
        }

        animator.Update(0f); // Force update
    }
}
