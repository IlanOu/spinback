
using NPC.NPCEvent;

public abstract class NPCEventStrategy
{
    public NPCEvent parent;
    public NPCEventStrategy(NPCEvent npcEvent) 
    {
        parent = npcEvent;
    }
}