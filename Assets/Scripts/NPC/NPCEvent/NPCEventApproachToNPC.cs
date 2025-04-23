using UnityEngine;
using UnityEngine.AI;

class NPCEventApproachToNPC : NPCEvent
{
    [SerializeField] GameObject targetNpc;
    [SerializeField] float _distance = 2f;

    float stoppingDistance;
    Vector3 targetPosition;

    protected override void Start()
    {
        base.Start();

        if (!targetNpc.CompareTag("NPC"))
        {
            Debug.LogError("Target must be a NPC");
            this.enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();

        UpdateTargetData();
    }

    void UpdateTargetData()
    {
        stoppingDistance = Mathf.Max(
                targetNpc.transform.localScale.x, 
                targetNpc.transform.localScale.z
            ) + Mathf.Max(
                transform.localScale.x, 
                transform.localScale.z
            ) + _distance;

        NavMeshAgent targetAgent = targetNpc.GetComponent<NavMeshAgent>();
        targetPosition = targetAgent != null ? targetAgent.destination : targetNpc.transform.position;

        if (_mainAgent.destination != null) {
            _mainAgent.stoppingDistance = stoppingDistance;
            _mainAgent.SetDestination(targetPosition);
        }

        if (_mainAgent.remainingDistance <= stoppingDistance) {
            _mainAgent.ResetPath();
        }
    }
    
    protected override void StartEvent()
    {
        base.StartEvent();

        _mainAgent.stoppingDistance = stoppingDistance;
        _mainAgent.SetDestination(targetPosition);
    }
}