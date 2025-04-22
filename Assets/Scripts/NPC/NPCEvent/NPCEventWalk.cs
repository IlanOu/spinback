using UnityEngine;
using UnityEngine.AI;

class NPCEventWalk : NPCEventInterface
{
    [SerializeField] 
    float minWanderDistance = 5f;
    [SerializeField] 
    float maxWanderDistance = 15f;
    
    protected override void StartEvent()
    {
        base.StartEvent();

        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(minWanderDistance, maxWanderDistance);
        randomDirection += transform.position;
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(randomDirection, out hit, maxWanderDistance, NavMesh.AllAreas))
        {
            _mainAgent.SetDestination(hit.position);
        }
    }
}