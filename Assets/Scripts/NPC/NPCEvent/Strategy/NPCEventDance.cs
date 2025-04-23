using UnityEngine;
using UnityEngine.AI;

class NPCEventDance : NPCEventStrategy, INPCEventStrategy
{
    public NPCEventDance(NPCEvent npcEvent) : base(npcEvent) {}

    public void StartEvent(NavMeshAgent mainAgent)
    {
        Debug.Log("Dancing!");
    }
}