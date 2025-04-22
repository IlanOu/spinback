using UnityEngine;

class NPCEventDance : NPCEventInterface
{   
    protected override void StartEvent()
    {
        base.StartEvent();

        Debug.Log("Dancing!");
    }
}