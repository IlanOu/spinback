/* --------------- NPCMovementApproachToNPC ---------------- */
using UnityEngine;
using UnityEngine.AI;
using NPC.NPCAnimations;      // enum + bus

class NPCMovementApproachToNPC : NPCMovementStrategy
{
    private readonly GameObject _targetNpc;
    private readonly float      _extraDistance;
    private bool   launched = false;

    public NPCMovementApproachToNPC(NPCMovement npcMovement,
        GameObject  targetNpc,
        float       distance)
        : base(npcMovement)
    {
        if (targetNpc == null) { Debug.LogError("Target NPC null"); npcMovement.Enabled = false; return; }
        if (!targetNpc.CompareTag("NPC")) { Debug.LogError("Target must be tagged NPC"); npcMovement.Enabled = false; return; }

        _targetNpc      = targetNpc;
        _extraDistance  = distance;
    }

    public override void StartMovement()
    {
        if (launched) return;
        launched = true;

        MainAgent.SetDestination(_targetNpc.transform.position);

        float stop = Mathf.Max(_targetNpc.transform.localScale.x, _targetNpc.transform.localScale.z) +
                     Mathf.Max(npcMovement.Manager.transform.localScale.x, npcMovement.Manager.transform.localScale.z) +
                     _extraDistance;
        MainAgent.stoppingDistance = stop;

        NPCAnimBus.Bool(npcMovement.Manager.gameObject, NPCAnimationsType.Walk, true);
    }

    public override bool IsDone
    {
        get
        {
            bool finished = !MainAgent.pathPending &&
                            MainAgent.remainingDistance <= MainAgent.stoppingDistance;

            if (finished && launched)
            {
                NPCAnimBus.Bool(npcMovement.Manager.gameObject, NPCAnimationsType.Walk, false);
                launched = false;
            }
            return finished;
        }
    }
}