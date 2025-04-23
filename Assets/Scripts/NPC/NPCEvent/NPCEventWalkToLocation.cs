using UnityEngine;

class NPCEventWalkToLocation : NPCEvent
{
    [SerializeField] GameObject targetLocation;

    protected override void Start()
    {
        base.Start();
        
        if (!targetLocation.CompareTag("Location"))
        {
            Debug.LogError("Target must be a location");
            this.enabled = false;
        }
    }
    
    protected override void StartEvent()
    {
        base.StartEvent();

        _mainAgent.SetDestination(targetLocation.transform.position);
    }
}