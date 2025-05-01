using UnityEngine.AI;

public abstract class NPCMovementStrategy : INPCMovementStrategy
{
    protected readonly NPCMovement   npcMovement;
    protected NavMeshAgent MainAgent => npcMovement.Manager.MainAgent;

    protected NPCMovementStrategy(NPCMovement npcMovement)
    {
        this.npcMovement = npcMovement;
    }

    public abstract void StartMovement();
    public abstract bool IsDone { get; }
}