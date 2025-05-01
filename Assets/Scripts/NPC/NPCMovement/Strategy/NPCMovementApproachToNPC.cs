using System.Collections;
using UnityEngine;
using UnityEngine.AI;

class NPCMovementApproachToNPC : NPCMovementStrategy
{
    private GameObject _targetNpc;
    private float _distance;

    public NPCMovementApproachToNPC(NPCMovement npcMovement, GameObject targetNpc, float distance) : base(npcMovement) 
    {
        if (targetNpc == null)
        {
            Debug.LogError("Missing parameters for NPCMovementApproachToNPC");
            this.npcMovement.Enabled = false;
            return;
        }
        
        _targetNpc = targetNpc;
        _distance = distance;

        if (!_targetNpc.CompareTag("NPC"))
        {
            Debug.LogError("Target must be a NPC");
            this.npcMovement.Enabled = false;
            return;
        }
    }

    public override void StartMovement() 
    {
        _mainAgent.SetDestination(_targetNpc.transform.position);

        float stoppingDistance = Mathf.Max(
                _targetNpc.transform.localScale.x,
                _targetNpc.transform.localScale.z
            ) + Mathf.Max(
                npcMovement.Manager.transform.localScale.x,
                npcMovement.Manager.transform.localScale.z
            ) + _distance;
        _mainAgent.stoppingDistance = stoppingDistance;
    }
}