using UnityEngine;
using UnityEngine.AI;

class NPCEventWalk : NPCEventStrategy, INPCEventStrategy
{
    private float _minWanderDistance;
    private float _maxWanderDistance;
    Vector3 targetPosition;

    public NPCEventWalk(NPCEvent npcEvent, float minWanderDistance, float maxWanderDistance) : base(npcEvent) 
    {
        _minWanderDistance = minWanderDistance;
        _maxWanderDistance = maxWanderDistance;
        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(_minWanderDistance, _maxWanderDistance);
        targetPosition = randomDirection + parent.Obj.transform.position;
    }
    
    public void StartEvent(NavMeshAgent mainAgent)
    {
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(targetPosition, out hit, _maxWanderDistance, NavMesh.AllAreas))
        {
            mainAgent.SetDestination(hit.position);
        }
    }
}