using UnityEngine;

class NPCEventDance : NPCEvent
{   
    protected override void StartEvent()
    {
        base.StartEvent();

        Debug.Log("Dancing!");
    }
}