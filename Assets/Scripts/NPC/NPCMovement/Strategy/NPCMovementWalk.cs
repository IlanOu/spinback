using UnityEngine;
using UnityEngine.AI;

class NPCMovementWalk : NPCMovementStrategy
{
    private float _minWanderDistance;
    private float _maxWanderDistance;
    private Vector3 targetPosition;

    public NPCMovementWalk(NPCMovement npcMovement, float minWanderDistance, float maxWanderDistance) : base(npcMovement) 
    {
        _minWanderDistance = minWanderDistance;
        _maxWanderDistance = maxWanderDistance;
        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(_minWanderDistance, _maxWanderDistance);
        targetPosition = randomDirection + base.npcMovement.Manager.transform.position;
    }
    
    public override void StartMovement()
    {
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(targetPosition, out hit, _maxWanderDistance, NavMesh.AllAreas))
        {
            _mainAgent.SetDestination(hit.position);
            _mainAgent.stoppingDistance = 0f;
        }
    }
}