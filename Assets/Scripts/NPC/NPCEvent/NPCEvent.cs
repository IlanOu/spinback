using System;
using NPC.NPCAnimations;
using NPC.NPCAnimations.Strategy;
using UnityEngine;
using UnityEngine.AI;

namespace NPC.NPCEvent
{
    [Serializable]
    public class NPCEvent
    {
        public NPCEventType npcEventType;
        
        // Animations
        public NPCAnimationsType animationType;
    
        public float TimeToStart;

        private INPCEventStrategy _strategy;
        private INPCAnimationStrategy _animationStrategy;

        [HideInInspector] public MonoBehaviour Obj;
        [HideInInspector] public bool Enabled;
        [HideInInspector] public Animator animator; // Référence à l'Animator
        public NPCAnimationManager animationManager; // Gestionnaire d'animations

        // SPECIFIC FIELDS PER TYPE
        [SerializeField] private GameObject targetNpc;
        [SerializeField] private float distance = 2f;
        [SerializeField] private float minWanderDistance = 5f;
        [SerializeField] private float maxWanderDistance = 15f;
        [SerializeField] private GameObject targetLocation;

        public void InitStrategy(MonoBehaviour obj, Animator animator)
        {
            Obj = obj;
            this.animator = animator;
            animationManager = obj.GetComponent<NPCAnimationManager>();

            if (animationManager == null)
            {
                animationManager = obj.gameObject.AddComponent<NPCAnimationManager>();
            }

            Enabled = true;
            switch (npcEventType)
            {
                case NPCEventType.ApproachToNPC:
                    _strategy = new NPCEventApproachToNPC(this, targetNpc, distance);
                    break;
                case NPCEventType.Walk:
                    _strategy = new NPCEventWalk(this, minWanderDistance, maxWanderDistance);
                    break;
                case NPCEventType.WalkToLocation:
                    _strategy = new NPCEventWalkToLocation(this, targetLocation);
                    break;
                case NPCEventType.Dance:
                    _strategy = new NPCEventDance(this);
                    break;
                default:
                    Enabled = false;
                    Debug.LogWarning("Event type not implemented");
                    break;
            }

            switch (animationType)
            {
                case NPCAnimationsType.Idle:
                    // _animationStrategy = new NPCIdleAnimation();
                    break;
                case NPCAnimationsType.Walk:
                    _animationStrategy = new NPCWalkAnimation();
                    break;
                case NPCAnimationsType.Dance:
                    _animationStrategy = new NPCDanceAnimation();
                    break;
                case NPCAnimationsType.Talk:
                    _animationStrategy = new NPCTalkAnimation();
                    break;
                case NPCAnimationsType.Whisper:
                    // _animationStrategy = new NPCWhisperAnimation();
                    break;
                default:
                    Debug.LogError("Animation type non supportée : " + animationType);
                    // _animationStrategy = new NPCIdleAnimation(); // Par défaut
                    break;
            }
        }

        public bool InRangeToStart(float currentTime, float margeTime = 0.1f)
        {
            return Mathf.Abs(currentTime - TimeToStart) < margeTime;
        }

        public void StartEvent(NavMeshAgent mainAgent)
        {
            if (_strategy == null)
            {
                Debug.LogError("Strategy not initialized");
                return;
            }
        
            if (!Enabled)
            {
                Debug.LogError("Event not enabled");
                return;
            }
        
            _strategy.StartEvent(mainAgent);
            _animationStrategy.PlayAnimation(animationManager);
        }

        public void StopEvent()
        {
            _animationStrategy.StopAnimation(animationManager);
        }
    }
}