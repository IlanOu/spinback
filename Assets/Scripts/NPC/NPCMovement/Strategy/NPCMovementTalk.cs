
using UnityEngine;
using NPC.NPCAnimations;

class NPCMovementTalk : NPCMovementStrategy
{
    private readonly AudioClip clip;
    private AudioSource source;
    private bool launched;
    private bool finished;

    public NPCMovementTalk(NPCMovement mov, AudioClip clip)
        : base(mov) => this.clip = clip;

    public override void StartMovement()
    {
        if (launched) return;
        launched = true;

        // Animation ON
        NPCAnimBus.Bool(npcMovement.Manager.gameObject,
            NPCAnimationsType.Talk, true);

        // Lecture audio
        source = npcMovement.Manager.GetComponent<AudioSource>();
        if (source == null) source = npcMovement.Manager.gameObject.AddComponent<AudioSource>();

        if (clip != null) source.PlayOneShot(clip);
        else              finished = true;                   // pas de son → fin immédiate
    }

    public override bool IsDone
    {
        get
        {
            if (finished) return true;
            if (!launched) return false;

            bool audioDone = (source == null) || !source.isPlaying;
            if (audioDone)
            {
                // Animation OFF
                NPCAnimBus.Bool(npcMovement.Manager.gameObject,
                    NPCAnimationsType.Talk, false);
                finished = true;
            }
            return finished;
        }
    }
}
