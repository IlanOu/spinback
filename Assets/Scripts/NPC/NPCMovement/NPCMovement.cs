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
    [HideInInspector] public bool WasLaunched;
    [HideInInspector] public NPCMovementManager Manager;
    [HideInInspector] public bool Enabled;

    // SPECIFIC FIELDS PER TYPE
    // To see default value, go check NPCMovementDrawer
    // ApproachToNPC
    [SerializeField] private GameObject targetNpc;
    [SerializeField] private float distance = 2f;

    // Walk
    [SerializeField] private float minWanderDistance = 5f;
    [SerializeField] private float maxWanderDistance = 15f;

    // WalkToLocation
    [SerializeField] private GameObject targetLocation;

    public void Init(NPCMovementManager obj)
    {
        Manager = obj;
        WasLaunched = false;
        Enabled = true;

        _strategy = npcMovementType switch
        {
            NPCMovementType.ApproachToNPC => new NPCMovementApproachToNPC(this, targetNpc, distance),
            NPCMovementType.Walk => new NPCMovementWalk(this, minWanderDistance, maxWanderDistance),
            NPCMovementType.WalkToLocation => new NPCMovementWalkToLocation(this, targetLocation),
            NPCMovementType.Dance => new NPCMovementDance(this),
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
        
        WasLaunched = true;
        _strategy.StartMovement();
    }

    public void Handle()
    {
        float currentTime = TimeRewindManager.Instance.RecordingTime;
        bool isRewinding = TimeRewindManager.Instance.IsRewinding;

        if (isRewinding) return;

        if (TimeToStart == -1) TimeToStart = currentTime;

        if (currentTime >= TimeToStart)
        {
            if (!WasLaunched) {
                StartMovement();
            }
            
            if (WasLaunched) HandleDestination();
        }

    }

    private void HandleDestination()
    {
        NavMeshAgent agent = Manager.MainAgent;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            Debug.Log("Arrived");
            agent.ResetPath();
            WasLaunched = false;
            Manager.NextMovement();
        }
    }
}
