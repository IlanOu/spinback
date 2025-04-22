
using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class NPCEventInterface : MonoBehaviour
{
    [SerializeField]
    public float TimeToStart;
    
    [HideInInspector]
    public bool wasLaunched = false;

    protected NavMeshAgent _mainAgent;
    float margeTime = 0.1f;

    void Start()
    {
        InitMainAgent();
    }

    protected void InitMainAgent()
    {
        _mainAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (CanHandleEvent())
        {
            StartEvent();
        }
    }

    bool InRangeToStart()
    {
        return TimeToStart < Time.time && Time.time < TimeToStart + margeTime;
    }

    protected bool CanHandleEvent()
    {
        return InRangeToStart() && !wasLaunched;
    }

    protected virtual void StartEvent()
    {
        if (!InRangeToStart())
        {
            Debug.LogWarning("Event out of range to start");
            return;
        }

        if (wasLaunched)
        {
            Debug.LogWarning("Event already started");
            return;
        }

        wasLaunched = true;
    }
}
