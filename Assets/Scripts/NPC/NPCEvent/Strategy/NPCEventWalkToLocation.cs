using UnityEngine;

class NPCEventWalkToLocation : NPCEventStrategy, INPCEventStrategy
{
    private GameObject _targetLocation;

    public NPCEventWalkToLocation(NPCEvent npcEvent, GameObject targetLocation) : base(npcEvent) 
    {
        if (targetLocation == null)
        {
            Debug.LogError("Missing parameters for NPCEventWalkToLocation");
            this.npcEvent.Enabled = false;
            return;
        }

        _targetLocation = targetLocation;

        if (!targetLocation.CompareTag("Location"))
        {
            Debug.LogError("Target must be a location");
            this.npcEvent.Enabled = false;
            return;
        }
    }
    
    public void StartEvent()
    {
        _mainAgent.SetDestination(_targetLocation.transform.position);
        _mainAgent.stoppingDistance = 0f;
    }

    public void StopEvent() {}
}