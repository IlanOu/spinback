
using UnityEngine.AI;

public abstract class NPCEventStrategy
{
    protected NPCEvent npcEvent;
    protected NavMeshAgent _mainAgent { get => npcEvent.Manager.MainAgent; }
    public NPCEventStrategy(NPCEvent npcEvent) 
    {
        this.npcEvent = npcEvent;
    }
}