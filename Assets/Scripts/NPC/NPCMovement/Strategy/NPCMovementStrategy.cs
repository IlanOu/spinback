
using System;
using UnityEngine.AI;

public abstract class NPCMovementStrategy : INPCMovementStrategy
{
    protected NPCMovement npcMovement;
    protected NavMeshAgent _mainAgent { get => npcMovement.Manager.MainAgent; }
    protected Action onComplete;

    public NPCMovementStrategy(NPCMovement npcMovement) 
    {
        this.npcMovement = npcMovement;
    }

    public virtual void StartMovement()
    {
        throw new NotImplementedException();
    }
}