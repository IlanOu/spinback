using UnityEngine;

class NPCMovementWalkToLocation : NPCMovementStrategy
{
    private GameObject _targetLocation;

    public NPCMovementWalkToLocation(NPCMovement npcMovement, GameObject targetLocation) : base(npcMovement) 
    {
        if (targetLocation == null)
        {
            Debug.LogError("Missing parameters for NPCMovementWalkToLocation");
            this.npcMovement.Enabled = false;
            return;
        }

        _targetLocation = targetLocation;

        if (!targetLocation.CompareTag("Location"))
        {
            Debug.LogError("Target must be a location");
            this.npcMovement.Enabled = false;
            return;
        }
    }
    
    public override void StartMovement()
    {
        Debug.Log("Walking to location");
        _mainAgent.SetDestination(_targetLocation.transform.position);
        _mainAgent.stoppingDistance = 0f;
    }
}