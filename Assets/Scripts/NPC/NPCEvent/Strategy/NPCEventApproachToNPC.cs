using System.Collections;
using UnityEngine;
using UnityEngine.AI;

class NPCEventApproachToNPC : NPCEventStrategy, INPCEventStrategy
{
    private GameObject _targetNpc;
    private float _distance;

    float stoppingDistance;
    Vector3 targetPosition;

    public NPCEventApproachToNPC(NPCEvent npcEvent, GameObject targetNpc, float distance) : base(npcEvent) 
    {
        if (targetNpc == null)
        {
            Debug.LogError("Missing parameters for NPCEventApproachToNPC");
            this.parent.Enabled = false;
            return;
        }
        
        _targetNpc = targetNpc;
        _distance = distance;

        if (!_targetNpc.CompareTag("NPC"))
        {
            Debug.LogError("Target must be a NPC");
            this.parent.Enabled = false;
            return;
        }
    }

    private IEnumerator FollowTarget(NavMeshAgent mainAgent)
    {
        while (true)
        {
            bool isRewinding = TimeRewindManager.Instance.IsRewinding;
            if (isRewinding)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            if (!isRewinding)
            {
                if (mainAgent.hasPath) yield break;
                if (_targetNpc == null) yield break;

                stoppingDistance = Mathf.Max(
                        _targetNpc.transform.localScale.x,
                        _targetNpc.transform.localScale.z
                    ) + Mathf.Max(
                        parent.Obj.transform.localScale.x,
                        parent.Obj.transform.localScale.z
                    ) + _distance;

                NavMeshAgent targetAgent = _targetNpc.GetComponent<NavMeshAgent>();
                targetPosition = targetAgent != null ? targetAgent.destination : _targetNpc.transform.position;

                mainAgent.stoppingDistance = stoppingDistance;
                mainAgent.SetDestination(targetPosition);

                float distanceToTarget = Vector3.Distance(mainAgent.transform.position, targetPosition);
                if (distanceToTarget <= stoppingDistance)
                {
                    mainAgent.ResetPath();
                    yield break;
                }

                yield return new WaitForSeconds(0.1f);
            }


        }
    }

    public void StartEvent(NavMeshAgent mainAgent) 
    {
        mainAgent.SetDestination(_targetNpc.transform.position);
        parent.Obj.StartCoroutine(FollowTarget(mainAgent));
    }
}