using UnityEngine;

class NPCEventDance : NPCEventStrategy, INPCEventStrategy
{
    public NPCEventDance(NPCEvent npcEvent) : base(npcEvent) {}

    public void StartEvent()
    {
        Debug.Log("Dancing!");
    }
    
    public void StopEvent() {}
}