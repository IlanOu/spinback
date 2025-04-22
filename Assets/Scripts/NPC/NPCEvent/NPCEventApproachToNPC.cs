using UnityEngine;
using UnityEngine.AI;

class NPCEventApproachToNPC : NPCEventInterface
{
    [SerializeField]
    GameObject targetNpc;

    void Start()
    {
        if (!targetNpc.CompareTag("NPC"))
        {
            Debug.LogError("Target must be a NPC");
            this.enabled = false;
        }

        InitMainAgent();
    }
    
    protected override void StartEvent()
    {
        base.StartEvent();

        float distance = Mathf.Max(
                targetNpc.transform.localScale.x, 
                targetNpc.transform.localScale.z
            ) + Mathf.Max(
                transform.localScale.x, 
                transform.localScale.z
            ) * 2f;
        _mainAgent.stoppingDistance = distance;
        _mainAgent.SetDestination(targetNpc.transform.position);
    }
}