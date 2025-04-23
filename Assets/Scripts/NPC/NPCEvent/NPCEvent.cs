using UnityEngine;
using UnityEngine.AI;

public class NPCEvent : MonoBehaviour
{
    [SerializeField] public float TimeToStart;
    
    protected NavMeshAgent _mainAgent;
    float margeTime = 0.01f;

    bool isLaunched = false;
    protected bool isMoving = false;

    protected virtual void Start()
    {
        _mainAgent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Update()
    {
        if (InRangeToStart())
        {
            if (!isLaunched)
            {
                StartEvent();
            }
        } else if (isLaunched) 
        {
            isLaunched = false;
        }
    }

    bool InRangeToStart()
    {
        float currentTime = TimeRewindManager.Instance.RecordingTime;
        return currentTime > TimeToStart - margeTime && currentTime < TimeToStart + margeTime;
    }

    protected virtual void StartEvent()
    {
        isLaunched = true;
    }
}
