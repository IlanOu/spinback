using UnityEngine;
using UnityEngine.AI;
using NPC.NPCAnimations;

class NPCMovementApproachToNPC : NPCMovementStrategy
{
    private readonly GameObject _targetNpc;
    private readonly GameObject _lookTarget;
    private readonly float      _extraDistance;
    private readonly Transform _visualPivot;
    
    private bool launched      = false;
    private bool arrived       = false;
    private bool rotationDone  = false;

    /* ---------------- ctor ---------------- */
    public NPCMovementApproachToNPC(NPCMovement mov,
                                    GameObject  targetNpc,
                                    float       distance,
                                    GameObject  lookTargetOverride = null)
        : base(mov)
    {
        if (targetNpc == null || !targetNpc.CompareTag("NPC"))
        { Debug.LogError("ApproachToNPC : Target must be a GameObject tagged NPC"); mov.Enabled = false; return; }

        _targetNpc     = targetNpc;
        _lookTarget    = lookTargetOverride != null ? lookTargetOverride : targetNpc;
        _extraDistance = distance;
    }

    /* -------------- Start ---------------- */
    public override void StartMovement()
    {
        if (launched) return;
        launched = true;

        MainAgent.SetDestination(_targetNpc.transform.position);

        float stop =
            Mathf.Max(_targetNpc.transform.localScale.x, _targetNpc.transform.localScale.z) +
            Mathf.Max(npcMovement.Manager.transform.localScale.x, npcMovement.Manager.transform.localScale.z) +
            _extraDistance;

        MainAgent.stoppingDistance = stop;

        NPCAnimBus.Bool(npcMovement.Manager.gameObject, NPCAnimationsType.Walk, true);
    }

    /* -------------- Terminé ? -------------- */
    public override bool IsDone
    {
        get
        {
            /* Étape 1 – Arrivée à la distance voulue */
            if (!arrived)
            {
                arrived = !MainAgent.pathPending &&
                          MainAgent.remainingDistance <= MainAgent.stoppingDistance;

                if (arrived)
                    NPCAnimBus.Bool(npcMovement.Manager.gameObject, NPCAnimationsType.Walk, false);
            }

            /* Étape 2 – rotation vers la cible */
            if (arrived && !rotationDone)
                rotationDone = RotateTowardsTarget();

            return rotationDone;   // true → manager passe au mouvement suivant
        }
    }

    /* ---------- Rotation douce ---------- */
    private bool RotateTowardsTarget()
    {
        if (_lookTarget == null) return true; // rien à regarder

        Transform me  = npcMovement.Manager.transform;
        Vector3   dir = _lookTarget.transform.position - me.position;
        dir.y = 0f;                             // on garde le sol

        if (dir.sqrMagnitude < 0.001f) return true;

        Quaternion desired = Quaternion.LookRotation(dir);
        float angleLeft    = Quaternion.Angle(me.rotation, desired);

        // Slerp pour lisser, 5 rad/sec ~ ajustable
        me.rotation = Quaternion.RotateTowards(me.rotation, desired, 360f * Time.deltaTime);

        return angleLeft < 2f;   // terminé quand l’écart < 2°
    }
}
