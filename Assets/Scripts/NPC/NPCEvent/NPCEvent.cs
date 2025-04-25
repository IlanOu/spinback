using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class NPCEvent
{
    public NPCEventType npcEventType;
    public float TimeToStart;

    // Useful for this class
    private INPCEventStrategy _strategy;
    private bool wasLaunched = false;
    private float canStartMargeTime = 1f;

    // Usefull for other class
    [HideInInspector] public NPCEventManager Manager;
    [HideInInspector] public bool Enabled;

    // SPECIFIC FIELDS PER TYPE
    // To see default value, go check NPCEventDrawer
    // ApproachToNPC
    [SerializeField] private GameObject targetNpc;
    [SerializeField] private float distance = 2f;

    // Walk
    [SerializeField] private float minWanderDistance = 5f;
    [SerializeField] private float maxWanderDistance = 15f;

    // WalkToLocation
    [SerializeField] private GameObject targetLocation;

    public void InitStrategy(NPCEventManager obj)
    {
        Manager = obj;
        Enabled = true;
        switch (npcEventType)
        {
            case NPCEventType.ApproachToNPC:
                _strategy = new NPCEventApproachToNPC(this, targetNpc, distance);
                break;
            case NPCEventType.Walk:
                _strategy = new NPCEventWalk(this, minWanderDistance, maxWanderDistance);
                break;
            case NPCEventType.WalkToLocation:
                _strategy = new NPCEventWalkToLocation(this, targetLocation);
                break;
            case NPCEventType.Dance:
                _strategy = new NPCEventDance(this);
                break;
            default:
                Enabled = false;
                Debug.LogWarning("Event type not implemented");
                break;
        }
    }

    public bool InRangeToStart(float currentTime)
    {
        if (TimeToStart == -1) return false;
        return MathF.Abs(currentTime - TimeToStart) < canStartMargeTime;
    }

    // public bool CanStart(float currentTime)
    // {
    //     bool canStart = TimeToStart == -1 || InRangeToStart(currentTime);
    //     Debug.Log("CanStart: " + canStart + " isLaunching: " + _launching);
    //     return canStart && !_launching && Enabled;
    // }

    public void StartEvent()
    {
        if (_strategy == null)
        {
            Debug.LogError("Strategy not initialized");
            return;
        }
        
        if (!Enabled)
        {
            Debug.LogError("Event not enabled");
            return;
        }
        wasLaunched = true;
        _strategy.StartEvent();
    }

    public void Handle()
    {
        float currentTime = TimeRewindManager.Instance.RecordingTime;

        if (TimeToStart == -1) TimeToStart = currentTime;

        if (currentTime >= TimeToStart)
        {
            Debug.Log("Type: " + npcEventType);
            if (!wasLaunched) StartEvent();

            HandleDestination();
        }        
    }

    public void StopEvent()
    {
        _strategy.StopEvent();
    }

    private void HandleDestination()
    {
        if (Manager.MainAgent.destination == null) return;

        if (Manager.MainAgent.remainingDistance <= Manager.MainAgent.stoppingDistance)
        {
            Manager.MainAgent.ResetPath();
            _strategy.StopEvent();
            wasLaunched = false;
            Manager.NextEvent();
        }
    }

    private IEnumerator StopLaunchingAfterStartingEvent()
    {
        yield return new WaitForSeconds(canStartMargeTime);
        // _launching = false;
    }
}
