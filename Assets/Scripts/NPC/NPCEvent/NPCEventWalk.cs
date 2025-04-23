using UnityEngine;
using UnityEngine.AI;

class NPCEventWalk : NPCEvent
{
    [SerializeField] float minWanderDistance = 5f;
    [SerializeField] float maxWanderDistance = 15f;
    Vector3 targetPosition;

    protected override void Start()
    {
        base.Start();

        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(minWanderDistance, maxWanderDistance);
        targetPosition = randomDirection + transform.position;
    }
    
    protected override void StartEvent()
    {
        base.StartEvent();

        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(targetPosition, out hit, maxWanderDistance, NavMesh.AllAreas))
        {
            _mainAgent.SetDestination(hit.position);
        }
    }
}