using System.Collections;
using UnityEngine;
using UnityEngine.AI;

class NPCEventApproachToNPC : NPCEventStrategy, INPCEventStrategy
{
    private GameObject _targetNpc;
    private float _distance;

    private float stoppingDistance;
    private Vector3 targetPosition;
    private Coroutine _currentCoroutine;

    public NPCEventApproachToNPC(NPCEvent npcEvent, GameObject targetNpc, float distance) : base(npcEvent) 
    {
        if (targetNpc == null)
        {
            Debug.LogError("Missing parameters for NPCEventApproachToNPC");
            this.npcEvent.Enabled = false;
            return;
        }
        
        _targetNpc = targetNpc;
        _distance = distance;

        if (!_targetNpc.CompareTag("NPC"))
        {
            Debug.LogError("Target must be a NPC");
            this.npcEvent.Enabled = false;
            return;
        }
    }

    private IEnumerator FollowTarget()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.01f);

            bool isRewinding = TimeRewindManager.Instance.IsRewinding;
            if (isRewinding) continue;

            if (!isRewinding)
            {
                if (_targetNpc == null) yield break;
                if (_mainAgent.destination == null) yield break;

                stoppingDistance = Mathf.Max(
                        _targetNpc.transform.localScale.x,
                        _targetNpc.transform.localScale.z
                    ) + Mathf.Max(
                        npcEvent.Manager.transform.localScale.x,
                        npcEvent.Manager.transform.localScale.z
                    ) + _distance;

                NavMeshAgent targetAgent = _targetNpc.GetComponent<NavMeshAgent>();
                targetPosition = targetAgent != null ? targetAgent.destination : _targetNpc.transform.position;

                _mainAgent.stoppingDistance = stoppingDistance;
                Debug.Log("Set destination");
                _mainAgent.SetDestination(targetPosition);
            }
        }
    }

    public void StartEvent() 
    {
        _mainAgent.SetDestination(_targetNpc.transform.position);
        _currentCoroutine = npcEvent.Manager.StartCoroutine(FollowTarget());
    }

    public void StopEvent()
    {
        npcEvent.Manager.StopCoroutine(_currentCoroutine);
    }
}