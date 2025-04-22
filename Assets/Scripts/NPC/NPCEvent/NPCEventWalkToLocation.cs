using UnityEngine;

class NPCEventWalkToLocation : NPCEventInterface
{
    [SerializeField]
    GameObject targetLocation;

    void Start()
    {
        if (!targetLocation.CompareTag("Location"))
        {
            Debug.LogError("Target must be a location");
            this.enabled = false;
        }

        InitMainAgent();
    }
    
    protected override void StartEvent()
    {
        base.StartEvent();

        _mainAgent.SetDestination(targetLocation.transform.position);
    }
}