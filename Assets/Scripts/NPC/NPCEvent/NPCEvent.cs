using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class NPCEvent
{
    public NPCEventType npcEventType;
    public float TimeToStart;

    private INPCEventStrategy _strategy;

    [HideInInspector] public MonoBehaviour Obj;
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

    public void InitStrategy(MonoBehaviour obj)
    {
        Obj = obj;
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

    public bool InRangeToStart(float currentTime, float margeTime = 0.1f)
    {
        return Mathf.Abs(currentTime - TimeToStart) < margeTime;
    }

    public void StartEvent(NavMeshAgent mainAgent)
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
        
        _strategy.StartEvent(mainAgent);
        return;
    }
}
