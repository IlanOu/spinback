using System;
using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class NPCMovement
{
    public NPCMovementType npcMovementType;
    public float TimeToStart;

    // Useful for this class
    private NPCMovementStrategy _strategy;
    private float canStartMargeTime = 1f;

    // Usefull for other class
    [HideInInspector] public bool launched = false;
    [HideInInspector] public NPCMovementManager Manager;
    [HideInInspector] public bool Enabled;
    
    
    // SPECIFIC FIELDS PER TYPE
    // To see default value, go check NPCMovementDrawer
    // ApproachToNPC
    [SerializeField] private GameObject targetNpc;
    [SerializeField] private float distance = 2f;
    [SerializeField] private GameObject lookTarget;
    
    // Walk
    [SerializeField] private float minWanderDistance = 5f;
    [SerializeField] private float maxWanderDistance = 15f;

    // WalkToLocation
    [SerializeField] private GameObject targetLocation;

    // Talk
    [SerializeField] private AudioClip talkClip;
    
    // LookAtTarget
    [SerializeField] private GameObject lookAtTarget;
    [SerializeField] private float lookAtDuration = 2f;
    
    [Tooltip("Délai (sec) après la fin du précédent si TimeToStart vaut -1")]
    public float delayAfterPrevious = 0f;
    
    public void Init(NPCMovementManager obj)
    {
        Manager = obj;
        Enabled = true;

        _strategy = npcMovementType switch
        {
            NPCMovementType.ApproachToNPC =>
                new NPCMovementApproachToNPC(this, targetNpc, distance, lookTarget),
            NPCMovementType.Walk => new NPCMovementWalk(this, minWanderDistance, maxWanderDistance),
            NPCMovementType.WalkToLocation => new NPCMovementWalkToLocation(this, targetLocation),
            NPCMovementType.Dance => new NPCMovementDance(this),
            NPCMovementType.Talk => new NPCMovementTalk(this, talkClip),   
            NPCMovementType.LookAtTarget   => new NPCMovementLookAtTarget(this, lookAtTarget, lookAtDuration),
            _ => null
        };

        if (_strategy == null)
        {
            Enabled = false;
            Debug.LogWarning($"Strategy not found for {npcMovementType}");
        }
    }

    public bool InRangeToStart(float currentTime)
    {
        if (TimeToStart == -1) return false;
        return MathF.Abs(currentTime - TimeToStart) < canStartMargeTime;
    }

    public void StartMovement()
    {
        if (_strategy == null)
        {
            Debug.LogError("Strategy not initialized");
            return;
        }
        
        if (!Enabled)
        {
            Debug.LogError("Movement not enabled");
            return;
        }
        
        _strategy.StartMovement();
    }

    public void Handle()
    {
        if (_strategy == null || !Enabled) return;
        
        // 1) rien à faire pendant le rewind
        if (TimeRewindManager.Instance.IsRewinding) return;

        float now = TimeRewindManager.Instance.RecordingTime;

        // 2) si pas encore planifié (-1), on attend que le manager le fasse
        if (TimeToStart < 0f) return;

        // 3) pas encore l'heure ?
        if (now < TimeToStart) return;

        // 4) c'est l'heure !
        if (!launched)
        {
            StartMovement();
            launched = true;
            return;
        }

        // 5) on surveille la fin
        if (_strategy.IsDone)
        {
            launched = false;
            Manager.NextMovement();
        }
    }


    private void HandleDestination()
    {
        NavMeshAgent agent = Manager.MainAgent;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            Debug.Log("Arrived");
            agent.ResetPath();
            launched = false;
            Manager.NextMovement();
        }
    }
}
