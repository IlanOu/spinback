using UnityEngine;

class NPCMovementDance : NPCMovementStrategy
{
    public NPCMovementDance(NPCMovement npcMovement) : base(npcMovement) {}

    public override void StartMovement()
    {
        Debug.Log("Dancing!");
    }
}