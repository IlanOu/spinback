/* ------------------ NPCMovementDance ------------------ */
using UnityEngine;
using NPC.NPCAnimations;

class NPCMovementSwim : NPCMovementStrategy
{
    private readonly float swimDuration;
    private float timer = 0f;
    private bool launched = false;

    public NPCMovementSwim(NPCMovement npcMovement, float duration = 3f)
        : base(npcMovement)
    {
        swimDuration = duration;
    }

    public override void StartMovement()
    {
        if (launched) return;
        launched = true;
        timer = 0f;

        // Bool ON
        NPCAnimBus.Bool(npcMovement.Manager.gameObject,
            NPCAnimationsType.Swim,
            true);
    }

    public override bool IsDone
    {
        get
        {
            if (!launched) return false;

            timer += Time.deltaTime;

            if (timer >= swimDuration)
            {
                launched = false;

                // Bool OFF  <--  C’était la ligne manquante
                NPCAnimBus.Bool(npcMovement.Manager.gameObject,
                    NPCAnimationsType.Swim,
                    false);

                return true;
            }
            return false;
        }
    }
}