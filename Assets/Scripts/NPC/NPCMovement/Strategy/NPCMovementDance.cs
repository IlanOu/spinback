/* ------------------ NPCMovementDance ------------------ */
using UnityEngine;
using NPC.NPCAnimations;

class NPCMovementDance : NPCMovementStrategy
{
    private readonly float danceDuration;
    private float timer = 0f;
    private bool launched = false;

    public NPCMovementDance(NPCMovement npcMovement, float duration = 3f)
        : base(npcMovement)
    {
        danceDuration = duration;
    }

    public override void StartMovement()
    {
        if (launched) return;
        launched = true;
        timer = 0f;

        // Bool ON
        NPCAnimBus.Bool(npcMovement.Manager.gameObject,
            NPCAnimationsType.Dance,
            true);
    }

    public override bool IsDone
    {
        get
        {
            if (!launched) return false;

            timer += Time.deltaTime;

            if (timer >= danceDuration)
            {
                launched = false;

                // Bool OFF  <--  C’était la ligne manquante
                NPCAnimBus.Bool(npcMovement.Manager.gameObject,
                    NPCAnimationsType.Dance,
                    false);

                return true;
            }
            return false;
        }
    }
}