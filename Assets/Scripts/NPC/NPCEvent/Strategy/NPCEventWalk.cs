using UnityEngine;
using UnityEngine.AI;

class NPCEventWalk : NPCEventStrategy, INPCEventStrategy
{
    private float _minWanderDistance;
    private float _maxWanderDistance;
    private Vector3 targetPosition;

    public NPCEventWalk(NPCEvent npcEvent, float minWanderDistance, float maxWanderDistance) : base(npcEvent) 
    {
        _minWanderDistance = minWanderDistance;
        _maxWanderDistance = maxWanderDistance;
        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(_minWanderDistance, _maxWanderDistance);
        targetPosition = randomDirection + base.npcEvent.Manager.transform.position;
    }
    
    public void StartEvent()
    {
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(targetPosition, out hit, _maxWanderDistance, NavMesh.AllAreas))
        {
            _mainAgent.SetDestination(hit.position);
            _mainAgent.stoppingDistance = 0f;
        }
    }
    
    public void StopEvent() {}
}