namespace NPC.NPCAnimations.Strategy
{
    public interface INPCAnimationStrategy
    {
        void PlayAnimation(NPCAnimationManager animationManager);
        void StopAnimation(NPCAnimationManager animationManager);
    }
}