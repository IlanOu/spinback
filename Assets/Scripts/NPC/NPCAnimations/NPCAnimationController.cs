using NPC.NPCAnimations;
using UnityEngine;

[RequireComponent(typeof(NPCAnimationManager))]
public class NPCAnimationController : MonoBehaviour
{
    private NPCAnimationManager manager;

    void Start()
    {
        manager = GetComponent<NPCAnimationManager>();
    }

    public void PlayDance() 
    {
        manager.StopAllAnimation();
        manager.PlayAnimation(NPCAnimationsType.Dance);
    }

    public void StopDance() 
    {
        manager.StopAllAnimation();
        manager.StopAnimation(NPCAnimationsType.Dance);
    }

    public void PlayWalk() 
    {
        manager.StopAllAnimation();
        manager.PlayAnimation(NPCAnimationsType.Walk);
    }

    public void StopWalk() 
    {
        manager.StopAllAnimation();
        manager.StopAnimation(NPCAnimationsType.Walk);
    }

    public void PlayIdle() 
    {
        manager.StopAllAnimation();
        manager.PlayAnimation(NPCAnimationsType.Idle);
    }

    public void StopIdle() 
    {
        manager.StopAllAnimation();
        manager.StopAnimation(NPCAnimationsType.Idle);
    }

    public void PlayTalk() 
    {
        manager.StopAllAnimation();
        manager.PlayAnimation(NPCAnimationsType.Talk);
    }

    public void StopTalk() 
    {
        manager.StopAllAnimation();
        manager.StopAnimation(NPCAnimationsType.Talk);
    }

    public void PlayWhisper() 
    {
        manager.StopAllAnimation();
        manager.PlayAnimation(NPCAnimationsType.Whisper);
    }

    public void StopWhisper() 
    {
        manager.StopAllAnimation();
        manager.StopAnimation(NPCAnimationsType.Whisper);
    }

    public void PlayYell() 
    {
        manager.StopAllAnimation();
        manager.PlayAnimation(NPCAnimationsType.Yell);
    }

    public void StopYell() 
    {
        manager.StopAllAnimation();
        manager.StopAnimation(NPCAnimationsType.Yell);
    }

    public void PlaySwim() 
    {
        manager.StopAllAnimation();
        manager.PlayAnimation(NPCAnimationsType.Swim);
    }

    public void StopSwim() 
    {
        manager.StopAllAnimation();
        manager.StopAnimation(NPCAnimationsType.Swim);
   }
}